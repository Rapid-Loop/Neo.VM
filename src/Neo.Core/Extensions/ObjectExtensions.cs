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

using System.Runtime.CompilerServices;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for unmanaged object arrays.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Gets the serialized size of an unmanaged array, including its compact-size length prefix.
        /// </summary>
        /// <typeparam name="TSource">The unmanaged element type.</typeparam>
        /// <param name="source">The array to measure.</param>
        /// <returns>The number of bytes required to serialize the array.</returns>
        public static int GetSerializedSize<TSource>(this TSource[] source)
            where TSource : unmanaged =>
            source.Length.GetCompactSize() +
            (source.Length * Unsafe.SizeOf<TSource>());
    }
}
