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
using Neo.Core.Net.Message;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Core.Net
{
    /// <summary>
    /// TCP connection that frames Neo P2P messages, runs Version/Verack handshake,
    /// and maintains the link with Ping/Pong + idle timeouts.
    /// </summary>
    public class NodeConnection : IAsyncDisposable
    {
        /// <summary>
        /// Initial timeout before any complete frame is received (Neo connection start limit).
        /// </summary>
        public static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Idle timeout after the last received frame (Neo connection limit).
        /// </summary>
        public static readonly TimeSpan ReceiveIdleTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// How often keep-alive checks run.
        /// </summary>
        public static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Send a Ping if nothing has been sent for this long (Neo RemoteNode timer).
        /// </summary>
        public static readonly TimeSpan SendIdleBeforePing = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Raised for each complete frame after the handshake is Ready
        /// (including Ping/Pong after internal handling).
        /// </summary>
        public event EventHandler<ProtocolMessage>? MessageReceived;

        /// <summary>
        /// Raised once when Version + Verack exchange completes successfully.
        /// </summary>
        public event EventHandler? HandshakeCompleted;

        /// <summary>
        /// Raised once when the connection is closed (read loop ended or dispose).
        /// </summary>
        public event EventHandler? Disconnected;

        public bool Connected => _socket.Connected;

        public EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;

        public EndPoint? LocalEndPoint => _socket.LocalEndPoint;

        public NodeHandshakeState HandshakeState => _handshake.State;

        public bool IsReady => _handshake.IsReady;

        public VersionMessage? RemoteVersion => _handshake.RemoteVersion;

        /// <summary>
        /// Highest block index reported by the remote peer (Version FullNode / Ping / Pong).
        /// </summary>
        public uint RemoteLastBlockIndex => (uint)Volatile.Read(ref _remoteLastBlockIndex);

        /// <summary>
        /// Local chain height included in outbound Ping/Pong. Host code should update this.
        /// </summary>
        public uint LocalBlockIndex
        {
            get => (uint)Volatile.Read(ref _localBlockIndex);
            set => Volatile.Write(ref _localBlockIndex, value);
        }

        private readonly Socket _socket;
        private readonly Pipe _networkPipe;
        private readonly ILogger<NodeConnection> _logger;
        private readonly ProtocolSettings _protocolSettings;
        private readonly uint _localNonce;
        private readonly NodeCapabilityMessage[] _localCapabilities;
        private readonly NodeHandshake _handshake;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly CancellationTokenSource _lifetimeCts = new();
        private readonly long _startedTicks = DateTime.UtcNow.Ticks;

        private Task _fillingPipeTask = Task.CompletedTask;
        private Task _protocolTask = Task.CompletedTask;
        private Task _keepAliveTask = Task.CompletedTask;
        private int _disconnected;
        private long _lastReceivedTicks;
        private long _lastSentTicks;
        private long _localBlockIndex;
        private long _remoteLastBlockIndex;

        public NodeConnection(
            Socket socket,
            ProtocolSettings protocolSettings,
            uint localNonce,
            NodeCapabilityMessage[]? localCapabilities = default,
            ILoggerFactory? loggerFactory = default)
        {
            ArgumentNullException.ThrowIfNull(socket);
            ArgumentNullException.ThrowIfNull(protocolSettings);

            _socket = socket;
            _protocolSettings = protocolSettings;
            _localNonce = localNonce;
            _localCapabilities = localCapabilities ?? [];
            _networkPipe = new Pipe();
            _handshake = new NodeHandshake(protocolSettings.Network, localNonce);
            _logger = (loggerFactory ?? NullLoggerFactory.Instance)
                .CreateLogger<NodeConnection>();

            var now = DateTime.UtcNow.Ticks;
            _lastReceivedTicks = now;
            _lastSentTicks = now;
        }

        /// <summary>
        /// Dials <paramref name="remoteEndPoint"/>, starts framing + handshake (sends Version).
        /// Caller owns the returned connection and should dispose it.
        /// </summary>
        public static async Task<NodeConnection> ConnectAsync(
            IPEndPoint remoteEndPoint,
            ProtocolSettings protocolSettings,
            uint localNonce,
            NodeCapabilityMessage[]? localCapabilities = null,
            ILoggerFactory? loggerFactory = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(remoteEndPoint);
            ArgumentNullException.ThrowIfNull(protocolSettings);

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(remoteEndPoint, cancellationToken).ConfigureAwait(false);

                var connection = new NodeConnection(
                    socket,
                    protocolSettings,
                    localNonce,
                    localCapabilities,
                    loggerFactory);

                connection.Start();
                return connection;
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Completes when the Version/Verack handshake finishes, or fails if the connection drops first.
        /// </summary>
        public async Task WaitForHandshakeAsync(CancellationToken cancellationToken = default)
        {
            if (IsReady)
                return;

            if (Volatile.Read(ref _disconnected) != 0)
                throw new IOException("Connection closed before handshake completed.");

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnCompleted(object? sender, EventArgs e) =>
                tcs.TrySetResult();

            void OnDisconnected(object? sender, EventArgs e) =>
                tcs.TrySetException(new IOException("Connection closed before handshake completed."));

            HandshakeCompleted += OnCompleted;
            Disconnected += OnDisconnected;

            try
            {
                // Re-check after subscribe to close the race with concurrent handshake/disconnect.
                if (IsReady)
                    return;

                if (Volatile.Read(ref _disconnected) != 0)
                    throw new IOException("Connection closed before handshake completed.");

                await using (cancellationToken.Register(static state =>
                {
                    var source = (TaskCompletionSource)state!;
                    source.TrySetCanceled();
                }, tcs))
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                HandshakeCompleted -= OnCompleted;
                Disconnected -= OnDisconnected;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _lifetimeCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                _socket.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                await _protocolTask.ConfigureAwait(false);
                await _keepAliveTask.ConfigureAwait(false);
                await _fillingPipeTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

            _sendLock.Dispose();
            _lifetimeCts.Dispose();
            NotifyDisconnected();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts receive pumps, keep-alive, and sends the local Version (Neo StartProtocol).
        /// </summary>
        /// <remarks>
        /// Local Version is sent before the frame reader runs. Otherwise a Verack reply to the
        /// peer's Version can race ahead of our Version on the wire; the peer then sees Verack
        /// first and aborts with a protocol violation (common on macOS CI scheduling).
        /// Socket fill still runs concurrently so peer bytes buffer in the pipe until we read.
        /// </remarks>
        internal void Start()
        {
            _fillingPipeTask = DoFillPipeAsync();
            _protocolTask = RunProtocolAsync();
            _keepAliveTask = RunKeepAliveAsync(_lifetimeCts.Token);
        }

        /// <summary>
        /// Send local Version, then process inbound frames (including remote Version / Verack).
        /// </summary>
        private async Task RunProtocolAsync()
        {
            await StartProtocolAsync().ConfigureAwait(false);
            await DoReadPipeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sends one framed protocol message.
        /// </summary>
        public async ValueTask SendAsync(
            ProtocolMessage message,
            bool? allowCompression = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var compress = allowCompression ?? _handshake.RemoteAllowsCompression;
            var bytes = message.ToArray(compress);

            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _socket.SendAsync(bytes, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                TouchSent();
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task StartProtocolAsync()
        {
            try
            {
                var version = VersionMessage.Create(
                    _protocolSettings.Network,
                    _localNonce,
                    _localCapabilities);

                await SendAsync(
                    ProtocolMessage.Create(ProtocolMessageCommand.Version, version),
                    allowCompression: false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start protocol (send Version).");
                AbortSocket();
            }
        }

        private async Task RunKeepAliveAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var timer = new PeriodicTimer(KeepAliveInterval);

                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                {
                    var now = DateTime.UtcNow;
                    var lastReceived = new DateTime(Interlocked.Read(ref _lastReceivedTicks), DateTimeKind.Utc);
                    var lastSent = new DateTime(Interlocked.Read(ref _lastSentTicks), DateTimeKind.Utc);
                    var started = new DateTime(_startedTicks, DateTimeKind.Utc);

                    if (IsReady == false)
                    {
                        // Neo: short timeout before the connection is fully active.
                        if (now - started >= HandshakeTimeout)
                        {
                            _logger.LogWarning(
                                "Handshake timeout with {Remote} after {Timeout}.",
                                RemoteEndPoint,
                                HandshakeTimeout);
                            AbortSocket();
                            break;
                        }

                        continue;
                    }

                    if (now - lastReceived >= ReceiveIdleTimeout)
                    {
                        _logger.LogWarning(
                            "Receive idle timeout with {Remote} after {Timeout}.",
                            RemoteEndPoint,
                            ReceiveIdleTimeout);
                        AbortSocket();
                        break;
                    }

                    if (now - lastSent >= SendIdleBeforePing)
                    {
                        var ping = PingMessage.Create(LocalBlockIndex);
                        await SendAsync(
                            ProtocolMessage.Create(ProtocolMessageCommand.Ping, ping),
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Keep-alive loop ended.");
            }
        }

        private async Task DoFillPipeAsync()
        {
            Exception? error = null;
            var writer = _networkPipe.Writer;

            try
            {
                while (true)
                {
                    var memory = writer.GetMemory(_socket.ReceiveBufferSize);
                    var bytesReceived = await _socket.ReceiveAsync(memory, SocketFlags.None).ConfigureAwait(false);

                    if (bytesReceived == 0)
                        break;

                    writer.Advance(bytesReceived);

                    var result = await writer.FlushAsync().ConfigureAwait(false);
                    if (result.IsCompleted || result.IsCanceled)
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException ex)
            {
                error = ex;
                _logger.LogDebug(ex, "Socket receive ended.");
            }
            catch (Exception ex)
            {
                error = ex;
                _logger.LogError(ex, "Failed to fill network pipe.");
            }
            finally
            {
                await writer.CompleteAsync(error).ConfigureAwait(false);
            }
        }

        private async Task DoReadPipeAsync()
        {
            Exception? unexpectedError = null;
            var reader = _networkPipe.Reader;

            try
            {
                while (true)
                {
                    var readResult = await reader.ReadAsync().ConfigureAwait(false);

                    if (readResult.IsCanceled)
                        break;

                    var buffer = readResult.Buffer;

                    while (TryReadFrame(ref buffer, out var message))
                        await HandleMessageAsync(message!).ConfigureAwait(false);

                    reader.AdvanceTo(buffer.Start, buffer.End);

                    if (readResult.IsCompleted)
                    {
                        if (buffer.Length > 0)
                            throw new FormatException("Connection closed with an incomplete protocol frame.");

                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                unexpectedError = ex;
                _logger.LogError(ex, "Failed to read protocol frames.");
            }
            finally
            {
                await reader.CompleteAsync(unexpectedError).ConfigureAwait(false);

                try
                {
                    _networkPipe.Writer.CancelPendingFlush();
                }
                catch (InvalidOperationException)
                {
                }

                AbortSocket();
                NotifyDisconnected();
            }
        }

        private void NotifyDisconnected()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) != 0)
                return;

            try
            {
                _lifetimeCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private async Task HandleMessageAsync(ProtocolMessage message)
        {
            try
            {
                TouchReceived();

                if (_handshake.IsReady == false)
                {
                    var reply = _handshake.Process(message);

                    if (reply is not null)
                    {
                        await SendAsync(reply, allowCompression: false).ConfigureAwait(false);
                    }

                    if (_handshake.IsReady)
                    {
                        ApplyRemoteVersionHeight(_handshake.RemoteVersion);
                        HandshakeCompleted?.Invoke(this, EventArgs.Empty);
                    }

                    return;
                }

                // Already Ready: Process validates no second handshake messages.
                _handshake.Process(message);

                if (message.Command == ProtocolMessageCommand.Ping)
                    await HandlePingAsync(message).ConfigureAwait(false);
                else if (message.Command == ProtocolMessageCommand.Pong)
                    HandlePong(message);

                MessageReceived?.Invoke(this, message);
            }
            catch (ProtocolViolationException ex)
            {
                _logger.LogWarning(ex, "Protocol violation from {Remote}.", RemoteEndPoint);
                AbortSocket();
            }
        }

        private async Task HandlePingAsync(ProtocolMessage message)
        {
            if (message.Message is not PingMessage ping)
            {
                _logger.LogWarning("Ping without payload from {Remote}.", RemoteEndPoint);
                return;
            }

            UpdateRemoteLastBlockIndex(ping.LastBlockIndex);

            // Neo: Pong reuses the Ping nonce; height is local tip.
            var pong = PingMessage.Create(LocalBlockIndex, ping.Nonce);
            await SendAsync(ProtocolMessage.Create(ProtocolMessageCommand.Pong, pong)).ConfigureAwait(false);
        }

        private void HandlePong(ProtocolMessage message)
        {
            if (message.Message is not PingMessage pong)
                return;

            UpdateRemoteLastBlockIndex(pong.LastBlockIndex);
        }

        private void ApplyRemoteVersionHeight(VersionMessage? version)
        {
            if (version is null)
                return;

            foreach (var capability in version.Capabilities.OfType<FullNodeCapabilityMessage>())
                UpdateRemoteLastBlockIndex(capability.StartHeight);
        }

        private void UpdateRemoteLastBlockIndex(uint lastBlockIndex)
        {
            while (true)
            {
                var current = (uint)Volatile.Read(ref _remoteLastBlockIndex);
                if (lastBlockIndex <= current)
                    return;

                if (Interlocked.CompareExchange(ref _remoteLastBlockIndex, lastBlockIndex, current) == current)
                    return;
            }
        }

        private void TouchReceived() =>
            Interlocked.Exchange(ref _lastReceivedTicks, DateTime.UtcNow.Ticks);

        private void TouchSent() =>
            Interlocked.Exchange(ref _lastSentTicks, DateTime.UtcNow.Ticks);

        private void AbortSocket()
        {
            try
            {
                _socket.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// Tries to consume one complete frame from <paramref name="buffer"/>.
        /// </summary>
        internal static bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out ProtocolMessage? message)
        {
            message = null;

            if (buffer.IsEmpty)
                return false;

            int consumed;
            if (buffer.IsSingleSegment)
            {
                consumed = ProtocolMessage.TryRead(buffer.FirstSpan, out message);
            }
            else
            {
                var copy = buffer.ToArray();
                consumed = ProtocolMessage.TryRead(copy, out message);
            }

            if (consumed == 0)
            {
                message = null;
                return false;
            }

            buffer = buffer.Slice(consumed);
            return true;
        }
    }
}
