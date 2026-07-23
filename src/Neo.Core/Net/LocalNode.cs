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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Core.Net
{
    /// <summary>
    /// Manages the local P2P node, including listening for incoming connections
    /// and connecting to seed nodes using TCP sockets.
    /// </summary>
    public sealed class LocalNode : IAsyncDisposable
    {
        private readonly ProtocolSettings _settings;
        private readonly ILogger? _logger;
        private readonly ConcurrentDictionary<IPEndPoint, Peer> _peers = new();
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _disposed;

        /// <summary>
        /// Gets the currently connected peers.
        /// </summary>
        public IReadOnlyCollection<Peer> ConnectedPeers => _peers.Values.ToList().AsReadOnly();

        /// <summary>
        /// Gets the number of currently connected peers.
        /// </summary>
        public int PeerCount => _peers.Count;

        /// <summary>
        /// Occurs when a new peer connects (inbound or outbound).
        /// </summary>
        public event Action<Peer>? PeerConnected;

        /// <summary>
        /// Occurs when a peer disconnects.
        /// </summary>
        public event Action<Peer>? PeerDisconnected;

        /// <summary>
        /// Occurs when a message is received from any peer.
        /// </summary>
        public event Action<Peer, Message>? MessageReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalNode"/> class.
        /// </summary>
        /// <param name="settings">The protocol settings containing network magic and seed list.</param>
        /// <param name="logger">Optional logger instance.</param>
        public LocalNode(ProtocolSettings settings, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// Starts the local node: begins listening for inbound connections and connects to seed nodes.
        /// </summary>
        /// <param name="listenPort">The local TCP port to listen on. Use 0 for ephemeral port.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task StartAsync(int listenPort = 10333, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_cts is not null)
                throw new InvalidOperationException("LocalNode is already started.");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start listener
            _listener = new TcpListener(IPAddress.Any, listenPort);
            _listener.Start();
            _logger?.LogInformation("P2P LocalNode listening on port {Port}", ((IPEndPoint)_listener.LocalEndpoint).Port);

            // Accept loop
            _ = AcceptLoopAsync(_cts.Token);

            // Connect to seeds
            await ConnectToSeedsAsync(_cts.Token).ConfigureAwait(false);
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _listener is not null)
                {
                    var socket = await _listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
                    _ = HandleInboundAsync(socket);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in accept loop");
            }
        }

        private async Task HandleInboundAsync(Socket socket)
        {
            try
            {
                var peer = new Peer(socket, _logger);
                if (_peers.TryAdd(peer.RemoteEndPoint, peer))
                {
                    peer.MessageReceived += OnPeerMessageReceived;
                    peer.Disconnected += OnPeerDisconnected;
                    peer.Start();
                    PeerConnected?.Invoke(peer);
                    _logger?.LogInformation("Inbound peer connected: {Remote}", peer.RemoteEndPoint);

                    // Send Version handshake
                    await SendVersionAsync(peer).ConfigureAwait(false);
                }
                else
                {
                    await peer.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to handle inbound connection");
                try { socket.Dispose(); } catch { /* ignore */ }
            }
        }

        private async Task ConnectToSeedsAsync(CancellationToken cancellationToken)
        {
            var seeds = _settings.SeedList;
            if (seeds.Length == 0)
            {
                _logger?.LogWarning("No seed nodes configured in ProtocolSettings.");
                return;
            }

            var tasks = seeds.Select(seed => ConnectToPeerAsync(seed, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Connects to a remote peer at the specified endpoint.
        /// </summary>
        /// <param name="endPoint">The remote endpoint.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The connected peer, or null if connection failed.</returns>
        public async Task<Peer?> ConnectToPeerAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(endPoint);

            if (_peers.ContainsKey(endPoint))
            {
                _logger?.LogDebug("Already connected to {Remote}", endPoint);
                return _peers[endPoint];
            }

            try
            {
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                await socket.ConnectAsync(endPoint, timeoutCts.Token).ConfigureAwait(false);

                var peer = new Peer(socket, _logger);
                if (_peers.TryAdd(endPoint, peer))
                {
                    peer.MessageReceived += OnPeerMessageReceived;
                    peer.Disconnected += OnPeerDisconnected;
                    peer.Start();
                    PeerConnected?.Invoke(peer);
                    _logger?.LogInformation("Outbound peer connected: {Remote}", endPoint);

                    await SendVersionAsync(peer).ConfigureAwait(false);
                    return peer;
                }

                await peer.DisposeAsync().ConfigureAwait(false);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to connect to seed {Remote}", endPoint);
                return null;
            }
        }

        private async Task SendVersionAsync(Peer peer)
        {
            // Minimal Version message (payload can be extended later)
            var versionPayload = new byte[0]; // Placeholder for full version payload
            var message = new Message(_settings.Network, MessageCommandType.Version, versionPayload);
            await peer.SendAsync(message).ConfigureAwait(false);
        }

        private void OnPeerMessageReceived(Peer peer, Message message)
        {
            // Basic handshake handling
            if (message.Command == MessageCommandType.Version)
            {
                // Respond with VersionAck
                _ = peer.SendAsync(new Message(_settings.Network, MessageCommandType.VersionAck));
            }
            else if (message.Command == MessageCommandType.Ping)
            {
                _ = peer.SendAsync(new Message(_settings.Network, MessageCommandType.Pong));
            }

            MessageReceived?.Invoke(peer, message);
        }

        private void OnPeerDisconnected(Peer peer)
        {
            if (_peers.TryRemove(peer.RemoteEndPoint, out _))
            {
                peer.MessageReceived -= OnPeerMessageReceived;
                peer.Disconnected -= OnPeerDisconnected;
                PeerDisconnected?.Invoke(peer);
            }
        }

        /// <summary>
        /// Broadcasts a message to all currently connected peers.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task BroadcastAsync(Message message, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(message);

            var tasks = _peers.Values.Select(p => p.SendAsync(message, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the local node and disconnects all peers.
        /// </summary>
        public async Task StopAsync()
        {
            if (_disposed || _cts is null) return;

            _cts.Cancel();

            if (_listener is not null)
            {
                try { _listener.Stop(); } catch { /* ignore */ }
                _listener = null;
            }

            var disconnectTasks = _peers.Values.Select(p => p.DisconnectAsync());
            await Task.WhenAll(disconnectTasks).ConfigureAwait(false);
            _peers.Clear();

            _cts.Dispose();
            _cts = null;

            _logger?.LogInformation("LocalNode stopped.");
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            await StopAsync().ConfigureAwait(false);
        }
    }
}
