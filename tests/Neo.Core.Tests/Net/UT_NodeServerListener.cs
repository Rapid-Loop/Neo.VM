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
using System.Threading.Tasks;

namespace Neo.Core.Tests.Net
{
    // Socket/listen tests must not run in parallel (assembly uses MethodLevel parallelization).
    [TestClass]
    [DoNotParallelize]
    public class UT_NodeServerListener
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task TestStartAndDispose()
        {
            // Port 0: OS assigns a free port at Bind (avoids GetFreeTcpPort TOCTOU on CI).
            await using var listener = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                ProtocolSettings.Default);

            listener.Start();
            Assert.IsTrue(listener.IsActive);
            Assert.IsGreaterThan(0u, listener.Nonce);
            Assert.IsGreaterThan(0, listener.BoundEndPoint.Port);
        }

        [TestMethod]
        public async Task TestDoubleStartIsIdempotent()
        {
            await using var listener = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                ProtocolSettings.Default);

            listener.Start();
            listener.Start();
            Assert.IsTrue(listener.IsActive);
        }

        [TestMethod]
        public async Task TestInvalidBacklogThrows()
        {
            await using var listener = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                ProtocolSettings.Default);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => listener.Start(0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => listener.Start(-1));
        }

        [TestMethod]
        public async Task TestAcceptHandshakeAndClientRemovedOnDisconnect()
        {
            var settings = new ProtocolSettings { Network = 0x334F454E };
            var ct = TestContext.CancellationToken;

            await using var listener = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                settings);
            listener.Start(backlog: 8);

            await using var client = await NodeConnection.ConnectAsync(
                listener.BoundEndPoint,
                settings,
                localNonce: listener.Nonce + 1,
                localCapabilities: [new FullNodeCapabilityMessage(0)],
                cancellationToken: ct);

            await client.WaitForHandshakeAsync(ct).WaitAsync(TimeSpan.FromSeconds(5), ct);

            Assert.IsTrue(client.IsReady);
            Assert.IsTrue(await TestUtilities.WaitForAsync(() => listener.Clients.Length == 1, TimeSpan.FromSeconds(5)));

            await client.DisposeAsync();

            Assert.IsTrue(await TestUtilities.WaitForAsync(() => listener.Clients.Length == 0, TimeSpan.FromSeconds(5)));
        }

        [TestMethod]
        public async Task TestOutboundConnectAsync()
        {
            var settings = new ProtocolSettings { Network = 0x334F454E };
            var ct = TestContext.CancellationToken;

            await using var server = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                settings);
            server.Start(backlog: 8);

            // Client node may dial without listening, but still uses node identity.
            await using var clientNode = new NodeServerListener(
                new IPEndPoint(IPAddress.Loopback, 0),
                settings);

            await using var outbound = await clientNode.ConnectAsync(server.BoundEndPoint, ct)
                .WaitAsync(TimeSpan.FromSeconds(5), ct);

            await outbound.WaitForHandshakeAsync(ct).WaitAsync(TimeSpan.FromSeconds(5), ct);

            Assert.IsTrue(outbound.IsReady);
            Assert.IsNotNull(outbound.RemoteVersion);
            Assert.AreEqual(settings.Network, outbound.RemoteVersion.Network);
            Assert.HasCount(1, clientNode.Clients);
            Assert.IsTrue(await TestUtilities.WaitForAsync(() => server.Clients.Length == 1, TimeSpan.FromSeconds(5)));

            await outbound.DisposeAsync();

            Assert.IsTrue(await TestUtilities.WaitForAsync(() => clientNode.Clients.Length == 0, TimeSpan.FromSeconds(5)));
            Assert.IsTrue(await TestUtilities.WaitForAsync(() => server.Clients.Length == 0, TimeSpan.FromSeconds(5)));
        }

        [TestMethod]
        public async Task TestPingPongAfterHandshake()
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
                localNonce: server.Nonce + 1,
                localCapabilities: [new FullNodeCapabilityMessage(5)],
                cancellationToken: ct);

            client.LocalBlockIndex = 5;
            await client.WaitForHandshakeAsync(ct).WaitAsync(TimeSpan.FromSeconds(5), ct);

            Assert.IsTrue(await TestUtilities.WaitForAsync(() => server.Clients.Length == 1, TimeSpan.FromSeconds(5)));
            var serverPeer = server.Clients[0];
            serverPeer.LocalBlockIndex = 10;

            var pongReceived = new TaskCompletionSource<PingMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.MessageReceived += (_, message) =>
            {
                if (message.Command == ProtocolMessageCommand.Pong && message.Message is PingMessage pong)
                    pongReceived.TrySetResult(pong);
            };

            var ping = PingMessage.Create(lastBlockIndex: 5, nonce: 0xABCD1234);
            await client.SendAsync(ProtocolMessage.Create(ProtocolMessageCommand.Ping, ping), cancellationToken: ct);

            var pongPayload = await pongReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            Assert.AreEqual(0xABCD1234u, pongPayload.Nonce);
            Assert.AreEqual(10u, pongPayload.LastBlockIndex);
            Assert.AreEqual(5u, serverPeer.RemoteLastBlockIndex);
        }
    }
}
