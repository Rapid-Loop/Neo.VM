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
using Neo.Core.Factory;
using Neo.Core.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neo.Core.Net.Message
{
    /// <summary>
    /// Payload for the P2P Version handshake message.
    /// </summary>
    public class VersionMessage : INeoSerializable
    {
        /// <summary>
        /// The maximum number of capabilities allowed in a version payload.
        /// </summary>
        public const int MaxCapabilities = 32;

        private readonly static string s_version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

        /// <summary>
        /// Gets the network magic of the sending node.
        /// </summary>
        public uint Network { get; private set; }

        /// <summary>
        /// Gets the protocol version of the sending node.
        /// </summary>
        public uint Version { get; private set; } = 0u;

        /// <summary>
        /// Gets the Unix timestamp (seconds) when this payload was created.
        /// </summary>
        public uint Timestamp { get; private set; } = CoreUtilities.ToUnixTimeSeconds(DateTime.UtcNow);

        /// <summary>
        /// Gets the random nonce identifying the sending node.
        /// </summary>
        public uint Nonce { get; private set; } = RandomNumberFactory.NextUInt32();

        /// <summary>
        /// Gets a value indicating whether the peer allows payload compression.
        /// </summary>
        public bool AllowCompression => !Capabilities.Any(static a => a.Type == NodeCapabilityType.DisableCompression);

        /// <summary>
        /// Gets the user agent string of the sending node.
        /// </summary>
        public string UserAgent { get; private set; } = $"/RapidLoop:{s_version}/";

        /// <summary>
        /// Gets the node capabilities advertised by the sender.
        /// </summary>
        public NodeCapabilityMessage[] Capabilities { get; private set; } = [];

        /// <summary>
        /// Gets the serialized size of this payload in bytes.
        /// </summary>
        public int Size =>
            sizeof(uint) + // Network
            sizeof(uint) + // Version
            sizeof(uint) + // Timestamp
            sizeof(uint) + // Nonce
            UserAgent.GetSerializedSize() +   // UserAgent
            Capabilities.GetSerializedSize(); // Node Capabilities

        /// <summary>
        /// Creates a version payload for the specified network and capabilities.
        /// </summary>
        /// <param name="network">The network magic number.</param>
        /// <param name="capabilities">The node capabilities to advertise.</param>
        /// <returns>A new <see cref="VersionMessage"/>.</returns>
        public static VersionMessage Create(uint network, params NodeCapabilityMessage[] capabilities) =>
            new()
            {
                Network = network,
                Capabilities = capabilities,
            };

        /// <summary>
        /// Creates a version payload with an explicit node nonce.
        /// </summary>
        /// <param name="network">The network magic number.</param>
        /// <param name="nonce">The local node nonce.</param>
        /// <param name="capabilities">The node capabilities to advertise.</param>
        /// <returns>A new <see cref="VersionMessage"/>.</returns>
        public static VersionMessage Create(uint network, uint nonce, params NodeCapabilityMessage[] capabilities) =>
            new()
            {
                Network = network,
                Nonce = nonce,
                Capabilities = capabilities,
            };

        /// <summary>
        /// Deserializes this payload from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="FormatException">Capability count exceeds the limit or capabilities are duplicated.</exception>
        public void Deserialize(Stream reader)
        {
            Network = reader.Read<uint>();
            Version = reader.Read<uint>();
            Timestamp = reader.Read<uint>();
            Nonce = reader.Read<uint>();
            UserAgent = reader.ReadString();

            var capabilityCount = reader.ReadCompact<int>();
            if (capabilityCount > MaxCapabilities)
                throw new FormatException($"Invalid capabilities count: {capabilityCount}.");

            Capabilities = new NodeCapabilityMessage[capabilityCount];
            for (var i = 0; i < Capabilities.Length; i++)
                Capabilities[i] = NodeCapabilityMessage.DeserializeFrom(reader);

            // Duplicate check is by capability Type (Neo VersionPayload).
            if (Capabilities.Select(static c => c.Type).Distinct().Count() != Capabilities.Length)
                throw new FormatException("Duplicate node capabilities.");
        }

        /// <summary>
        /// Serializes this payload to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(Network);
            writer.Write(Version);
            writer.Write(Timestamp);
            writer.Write(Nonce);
            writer.Write(UserAgent);
            writer.WriteObjects(Capabilities);
        }
    }
}
