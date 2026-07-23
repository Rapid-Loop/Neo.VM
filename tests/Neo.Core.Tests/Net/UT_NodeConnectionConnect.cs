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

using Neo.Core.Net;
using Neo.Core.Net.Message;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Neo.Core.Tests.Net
{
    // Socket/listen tests must not run in parallel (assembly uses MethodLevel parallelization).
    [TestClass]
    [DoNotParallelize]
    public class UT_NodeConnectionConnect
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task TestConnectAsyncHandshake()
        {
            var settings = new ProtocolSettings { Network = 0x334F454E };
            var ct = TestContext.CancellationToken;

            await using var server = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                settings);
            server.Start(backlog: 4);

            await using var client = await NodeConnection.ConnectAsync(
                server.BoundEndPoint,
                settings,
                localNonce: 42,
                localCapabilities:
                [
                    new FullNodeCapabilityMessage(10),
                    new ServerCapabilityMessage(20333),
                ],
                cancellationToken: ct);

            await client.WaitForHandshakeAsync(ct).WaitAsync(TimeSpan.FromSeconds(5), ct);

            Assert.IsTrue(client.IsReady);
            Assert.IsNotNull(client.RemoteVersion);
            Assert.AreEqual(settings.Network, client.RemoteVersion.Network);
            Assert.AreEqual(server.Nonce, client.RemoteVersion.Nonce);
        }

        [TestMethod]
        public async Task TestConnectAsyncUnreachableThrows()
        {
            var settings = new ProtocolSettings { Network = 1 };
            // Port with nothing listening (best-effort; OS may refuse quickly).
            var endPoint = new IPEndPoint(IPAddress.Loopback, TestUtilities.GetFreeTcpPort());
            var ct = TestContext.CancellationToken;

            await Assert.ThrowsExactlyAsync<SocketException>(async () =>
            {
                await NodeConnection.ConnectAsync(endPoint, settings, localNonce: 1, cancellationToken: ct)
                    .WaitAsync(TimeSpan.FromSeconds(5), ct);
            });
        }
    }
}
