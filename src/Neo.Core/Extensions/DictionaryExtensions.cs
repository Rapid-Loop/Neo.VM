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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for dictionary types.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified key, or adds a new value created by the factory.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="source">The dictionary to query or update.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="valueFactory">A factory used to create the value when the key is missing.</param>
        /// <returns>The existing or newly created value for <paramref name="key"/>.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (source.TryGetValue(key, out var value) == false)
            {
                value = valueFactory(key);
                source[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Computes a stable integer hash code over the dictionary entries.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="source">The dictionary to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the dictionary contents.</returns>
        public static int ToHashCode<TKey, TValue>(this IDictionary<TKey, TValue> source, int seed = 397) =>
            source.Aggregate(seed,
                (hash, b) =>
                       unchecked((hash * 31) + ((b.Key?.GetHashCode() ?? 0) ^ (b.Value?.GetHashCode() ?? 0)))
                );

        /// <summary>
        /// Computes a stable integer hash code over the read-only dictionary entries.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="source">The dictionary to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the dictionary contents.</returns>
        public static int ToHashCode<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, int seed = 397) =>
            source.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) + ((b.Key?.GetHashCode() ?? 0) ^ (b.Value?.GetHashCode() ?? 0)))
                );
    }
}
