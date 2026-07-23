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
using Neo.Core.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Core.Net
{
    /// <summary>
    /// Represents a connected remote peer in the P2P network using TCP sockets.
    /// </summary>
    public sealed class Peer : IAsyncDisposable
    {
        private readonly Socket _socket;
        private readonly NetworkStream _stream;
        private readonly ILogger? _logger;
        private readonly CancellationTokenSource _cts = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// Gets the remote endpoint of this peer.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets a value indicating whether the peer is currently connected.
        /// </summary>
        public bool IsConnected => _socket.Connected && !_disposed;

        /// <summary>
        /// Occurs when a message is received from the peer.
        /// </summary>
        public event Action<Peer, Message>? MessageReceived;

        /// <summary>
        /// Occurs when the peer is disconnected.
        /// </summary>
        public event Action<Peer>? Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Peer"/> class from an accepted socket.
        /// </summary>
        /// <param name="socket">The connected socket.</param>
        /// <param name="logger">Optional logger.</param>
        public Peer(Socket socket, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(socket);
            if (!socket.Connected)
                throw new ArgumentException("Socket must be connected.", nameof(socket));

            _socket = socket;
            _socket.NoDelay = true;
            _stream = new NetworkStream(socket, ownsSocket: false);
            _logger = logger;
            RemoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint!;
        }

        /// <summary>
        /// Starts the receive loop for this peer.
        /// </summary>
        public void Start() => _ = ReceiveLoopAsync(_cts.Token);

        /// <summary>
        /// Sends a message to the remote peer asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SendAsync(Message message, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(message);

            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using var ms = new MemoryStream();
                message.Serialize(ms);
                var data = ms.ToArray();
                await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                _logger?.LogDebug("Sent {Command} to {Remote} ({Length} bytes)", message.Command, RemoteEndPoint, data.Length);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var headerBuffer = new byte[Message.HeaderSize];
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    // Read header
                    await ReadExactlyAsync(_stream, headerBuffer, cancellationToken).ConfigureAwait(false);

                    using var headerStream = new MemoryStream(headerBuffer);
                    var magic = headerStream.Read<uint>();
                    var command = (MessageCommandType)headerStream.ReadByte();
                    var length = headerStream.Read<uint>();
                    var checksum = headerStream.Read<uint>();

                    if (length > Message.MaxPayloadSize)
                    {
                        _logger?.LogWarning("Peer {Remote} sent oversized payload ({Length}). Disconnecting.", RemoteEndPoint, length);
                        break;
                    }

                    byte[] payload = [];
                    if (length > 0)
                    {
                        payload = new byte[length];
                        await ReadExactlyAsync(_stream, payload, cancellationToken).ConfigureAwait(false);

                        if (Message.ComputeChecksum(payload) != checksum)
                        {
                            _logger?.LogWarning("Peer {Remote} sent message with invalid checksum. Disconnecting.", RemoteEndPoint);
                            break;
                        }
                    }

                    var message = new Message
                    {
                        Magic = magic,
                        Command = command,
                        Payload = payload
                    };

                    _logger?.LogDebug("Received {Command} from {Remote} ({Length} bytes)", command, RemoteEndPoint, Message.HeaderSize + length);
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (IOException ex)
            {
                _logger?.LogInformation(ex, "IO error with peer {Remote}", RemoteEndPoint);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error receiving from peer {Remote}", RemoteEndPoint);
            }
            finally
            {
                await DisconnectAsync().ConfigureAwait(false);
            }
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    throw new EndOfStreamException("Remote peer closed the connection.");
                offset += read;
            }
        }

        /// <summary>
        /// Disconnects the peer and releases resources.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _cts.Cancel();
                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch
            {
                // Ignore shutdown errors
            }

            try
            {
                _stream.Dispose();
                _socket.Dispose();
            }
            catch
            {
                // Ignore
            }

            _cts.Dispose();
            _sendLock.Dispose();

            Disconnected?.Invoke(this);
            _logger?.LogInformation("Disconnected from peer {Remote}", RemoteEndPoint);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() => await DisconnectAsync().ConfigureAwait(false);
    }
}
