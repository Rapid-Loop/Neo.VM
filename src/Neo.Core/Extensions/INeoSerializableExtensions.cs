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

using Neo.Core.Serialization;
using System.IO;
using System.Linq;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="INeoSerializable"/> types.
    /// </summary>
    public static class INeoSerializableExtensions
    {
        /// <summary>
        /// Converts an <see cref="INeoSerializable"/> object to a byte array.
        /// </summary>
        /// <param name="source">The <see cref="INeoSerializable"/> object to be converted.</param>
        /// <returns>The converted byte array.</returns>
        public static byte[] ToArray(this INeoSerializable source)
        {
            using var ms = new MemoryStream();

            source.Serialize(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Gets the serialized size of an array of <see cref="INeoSerializable"/> objects, including the compact-size length prefix.
        /// </summary>
        /// <param name="source">The array of serializable objects.</param>
        /// <returns>The number of bytes required to serialize the array.</returns>
        public static int GetSerializedSize(this INeoSerializable[] source) =>
            source.Length.GetCompactSize() +
            source.Sum(static s => s.Size);
    }
}
