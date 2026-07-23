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

using System.IO;

namespace Neo.Core.Serialization
{
    /// <summary>
    /// Defines binary serialization and deserialization for Neo wire and storage types.
    /// </summary>
    public interface INeoSerializable
    {
        /// <summary>
        /// The size of the object in bytes after serialization.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Serializes the object using the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="writer">The <see cref="Stream"/> for writing data.</param>
        void Serialize(Stream writer);

        /// <summary>
        /// Deserializes the object using the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="reader">The <see cref="Stream"/> for reading data.</param>
        void Deserialize(Stream reader);
    }
}
