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
using Microsoft.Extensions.Logging.Abstractions;
using Neo.Core.Factory;
using Neo.Core.Net.Message;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Core.Net
{
    /// <summary>
    /// TCP listener that accepts Neo P2P peers and starts each connection's handshake.
    /// Bind exclusivity is enforced by the OS (no process-wide named mutex).
    /// </summary>
    public class NodeServerListener : IAsyncDisposable
    {
        /// <summary>
        /// Default accept backlog when <see cref="Start()"/> is called without a value.
        /// </summary>
        public const int DefaultBacklog = 128;

        public NodeConnection[] Clients => [.. _connections.Keys];

        public ProtocolSettings ProtocolSettings => _protocolSettings;

        /// <summary>
        /// Configured listen endpoint (port may be 0 before <see cref="Start()"/>).
        /// Prefer <see cref="BoundEndPoint"/> after start when port 0 was used.
        /// </summary>
        public EndPoint LocalEndPoint => _serverSocket.LocalEndPoint ?? _endPoint;

        /// <summary>
        /// Actual bound endpoint after <see cref="Start()"/> (resolves port 0 to the OS-assigned port).
        /// </summary>
        public IPEndPoint BoundEndPoint =>
            _serverSocket.LocalEndPoint as IPEndPoint
                ?? throw new InvalidOperationException($"Listener is not bound. Call {nameof(Start)} first.");

        public bool IsActive => _isActive != 0;

        /// <summary>
        /// Random nonce identifying this node (Neo LocalNode.Nonce).
        /// </summary>
        public uint Nonce => _nonce;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private readonly IPEndPoint _endPoint;
        private readonly Socket _serverSocket;
        private readonly ProtocolSettings _protocolSettings;
        private readonly uint _nonce;
        private readonly NodeCapabilityMessage[] _capabilities;

        private readonly CancellationTokenSource _listeningTokenSource = new();
        private readonly ConcurrentDictionary<NodeConnection, byte> _connections = [];

        private Task _listeningTask = Task.CompletedTask;
        private int _isActive;
        private int _disposed;

        public NodeServerListener(
            IPEndPoint endPoint,
            ProtocolSettings? protocolSettings = default,
            ILoggerFactory? loggerFactory = default)
        {
            ArgumentNullException.ThrowIfNull(endPoint);

            _endPoint = endPoint;
            _serverSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _protocolSettings = protocolSettings ?? ProtocolSettings.Default;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<NodeServerListener>();
            _nonce = RandomNumberFactory.NextUInt32();
            _capabilities = CreateLocalCapabilities(endPoint);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Interlocked.Exchange(ref _isActive, 0);

            try
            {
                _listeningTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            // Unblock AcceptAsync.
            try
            {
                _serverSocket.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                await _listeningTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

            foreach (var client in _connections.Keys)
            {
                client.Disconnected -= OnClientDisconnected;
                await client.DisposeAsync().ConfigureAwait(false);
            }

            _connections.Clear();
            _listeningTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            Start(DefaultBacklog);
        }

        public void Start(int backlog)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(backlog);
            ObjectDisposedException.ThrowIf(_disposed != 0, this);

            if (Interlocked.CompareExchange(ref _isActive, 1, 0) != 0)
                return;

            try
            {
                // Exclusive bind on Windows: avoids port-sharing races under SO_REUSEADDR on CI.
                if (OperatingSystem.IsWindows())
                    _serverSocket.ExclusiveAddressUse = true;

                _serverSocket.Bind(_endPoint);
                _serverSocket.Listen(backlog);
                _listeningTask = AcceptLoopAsync();
            }
            catch
            {
                Interlocked.Exchange(ref _isActive, 0);
                throw;
            }
        }

        /// <summary>
        /// Outbound connect to a remote peer using this node's network identity (nonce + capabilities).
        /// The connection is tracked in <see cref="Clients"/> until it disconnects.
        /// Listening is optional — you may call this without starting the accept loop.
        /// </summary>
        public async Task<NodeConnection> ConnectAsync(
            IPEndPoint remoteEndPoint,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(remoteEndPoint);
            ObjectDisposedException.ThrowIf(_disposed != 0, this);

            var connection = await NodeConnection.ConnectAsync(
                remoteEndPoint,
                _protocolSettings,
                _nonce,
                _capabilities,
                _loggerFactory,
                cancellationToken).ConfigureAwait(false);

            if (TrackConnection(connection) == false)
            {
                await connection.DisposeAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Failed to track outbound connection.");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Connected outbound to {Remote}.", connection.RemoteEndPoint);

            return connection;
        }

        private async Task AcceptLoopAsync()
        {
            var token = _listeningTokenSource.Token;

            while (token.IsCancellationRequested == false)
            {
                Socket clientSocket;
                try
                {
                    clientSocket = await _serverSocket.AcceptAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_disposed != 0)
                {
                    break;
                }
                catch (SocketException ex) when (token.IsCancellationRequested == false)
                {
                    _logger.LogError(ex, "Accept failed.");
                    continue;
                }

                var clientConnection = new NodeConnection(
                    clientSocket,
                    _protocolSettings,
                    _nonce,
                    _capabilities,
                    _loggerFactory);

                if (TrackConnection(clientConnection) == false)
                {
                    await clientConnection.DisposeAsync().ConfigureAwait(false);
                    continue;
                }

                clientConnection.Start();

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Accepted peer {Remote}.", clientConnection.RemoteEndPoint);
            }
        }

        private bool TrackConnection(NodeConnection connection)
        {
            connection.Disconnected += OnClientDisconnected;

            if (_connections.TryAdd(connection, 0))
                return true;

            connection.Disconnected -= OnClientDisconnected;
            return false;
        }

        private void OnClientDisconnected(object? sender, EventArgs e)
        {
            if (sender is not NodeConnection connection)
                return;

            connection.Disconnected -= OnClientDisconnected;
            _connections.TryRemove(connection, out _);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Peer disconnected {Remote}.", connection.RemoteEndPoint);
        }

        private static NodeCapabilityMessage[] CreateLocalCapabilities(IPEndPoint endPoint)
        {
            return
            [
                new FullNodeCapabilityMessage(startHeight: 0),
                new ServerCapabilityMessage(port: (ushort)endPoint.Port),
            ];
        }
    }
}
