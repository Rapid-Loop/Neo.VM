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

using Neo.Core.Cryptography;
using Neo.Core.Serialization;
using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="byte"/> arrays.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Returns a new byte array containing the bitwise XOR of the two arrays.
        /// </summary>
        /// <param name="x">The first operand.</param>
        /// <param name="y">The second operand; must be the same length as <paramref name="x"/>.</param>
        /// <returns>A new array containing the bitwise XOR of <paramref name="x"/> and <paramref name="y"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the arrays have different lengths.</exception>
        public static byte[] Xor(this byte[] x, byte[] y)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(y.Length, x.Length, nameof(y));

            var xorByteArray = GC.AllocateUninitializedArray<byte>(x.Length, false);
            var i = 0;
            var length = x.Length;

            // Use Vector if enough data is there and hardware supports it
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count)
            {
                var vectorSize = Vector<byte>.Count;

                while (i <= length - vectorSize)
                {
                    var vx = new Vector<byte>(x.AsSpan()[i..]);
                    var vy = new Vector<byte>(y.AsSpan()[i..]);
                    var vr = vx ^ vy;

                    vr.CopyTo(xorByteArray.AsSpan()[i..]);

                    i += vectorSize;
                }
            }

            // Remaining bytes and fallback method
            while (i < length)
                xorByteArray[i] = (byte)(x[i] ^ y[i++]);

            return xorByteArray;
        }

        /// <summary>
        /// Converts a byte array to an <see cref="INeoSerializable"/> object.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="data">The byte array to be converted.</param>
        /// <param name="startIndex">The offset into the byte array from which to begin using data.</param>
        /// <returns>The converted <see cref="INeoSerializable"/> object.</returns>
        public static T AsSerializable<T>(this byte[] data, int startIndex = 0)
            where T : class?, INeoSerializable?
        {
            using var ms = new MemoryStream(data, false);
            ms.Seek(startIndex, SeekOrigin.Begin);

            var newObject = (T)typeof(T).CreateInitializedObject();
            newObject.Deserialize(ms);

            return newObject;
        }

        /// <summary>
        /// Gets the serialized size of a byte array, including its compact-size length prefix.
        /// </summary>
        /// <param name="data">The byte array.</param>
        /// <returns>The number of bytes required to serialize <paramref name="data"/>.</returns>
        public static int GetSerializedSize(this byte[] data) =>
            data.Length.GetCompactSize() + data.Length;

        /// <summary>
        /// Computes the script hash of the byte array (Hash160).
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A <see cref="UInt160"/> script hash of the data.</returns>
        public static UInt160 ToScriptHash(this byte[] data) =>
            new(data.ToHash160());

        /// <summary>
        /// Computes the RIPEMD-160 hash of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The 20-byte RIPEMD-160 digest.</returns>
        public static byte[] ToRipeMD160(this byte[] data) =>
            RipeMD160.HashData(data);

        /// <summary>
        /// Computes the SHA-256 hash of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The 32-byte SHA-256 digest.</returns>
        public static byte[] ToSha256(this byte[] data) =>
            SHA256.HashData(data);

        /// <summary>
        /// Computes Hash160 (SHA-256 followed by RIPEMD-160) of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The 20-byte Hash160 digest.</returns>
        public static byte[] ToHash160(this byte[] data) =>
            data.ToSha256().ToRipeMD160();

        /// <summary>
        /// Computes a transaction-style hash of the byte array as a <see cref="UInt256"/>.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A <see cref="UInt256"/> containing the SHA-256 digest.</returns>
        public static UInt256 ToTxHash(this byte[] data) =>
            new(data.ToSha256());

        /// <summary>
        /// Computes a stable integer hash code over the byte array contents.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="seed">The initial hash seed.</param>
        /// <returns>An integer hash code derived from the array contents.</returns>
        public static int ToHashCode(this byte[] data, int seed = 397) =>
            data.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) ^ b)
                );
    }
}
