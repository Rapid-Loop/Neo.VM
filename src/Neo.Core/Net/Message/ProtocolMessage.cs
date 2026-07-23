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
using Neo.Core.Serialization;
using System;
using System.Buffers.Binary;
using System.IO;

namespace Neo.Core.Net.Message
{
    /// <summary>
    /// Neo N3 P2P wire frame:
    /// <c>[Flags:1][Command:1][VarSize length][payload]</c>
    /// Matches neo-project Message framing (master-n3).
    /// </summary>
    public class ProtocolMessage : INeoSerializable
    {
        /// <summary>
        /// Maximum allowed payload size (same as Neo N3 <c>Message.PayloadMaxSize</c>).
        /// </summary>
        public const int PayloadMaxSize = 0x0200_0000;

        private const int CompressionMinSize = 128;
        private const int CompressionThreshold = 64;

        /// <summary>
        /// Gets the message flags (for example, compressed).
        /// </summary>
        public ProtocolMessageFlags Flags { get; private set; }

        /// <summary>
        /// Gets the protocol command of this frame.
        /// </summary>
        public ProtocolMessageCommand Command { get; private set; }

        /// <summary>
        /// Gets the decoded payload object when known (Version, Ping, or Pong); otherwise, <see langword="null"/>.
        /// </summary>
        public INeoSerializable? Message => _protocolMessage;

        /// <summary>
        /// Gets a value indicating whether the payload is LZ4-compressed on the wire.
        /// </summary>
        public bool Compressed => Flags.HasFlag(ProtocolMessageFlags.Compressed);

        /// <summary>
        /// Payload bytes written on the wire (may be LZ4-compressed).
        /// </summary>
        public ReadOnlyMemory<byte> PayloadBytes => _compressedPayloadBytes;

        /// <summary>
        /// Gets the serialized size of this frame in bytes.
        /// </summary>
        public int Size =>
            sizeof(ProtocolMessageFlags) +
            sizeof(ProtocolMessageCommand) +
            _compressedPayloadBytes.GetSerializedSize();

        private INeoSerializable? _protocolMessage;
        private ReadOnlyMemory<byte> _payloadRaw;
        private ReadOnlyMemory<byte> _compressedPayloadBytes;

        /// <summary>
        /// Creates an outbound protocol message (Neo <c>Message.Create</c>).
        /// Optionally LZ4-compresses the payload for commands that allow it.
        /// </summary>
        public static ProtocolMessage Create(ProtocolMessageCommand command, INeoSerializable? payload = null)
        {
            var rawPayload = payload is null
                ? ReadOnlyMemory<byte>.Empty
                : payload.ToArray();

            if (rawPayload.Length > PayloadMaxSize)
            {
                throw new FormatException(
                    $"Invalid payload length: {rawPayload.Length}. " +
                    $"The payload size exceeds the maximum allowed size of {PayloadMaxSize} bytes.");
            }

            var message = new ProtocolMessage
            {
                Flags = ProtocolMessageFlags.None,
                Command = command,
                _protocolMessage = payload,
                _payloadRaw = rawPayload,
                _compressedPayloadBytes = rawPayload,
            };

            if (ShallCompress(command) && rawPayload.Length > CompressionMinSize)
            {
                var compressed = rawPayload.Span.ToLz4Compress().ToArray();

                // Only keep compression when it saves at least CompressionThreshold bytes.
                if (compressed.Length < rawPayload.Length - CompressionThreshold)
                {
                    message._compressedPayloadBytes = compressed;
                    message.Flags |= ProtocolMessageFlags.Compressed;
                }
            }

            return message;
        }

