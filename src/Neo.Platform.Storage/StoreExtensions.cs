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
using Neo.Platform.Storage.Interface;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Storage
{
    public static class StoreExtensions
    {
        public static void Put<T>(this IStore store, ReadOnlySpan<byte> key, T value)
            where T : INeoSerializable =>
            store.Put(key, value.ToArray());

        public static TResult? Get<TResult>(this IReadOnlyStore store, ReadOnlySpan<byte> key)
            where TResult : class?, INeoSerializable =>
            store.Get(key)?.AsSerializable<TResult>();

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
