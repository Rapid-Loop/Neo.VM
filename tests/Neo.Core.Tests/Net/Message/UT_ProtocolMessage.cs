// BSD 2-Clause License
//
// Copyright (c) 2026, Rapid Loop
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES

using Neo.Core.Extensions;
using Neo.Core.Net;
using Neo.Core.Net.Message;
using Neo.Core.Serialization;
using System;
using System.Buffers.Binary;
using System.IO;

namespace Neo.Core.Tests.Net.Message
{
    [TestClass]
    public class UT_ProtocolMessage
    {
        [TestMethod]
        public void TestTryReadIncompleteHeader()
        {
            // Only flags + command; missing the first VarSize byte.
            ReadOnlySpan<byte> data = [(byte)ProtocolMessageFlags.None, (byte)ProtocolMessageCommand.VersionAck];

            var consumed = ProtocolMessage.TryRead(data, out var message);

            Assert.AreEqual(0, consumed);
            Assert.IsNull(message);
        }

        [TestMethod]
        public void TestTryReadIncompletePayload()
        {
            // Declares 5 payload bytes, supplies only 2.
            ReadOnlySpan<byte> data =
            [
                (byte)ProtocolMessageFlags.None,
                (byte)ProtocolMessageCommand.Version,
                0x05, // compact length = 5
                0x01, 0x02, // only 2 of 5
            ];

            var consumed = ProtocolMessage.TryRead(data, out var message);

            Assert.AreEqual(0, consumed);
            Assert.IsNull(message);
        }

        [TestMethod]
        public void TestTryReadEmptyPayloadVersionAck()
        {
            // Neo Verack has no payload: 00 01 00
            ReadOnlySpan<byte> data =
            [
                (byte)ProtocolMessageFlags.None,
                (byte)ProtocolMessageCommand.VersionAck,
                0x00, // length 0
            ];

            var consumed = ProtocolMessage.TryRead(data, out var message);

            Assert.AreEqual(3, consumed);
            Assert.IsNotNull(message);
            Assert.AreEqual(ProtocolMessageFlags.None, message.Flags);
            Assert.AreEqual(ProtocolMessageCommand.VersionAck, message.Command);
            Assert.IsFalse(message.Compressed);
            Assert.HasCount(0, message.PayloadBytes);
            Assert.IsNull(message.Message);
        }

        [TestMethod]
        public void TestTryReadLeavesTrailingBytes()
        {
            // Framing only: use a command with no typed payload decoder yet.
            // One full frame (3 header + 2 payload) + leftover bytes for the next frame.
            byte[] buffer =
            [
                (byte)ProtocolMessageFlags.None,
                (byte)ProtocolMessageCommand.GetAddress,
                0x02,       // length 2
                0xAA, 0xBB, // payload
                0xFF, 0xEE, // NOT part of this frame
            ];

            var consumed = ProtocolMessage.TryRead(buffer, out var message);

            Assert.AreEqual(5, consumed);
            Assert.IsNotNull(message);
            Assert.AreEqual(ProtocolMessageCommand.GetAddress, message.Command);
            Assert.AreSequenceEqual(new byte[] { 0xAA, 0xBB }, message.PayloadBytes.ToArray());
            Assert.IsNull(message.Message);

            // Caller would next call TryRead on buffer[consumed..] for the next frame.
            var remaining = buffer.AsSpan(consumed);
            Assert.AreSequenceEqual(new byte[] { 0xFF, 0xEE }, remaining.ToArray());
        }

        [TestMethod]
        public void TestTryReadUInt16LengthPrefix()
        {
            // Length uses 0xFD marker + 2 LE bytes (length = 0x0100 = 256).
            var payload = new byte[256];
            payload[0] = 0x42;
            payload[^1] = 0x24;

            var buffer = new byte[5 + payload.Length];
            buffer[0] = (byte)ProtocolMessageFlags.None;
            buffer[1] = (byte)ProtocolMessageCommand.Address;
            buffer[2] = 0xfd;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(3, 2), 256);
            payload.CopyTo(buffer.AsSpan(5));

            var consumed = ProtocolMessage.TryRead(buffer, out var message);

            Assert.AreEqual(5 + 256, consumed);
            Assert.IsNotNull(message);
            Assert.HasCount(256, message.PayloadBytes);
            Assert.AreEqual(0x42, message.PayloadBytes.Span[0]);
            Assert.AreEqual(0x24, message.PayloadBytes.Span[^1]);
        }

