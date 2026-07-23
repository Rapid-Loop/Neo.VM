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

using K4os.Compression.LZ4;
using Neo.Core.Serialization;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Gets the serialized size of a span of unmanaged values, including the compact-size length prefix.
        /// </summary>
        /// <typeparam name="TSource">The unmanaged element type.</typeparam>
        /// <param name="span">The span to measure.</param>
        /// <returns>The number of bytes required to serialize the span contents.</returns>
        public static int GetSerializedSize<TSource>(this Span<TSource> span)
            where TSource : unmanaged =>
            span.Length.GetCompactSize() +
            (span.Length * Unsafe.SizeOf<TSource>());

        /// <summary>
        /// Gets the serialized size of a read-only span of unmanaged values, including the compact-size length prefix.
        /// </summary>
        /// <typeparam name="TSource">The unmanaged element type.</typeparam>
        /// <param name="readOnlySpan">The span to measure.</param>
        /// <returns>The number of bytes required to serialize the span contents.</returns>
        public static int GetSerializedSize<TSource>(this ReadOnlySpan<TSource> readOnlySpan)
            where TSource : unmanaged =>
            readOnlySpan.Length.GetCompactSize() +
            (readOnlySpan.Length * Unsafe.SizeOf<TSource>());

        /// <summary>
        /// Converts a byte span to an <see cref="INeoSerializable"/> object.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="span">The bytes to be converted.</param>
        /// <param name="startIndex">The offset into the byte data from which to begin deserialization.</param>
        /// <returns>The converted <see cref="INeoSerializable"/> object.</returns>
        public static T? AsSerializable<T>(this ReadOnlySpan<byte> span, int startIndex = 0)
            where T : class?, INeoSerializable? =>
            span.ToArray()
                .AsSerializable<T>(startIndex);

        /// <summary>
        /// Applies an accumulator function over a span.
        /// </summary>
        /// <typeparam name="TSource">The type of the span elements.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="source">The span to aggregate.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <returns>The final accumulator value.</returns>
        public static TAccumulate Aggregate<TSource, TAccumulate>(this Span<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var val = seed;

            ReadOnlySpan<TSource> readOnlySpan = source;

            for (var i = 0; i < readOnlySpan.Length; i++)
            {
                var arg = readOnlySpan[i];
                val = func(val, arg);
            }

            return val;
        }

        /// <summary>
        /// Applies an accumulator function over a read-only span.
        /// </summary>
        /// <typeparam name="TSource">The type of the span elements.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="source">The span to aggregate.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <returns>The final accumulator value.</returns>
        public static TAccumulate Aggregate<TSource, TAccumulate>(this ReadOnlySpan<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var val = seed;

            for (var i = 0; i < source.Length; i++)
            {
                var arg = source[i];
                val = func(val, arg);
            }

            return val;
        }

        /// <summary>
        /// Computes a stable integer hash code over the byte span contents.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the span contents.</returns>
        public static int ToHashCode(this ReadOnlySpan<byte> data, int seed = 397) =>
            data.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) ^ b)
                );

        /// <summary>
        /// Compresses the span using LZ4, prefixing the output with the original length as a little-endian <see cref="int"/>.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>A span containing the original length followed by the LZ4-compressed payload.</returns>
        public static Span<byte> ToLz4Compress(this ReadOnlySpan<byte> data)
        {
            var maxLength = LZ4Codec.MaximumOutputSize(data.Length);
            Span<byte> buffer = new byte[sizeof(uint) + maxLength];

            BinaryPrimitives.WriteInt32LittleEndian(buffer, data.Length);

            var length = LZ4Codec.Encode(data, buffer[sizeof(uint)..]);

            return buffer[..(sizeof(uint) + length)];
        }

        /// <summary>
        /// Decompresses an LZ4 payload previously produced by <see cref="ToLz4Compress"/>.
        /// </summary>
        /// <param name="data">The compressed data, beginning with a little-endian original length.</param>
        /// <returns>The decompressed bytes.</returns>
        /// <exception cref="FormatException">
        /// Thrown when the decompressed length does not match the length prefix.
        /// </exception>
        public static Span<byte> ToLz4Decompress(this ReadOnlySpan<byte> data)
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(data);
            Span<byte> result = new byte[length];
            var decodedBytes = LZ4Codec.Decode(data[4..], result);

            if (decodedBytes != length)
                throw new FormatException($"Length \'{length}\' does not match the decompressed data length \'{decodedBytes}\'.");

            return result;
        }
    }
}
