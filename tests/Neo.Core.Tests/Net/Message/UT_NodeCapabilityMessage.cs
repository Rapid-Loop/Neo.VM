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
using System.IO;

namespace Neo.Core.Tests.Net.Message
{
    [TestClass]
    public class UT_NodeCapabilityMessage
    {
        [TestMethod]
        public void TestServerCapabilityRoundTrip()
        {
            var expected = new ServerCapabilityMessage(port: 10333);
            var bytes = expected.ToArray();

            Assert.HasCount(expected.Size, bytes);

            using var ms = new MemoryStream(bytes);
            var actual = NodeCapabilityMessage.DeserializeFrom(ms);

            Assert.IsInstanceOfType<ServerCapabilityMessage>(actual);
            Assert.AreEqual(NodeCapabilityType.TcpServer, actual.Type);
            Assert.AreEqual((ushort)10333, ((ServerCapabilityMessage)actual).Port);
            Assert.AreEqual(bytes.Length, ms.Position);
        }

        [TestMethod]
        public void TestFullNodeCapabilityRoundTrip()
        {
            var expected = new FullNodeCapabilityMessage(startHeight: 1_000_000u);
            var bytes = expected.ToArray();

            Assert.HasCount(expected.Size, bytes);

            using var ms = new MemoryStream(bytes);
            var actual = NodeCapabilityMessage.DeserializeFrom(ms);

            Assert.IsInstanceOfType<FullNodeCapabilityMessage>(actual);
            Assert.AreEqual(1_000_000u, ((FullNodeCapabilityMessage)actual).StartHeight);
            Assert.AreEqual(bytes.Length, ms.Position);
        }

        [TestMethod]
        public void TestArchivalNodeCapabilityRoundTrip()
        {
            var expected = new ArchivalNodeCapabilityMessage();
            var bytes = expected.ToArray();

            using var ms = new MemoryStream(bytes);
            var actual = NodeCapabilityMessage.DeserializeFrom(ms);

            Assert.IsInstanceOfType<ArchivalNodeCapabilityMessage>(actual);
            Assert.AreEqual(NodeCapabilityType.ArchivalNode, actual.Type);
            Assert.AreEqual(bytes.Length, ms.Position);
        }

        [TestMethod]
        public void TestDisableCompressionCapabilityRoundTrip()
        {
            var expected = new DisableCompressionCapabilityMessage();
            var bytes = expected.ToArray();

            using var ms = new MemoryStream(bytes);
            var actual = NodeCapabilityMessage.DeserializeFrom(ms);

            Assert.IsInstanceOfType<DisableCompressionCapabilityMessage>(actual);
            Assert.AreEqual(NodeCapabilityType.DisableCompression, actual.Type);
            Assert.AreEqual(bytes.Length, ms.Position);
        }

        [TestMethod]
        public void TestStandaloneDeserialize()
        {
            // INeoSerializable path still works: type byte + payload.
            var expected = new ServerCapabilityMessage(port: 20333);
            var actual = expected.ToArray().AsSerializable<ServerCapabilityMessage>();

            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.Type, actual.Type);
        }
    }
}