        [TestMethod]
        public void TestTryReadPayloadLengthExceedsMax()
        {
            // 0xFE + 4-byte length claiming PayloadMaxSize + 1.
            var buffer = new byte[7];
            buffer[0] = (byte)ProtocolMessageFlags.None;
            buffer[1] = (byte)ProtocolMessageCommand.Block;
            buffer[2] = 0xfe;
            BinaryPrimitives.WriteUInt32LittleEndian(
                buffer.AsSpan(3, 4),
                (uint)ProtocolMessage.PayloadMaxSize + 1);

            Assert.ThrowsExactly<FormatException>(() =>
                ProtocolMessage.TryRead(buffer, out _));
        }

        [TestMethod]
        public void TestCreateVersionAckRoundTrip()
        {
            var expected = ProtocolMessage.Create(ProtocolMessageCommand.VersionAck);
            var bytes = expected.ToArray();

            var consumed = ProtocolMessage.TryRead(bytes, out var actual);

            Assert.AreEqual(bytes.Length, consumed);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ProtocolMessageCommand.VersionAck, actual.Command);
            Assert.AreEqual(ProtocolMessageFlags.None, actual.Flags);
            Assert.HasCount(0, actual.PayloadBytes);
            Assert.IsNull(actual.Message);
        }

        [TestMethod]
        public void TestCreateVersionRoundTrip()
        {
            var version = VersionMessage.Create(network: 0x334F454E, nonce: 12345u);
            var expected = ProtocolMessage.Create(ProtocolMessageCommand.Version, version);
            var bytes = expected.ToArray();

            var consumed = ProtocolMessage.TryRead(bytes, out var actual);

            Assert.AreEqual(bytes.Length, consumed);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ProtocolMessageCommand.Version, actual.Command);
            Assert.IsFalse(actual.Compressed);
            Assert.IsInstanceOfType<VersionMessage>(actual.Message);

            var actualVersion = (VersionMessage)actual.Message;
            Assert.AreEqual(version.Network, actualVersion.Network);
            Assert.AreEqual(version.Nonce, actualVersion.Nonce);
            Assert.AreEqual(version.UserAgent, actualVersion.UserAgent);
        }

        [TestMethod]
        public void TestCreateDoesNotCompressVersion()
        {
            // Version is not in Neo's compressible command list.
            var largePayload = new RawBytesPayload(new byte[512]);
            var message = ProtocolMessage.Create(ProtocolMessageCommand.Version, largePayload);

            Assert.IsFalse(message.Compressed);
            Assert.HasCount(512, message.PayloadBytes);
        }

        [TestMethod]
        public void TestCreateCompressesEligiblePayload()
        {
            // Block is compressible; zeros compress well past the 64-byte threshold.
            var largePayload = new RawBytesPayload(new byte[512]);
            var message = ProtocolMessage.Create(ProtocolMessageCommand.Block, largePayload);

            Assert.IsTrue(message.Compressed);
            Assert.IsLessThan(512 - 64, message.PayloadBytes.Length);
        }

        [TestMethod]
        public void TestToArrayWithoutCompression()
        {
            var largePayload = new RawBytesPayload(new byte[512]);
            var message = ProtocolMessage.Create(ProtocolMessageCommand.Block, largePayload);
            Assert.IsTrue(message.Compressed);

            var compressedBytes = message.ToArray(allowCompression: true);
            var uncompressedBytes = message.ToArray(allowCompression: false);

            Assert.IsGreaterThan(0, compressedBytes.Length);
            Assert.AreNotEqual(compressedBytes.Length, uncompressedBytes.Length);

            var consumed = ProtocolMessage.TryRead(uncompressedBytes, out var actual);
            Assert.AreEqual(uncompressedBytes.Length, consumed);
            Assert.IsNotNull(actual);
            Assert.IsFalse(actual.Compressed);
            Assert.HasCount(512, actual.PayloadBytes);
        }

        /// <summary>
        /// Test-only payload: writes raw bytes with no extra framing.
        /// </summary>
        private sealed class RawBytesPayload(byte[] bytes) : INeoSerializable
        {
            private readonly byte[] _bytes = bytes;

            public int Size => _bytes.Length;

            public void Serialize(Stream writer) =>
                writer.Write(_bytes, 0, _bytes.Length);

            public void Deserialize(Stream reader) =>
                throw new NotSupportedException();
        }
    }
}
