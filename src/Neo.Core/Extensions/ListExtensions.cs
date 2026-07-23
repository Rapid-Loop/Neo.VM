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

using System.Collections.Generic;
using System.Linq;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for list types.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Computes a stable integer hash code over the list elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the list elements.</typeparam>
        /// <param name="source">The list to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the list contents.</returns>
        public static int ToHashCode<TSource>(this IList<TSource> source, int seed = 397) =>
            source.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) ^ (b?.GetHashCode() ?? 0))
                );

        /// <summary>
        /// Computes a stable integer hash code over the read-only list elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the list elements.</typeparam>
        /// <param name="source">The list to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the list contents.</returns>
        public static int ToHashCode<TSource>(this IReadOnlyList<TSource> source, int seed = 397) =>
            source.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) ^ (b?.GetHashCode() ?? 0))
                );
    }
}
