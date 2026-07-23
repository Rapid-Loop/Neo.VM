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

using Neo.Core.Net.Message;
using System;
using System.Net;

namespace Neo.Core.Net
{
    /// <summary>
    /// Pure Neo N3 handshake state machine (no I/O).
    /// Local already sent Version; then: receive Version → send Verack → receive Verack → Ready.
    /// </summary>
    public sealed class NodeHandshake(uint network, uint localNonce)
    {
        private readonly uint _network = network;
        private readonly uint _localNonce = localNonce;

        public NodeHandshakeState State { get; private set; } = NodeHandshakeState.WaitingForVersion;

        public VersionMessage? RemoteVersion { get; private set; }

        public bool IsReady => State == NodeHandshakeState.Ready;

        /// <summary>
        /// Whether the remote peer allows payload compression (known after Version).
        /// </summary>
        public bool RemoteAllowsCompression => RemoteVersion?.AllowCompression ?? false;

        /// <summary>
        /// Processes one inbound frame.
        /// </summary>
        /// <returns>
        /// An optional reply to send immediately (e.g. Verack after Version).
        /// When <see cref="IsReady"/> and the message is a normal protocol command, returns null
        /// and the caller should dispatch the message to application handlers.
        /// </returns>
        /// <exception cref="ProtocolViolationException">Wrong command order or invalid handshake data.</exception>
        public ProtocolMessage? Process(ProtocolMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            return State switch
            {
                NodeHandshakeState.WaitingForVersion => ProcessVersion(message),
                NodeHandshakeState.WaitingForVerack => ProcessVerack(message),
                NodeHandshakeState.Ready => ProcessReady(message),
                _ => throw new InvalidOperationException($"Unknown handshake state: {State}."),
            };
        }

        private ProtocolMessage ProcessVersion(ProtocolMessage message)
        {
            if (message.Command != ProtocolMessageCommand.Version)
                throw new ProtocolViolationException("Expected Version message.");

            if (message.Message is not VersionMessage version)
                throw new ProtocolViolationException("Version payload is missing or invalid.");

            if (version.Network != _network)
            {
                throw new ProtocolViolationException(
                    $"Network mismatch: remote={version.Network}, local={_network}.");
            }

            if (version.Nonce == _localNonce)
                throw new ProtocolViolationException("Connection to self is not allowed.");

            RemoteVersion = version;
            State = NodeHandshakeState.WaitingForVerack;

            return ProtocolMessage.Create(ProtocolMessageCommand.VersionAck);
        }

        private ProtocolMessage? ProcessVerack(ProtocolMessage message)
        {
            if (message.Command != ProtocolMessageCommand.VersionAck)
                throw new ProtocolViolationException("Expected Verack message.");

            State = NodeHandshakeState.Ready;
            return null;
        }

        private static ProtocolMessage? ProcessReady(ProtocolMessage message)
        {
            // Neo rejects Version/Verack after handshake completes.
            if (message.Command is ProtocolMessageCommand.Version or ProtocolMessageCommand.VersionAck)
                throw new ProtocolViolationException("Unexpected handshake message after handshake completed.");

            return null;
        }
    }
}
