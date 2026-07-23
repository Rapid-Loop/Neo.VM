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
using Neo.Core.Storage;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Storage
{
    /// <summary>
    /// Extension methods for reading and writing <see cref="INeoSerializable"/> values from stores.
    /// </summary>
    public static class StoreExtensions
    {
        /// <summary>
        /// Serializes <paramref name="value"/> and stores it under <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The serializable value type.</typeparam>
        /// <param name="store">The store to write to.</param>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to serialize and store.</param>
        public static void Put<T>(this IStore store, ReadOnlySpan<byte> key, T value)
            where T : INeoSerializable =>
            store.Put(key, value.ToArray());

        /// <summary>
        /// Reads and deserializes a value of type <typeparamref name="TResult"/> for <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="TResult">The serializable result type.</typeparam>
        /// <param name="store">The store to read from.</param>
        /// <param name="key">The storage key.</param>
        /// <returns>
        /// The deserialized value, or <see langword="null"/> if the key is not present.
        /// </returns>
        public static TResult? Get<TResult>(this IReadOnlyStore store, ReadOnlySpan<byte> key)
            where TResult : class?, INeoSerializable =>
            store.Get(key)?.AsSerializable<TResult>();

        /// <summary>
        /// Attempts to read and deserialize a value of type <typeparamref name="T"/> for <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The serializable value type.</typeparam>
        /// <param name="store">The store to read from.</param>
        /// <param name="key">The storage key.</param>
        /// <param name="value">
        /// When this method returns <see langword="true"/>, contains the deserialized value;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the key was found and deserialized; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryGet<T>(this IReadOnlyStore store, ReadOnlySpan<byte> key, [NotNullWhen(true)] out T? value)
            where T : class?, INeoSerializable
        {
            value = default;
            if (store.TryGet(key, out var valueBytes))
            {
                value = valueBytes.AsSerializable<T>();
                return true;
            }

            return false;
        }
    }
}
