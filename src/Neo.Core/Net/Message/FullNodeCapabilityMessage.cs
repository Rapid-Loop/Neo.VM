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
using System.IO;

namespace Neo.Core.Net.Message
{
    /// <summary>
    /// Advertises that the node has complete current state at a given start height.
    /// </summary>
    public class FullNodeCapabilityMessage : NodeCapabilityMessage
    {
        /// <summary>
        /// Gets the capability type (<see cref="NodeCapabilityType.FullNode"/>).
        /// </summary>
        public override NodeCapabilityType Type => NodeCapabilityType.FullNode;

        /// <summary>
        /// Gets the block height at which this node reports full state.
        /// </summary>
        public uint StartHeight { get; private set; }

        /// <summary>
        /// Gets the serialized size of this capability in bytes.
        /// </summary>
        public override int Size =>
            base.Size +
            sizeof(uint);

        /// <summary>
        /// Initializes an empty full-node capability for deserialization.
        /// </summary>
        public FullNodeCapabilityMessage()
        {
        }

        /// <summary>
        /// Initializes a full-node capability with the specified start height.
        /// </summary>
        /// <param name="startHeight">The reported full-state block height.</param>
        public FullNodeCapabilityMessage(uint startHeight)
        {
            StartHeight = startHeight;
        }

        protected override void DeserializeWithoutType(Stream reader)
        {
            StartHeight = reader.Read<uint>();
        }

        protected override void SerializeWithoutType(Stream writer)
        {
            writer.Write(StartHeight);
        }
    }
}