        /// <summary>
        /// Tries to read one complete protocol frame from a buffer.
        /// </summary>
        /// <returns>
        /// Bytes consumed for this frame, or <c>0</c> if more data is needed.
        /// </returns>
        /// <exception cref="FormatException">Payload length exceeds <see cref="PayloadMaxSize"/>.</exception>
        public static int TryRead(ReadOnlySpan<byte> data, out ProtocolMessage? message)
        {
            message = null;

            if (data.Length < 3)
                return 0;

            var flags = (ProtocolMessageFlags)data[0];
            var command = (ProtocolMessageCommand)data[1];

            ulong payloadLength = data[2];
            var payloadOffset = 3;

            if (payloadLength == 0xfd)
            {
                if (data.Length < 5)
                    return 0;

                payloadLength = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(3, 2));
                payloadOffset = 5;
            }
            else if (payloadLength == 0xfe)
            {
                if (data.Length < 7)
                    return 0;

                payloadLength = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(3, 4));
                payloadOffset = 7;
            }
            else if (payloadLength == 0xff)
            {
                if (data.Length < 11)
                    return 0;

                payloadLength = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(3, 8));
                payloadOffset = 11;
            }

            if (payloadLength > PayloadMaxSize)
            {
                throw new FormatException(
                    $"Invalid payload length: {payloadLength}. " +
                    $"The payload size exceeds the maximum allowed size of {PayloadMaxSize} bytes.");
            }

            if ((ulong)data.Length < (ulong)payloadOffset + payloadLength)
                return 0;

            var payload = payloadLength == 0
                ? ReadOnlyMemory<byte>.Empty
                : data.Slice(payloadOffset, (int)payloadLength).ToArray();

            message = new ProtocolMessage
            {
                Flags = flags,
                Command = command,
                _compressedPayloadBytes = payload,
                _payloadRaw = payload,
            };

            message.DecodePayload();

            return payloadOffset + (int)payloadLength;
        }

        /// <summary>
        /// Serializes this frame to bytes.
        /// When <paramref name="allowCompression"/> is false, the compressed flag is cleared
        /// and the uncompressed payload is sent (Neo: until remote Version allows compression).
        /// </summary>
        public byte[] ToArray(bool allowCompression = true)
        {
            if (allowCompression || Compressed == false)
                return ((INeoSerializable)this).ToArray();

            using var ms = new MemoryStream();
            ms.Write(Flags & ~ProtocolMessageFlags.Compressed);
            ms.Write(Command);
            ms.Write<byte>(_payloadRaw.Span);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserializes this frame from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        public void Deserialize(Stream reader)
        {
            Flags = reader.Read<ProtocolMessageFlags>();
            Command = reader.Read<ProtocolMessageCommand>();
            _compressedPayloadBytes = reader.ReadDynamic<byte>();
            _payloadRaw = _compressedPayloadBytes;

            DecodePayload();
        }

        /// <summary>
        /// Serializes this frame to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(Flags);
            writer.Write(Command);
            writer.Write<byte>(_compressedPayloadBytes.Span);
        }

        private static bool ShallCompress(ProtocolMessageCommand command) =>
            command is ProtocolMessageCommand.Block
                or ProtocolMessageCommand.Extensible
                or ProtocolMessageCommand.Transaction
                or ProtocolMessageCommand.Headers
                or ProtocolMessageCommand.Address
                or ProtocolMessageCommand.MerkleBlock
                or ProtocolMessageCommand.FilterLoad
                or ProtocolMessageCommand.FilterAdd;

        private void DecodePayload()
        {
            if (_compressedPayloadBytes.Length == 0)
            {
                _protocolMessage = null;
                _payloadRaw = ReadOnlyMemory<byte>.Empty;
                return;
            }

            var decompressedBytes = Flags.HasFlag(ProtocolMessageFlags.Compressed)
                ? _compressedPayloadBytes.Span.ToLz4Decompress()
                : _compressedPayloadBytes.Span;

            // After decompress, raw payload is the logical body (for re-send without compression).
            if (Flags.HasFlag(ProtocolMessageFlags.Compressed))
                _payloadRaw = decompressedBytes.ToArray();
            else
                _payloadRaw = _compressedPayloadBytes;

            _protocolMessage = Command switch
            {
                ProtocolMessageCommand.Version => decompressedBytes.AsSerializable<VersionMessage>(),
                ProtocolMessageCommand.Ping or ProtocolMessageCommand.Pong =>
                    decompressedBytes.AsSerializable<PingMessage>(),
                _ => null,
            };
        }
    }
}
