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
using System.IO;
using System.Security.Cryptography;

namespace Neo.Core.Net
{
    /// <summary>
    /// Represents a P2P network message exchanged between peers.
    /// </summary>
    public sealed class Message : INeoSerializable
    {
        /// <summary>
        /// The size of the message header in bytes (Magic + Command + Length + Checksum).
        /// </summary>
        public const int HeaderSize = 13;

        /// <summary>
        /// Maximum allowed payload size to prevent resource exhaustion.
        /// </summary>
        public const int MaxPayloadSize = 0x02000000; // 32 MB

        /// <summary>
        /// Network magic number identifying the network.
        /// </summary>
        public uint Magic { get; set; }

        /// <summary>
        /// The command type of this message.
        /// </summary>
        public MessageCommandType Command { get; set; }

        /// <summary>
        /// The payload data of the message.
        /// </summary>
        public byte[] Payload { get; set; } = [];

        /// <inheritdoc/>
        public int Size => HeaderSize + Payload.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class with the specified values.
        /// </summary>
        /// <param name="magic">The network magic number.</param>
        /// <param name="command">The message command.</param>
        /// <param name="payload">The optional payload.</param>
        public Message(uint magic, MessageCommandType command, byte[]? payload = null)
        {
            Magic = magic;
            Command = command;
            Payload = payload ?? [];
        }

        /// <inheritdoc/>
        public void Serialize(Stream writer)
        {
            writer.Write(Magic);
            writer.Write((byte)Command);
            writer.Write((uint)Payload.Length);
            writer.Write(ComputeChecksum(Payload));
            if (Payload.Length > 0)
                writer.Write(Payload.AsSpan());
        }

        /// <inheritdoc/>
        public void Deserialize(Stream reader)
        {
            Magic = reader.Read<uint>();
            Command = (MessageCommandType)reader.ReadByte();
            var length = reader.Read<uint>();
            if (length > MaxPayloadSize)
                throw new FormatException($"Payload length {length} exceeds maximum allowed size of {MaxPayloadSize}.");

            var checksum = reader.Read<uint>();
            Payload = length > 0 ? new byte[length] : [];
            if (length > 0)
            {
                reader.ReadExactly(Payload);
                if (ComputeChecksum(Payload) != checksum)
                    throw new FormatException("Message checksum mismatch.");
            }
        }

        /// <summary>
        /// Computes a simple checksum for the payload using SHA-256 truncated to 4 bytes.
        /// </summary>
        public static uint ComputeChecksum(ReadOnlySpan<byte> data)
        {
            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(data, hash);
            return BitConverter.ToUInt32(hash);
        }
    }
}
