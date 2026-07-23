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

namespace Neo.Platform.Storage.Extensions
{
    /// <summary>
    /// Extension methods that append <see cref="INeoSerializable"/> values to a <see cref="StorageKeyBuilder"/>.
    /// </summary>
    public static class StorageKeyBuilderExtensions
    {
        /// <summary>
        /// Serializes <paramref name="data"/> and appends the resulting bytes to the builder.
        /// </summary>
        /// <param name="builder">The key builder.</param>
        /// <param name="data">The serializable value to append.</param>
        /// <returns>The same <paramref name="builder"/> for chaining.</returns>
        public static StorageKeyBuilder Append(this StorageKeyBuilder builder, INeoSerializable data) =>
            builder.Append(data.ToArray());

        /// <summary>
        /// Appends a prefix byte, then serializes and appends <paramref name="data"/>.
        /// </summary>
        /// <param name="builder">The key builder.</param>
        /// <param name="prefix">The prefix byte to append first.</param>
        /// <param name="data">The serializable value to append after the prefix.</param>
        /// <returns>The same <paramref name="builder"/> for chaining.</returns>
        public static StorageKeyBuilder Append(this StorageKeyBuilder builder, byte prefix, INeoSerializable data) =>
            builder.Append(prefix).Append(data.ToArray());
    }
}
