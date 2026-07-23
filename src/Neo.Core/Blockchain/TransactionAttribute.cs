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

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// Base type for optional transaction attributes carried on a <see cref="Transaction"/>.
    /// </summary>
    public abstract class TransactionAttribute : INeoSerializable
    {
        /// <summary>
        /// The type of the attribute.
        /// </summary>
        public abstract TransactionAttributeType Type { get; }

        /// <summary>
        /// Indicates whether multiple instances of this attribute are allowed.
        /// </summary>
        public abstract bool AllowMultiple { get; }

        /// <summary>
        /// Gets the serialized size of this attribute in bytes.
        /// </summary>
        public virtual int Size => sizeof(TransactionAttributeType);

        /// <summary>
        /// Deserializes this attribute from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="FormatException">The attribute type is not defined.</exception>
        public virtual void Deserialize(Stream reader)
        {
            var type = reader.Read<TransactionAttributeType>();

            if (Enum.IsDefined(type) == false)
                throw new FormatException($"[{nameof(TransactionAttribute)}] Invalid {nameof(Type)} \'{type:d}\'.");
        }

        /// <summary>
        /// Serializes this attribute to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public virtual void Serialize(Stream writer)
        {
            writer.Write(Type);
        }
    }
}
