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
using System.Diagnostics.CodeAnalysis;

namespace Neo.Core.Storage
{
    /// <summary>
    /// A read-only key/value store of binary entries.
    /// </summary>
    public interface IReadOnlyStore : IDisposable
    {
        /// <summary>
        /// Determines whether the store contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><see langword="true"/> if the key exists; otherwise, <see langword="false"/>.</returns>
        bool ContainsKey(ReadOnlySpan<byte> key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the key, or <see langword="null"/> if not found.</returns>
        byte[]? Get(ReadOnlySpan<byte> key);

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, the value if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
        bool TryGet(ReadOnlySpan<byte> key, [NotNullWhen(true)] out byte[]? value);

        /// <summary>
        /// Seeks key/value pairs starting at the specified key or prefix.
        /// </summary>
        /// <param name="keyOrPrefix">The key or prefix at which to begin seeking.</param>
        /// <param name="seekFromEnd">
        /// <see langword="true"/> to seek in reverse from the end; otherwise forward.
        /// </param>
        /// <returns>An enumerable of matching key/value pairs.</returns>
        IEnumerable<KeyValuePair<byte[], byte[]>> Seek(ReadOnlyMemory<byte> keyOrPrefix, bool seekFromEnd = false);
    }
}
