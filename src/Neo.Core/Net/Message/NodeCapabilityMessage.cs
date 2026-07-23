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

namespace Neo.Core.Net.Message
{
    /// <summary>
    /// Neo N3 node capability on the wire:
    /// <c>[Type:1][type-specific payload]</c>
    /// Type is read once in <see cref="DeserializeFrom"/>; subclasses only read/write payload.
    /// </summary>
    public abstract class NodeCapabilityMessage : INeoSerializable
    {
        /// <summary>
        /// Gets the capability type of this message.
        /// </summary>
        public abstract NodeCapabilityType Type { get; }

        /// <summary>
        /// Gets the serialized size of this capability in bytes.
        /// </summary>
        public virtual int Size =>
            sizeof(NodeCapabilityType);

        /// <summary>
        /// Reads one capability: type byte, then subtype payload (Neo <c>DeserializeFrom</c>).
        /// </summary>
        public static NodeCapabilityMessage DeserializeFrom(Stream reader)
        {
            NodeCapabilityMessage capability = reader.Read<NodeCapabilityType>() switch
            {
                NodeCapabilityType.TcpServer => new ServerCapabilityMessage(),
                NodeCapabilityType.FullNode => new FullNodeCapabilityMessage(),
                NodeCapabilityType.ArchivalNode => new ArchivalNodeCapabilityMessage(),
                NodeCapabilityType.DisableCompression => new DisableCompressionCapabilityMessage(),
                _ => throw new FormatException("Unknown Capability"),
            };

            // Type already consumed — payload only.
            capability.DeserializeWithoutType(reader);

            return capability;
        }

        /// <summary>
        /// Full deserialize when the type byte is still ahead in the stream
        /// (e.g. standalone capability as <see cref="INeoSerializable"/>).
        /// </summary>
        public void Deserialize(Stream reader)
        {
            var type = reader.Read<NodeCapabilityType>();

            if (type != Type)
                throw new FormatException($"Invalid capability type: {type}");

            DeserializeWithoutType(reader);
        }

        /// <summary>
        /// Serializes this capability to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(Type);
            SerializeWithoutType(writer);
        }

        /// <summary>
        /// Deserializes only the type-specific payload (type byte already consumed).
        /// </summary>
        protected abstract void DeserializeWithoutType(Stream reader);

        /// <summary>
        /// Serializes only the type-specific payload (type written by <see cref="Serialize"/>).
        /// </summary>
        protected abstract void SerializeWithoutType(Stream writer);
    }
}
