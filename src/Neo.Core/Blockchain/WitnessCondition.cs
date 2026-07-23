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
using System.Runtime.CompilerServices;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// Base type for conditions evaluated as part of a <see cref="WitnessRule"/>.
    /// </summary>
    public abstract class WitnessCondition : INeoSerializable
    {
        /// <summary>
        /// The type of the <see cref="WitnessCondition"/>.
        /// </summary>
        public abstract WitnessConditionType Type { get; }

        /// <summary>
        /// Gets the serialized size of this condition in bytes.
        /// </summary>
        public virtual int Size => Unsafe.SizeOf<WitnessConditionType>();

        /// <summary>
        /// Deserializes this condition from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="FormatException">The type byte does not match <see cref="Type"/>.</exception>
        public void Deserialize(Stream reader)
        {
            var type = reader.Read<WitnessConditionType>();

            if (type != Type)
                throw new FormatException($"[{nameof(WitnessCondition)}::{nameof(Type)}] Value \'{type}\' does not match \'{Type}\'.");
        }

        /// <summary>
        /// Serializes this condition to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(Type);
        }
    }
}
