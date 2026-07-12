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
    public static class ByteExtensions
    {
        /// <summary>
        /// Returns a new byte array containing the bitwise XOR of the two arrays.
        /// </summary>
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

        public static int GetSerializedSize(this byte[] data) =>
            data.Length.GetCompactSize() + data.Length;

        public static UInt160 ToScriptHash(this byte[] data) =>
            new(data.ToHash160());

        public static byte[] ToRipeMD160(this byte[] data) =>
            RipeMD160.HashData(data);

        public static byte[] ToSha256(this byte[] data) =>
            SHA256.HashData(data);

        public static byte[] ToHash160(this byte[] data) =>
            data.ToSha256().ToRipeMD160();

        public static UInt256 ToTxHash(this byte[] data) =>
            new(data.ToSha256());
        public static int ToHashCode(this byte[] data, int seed = 397) =>
            data.Aggregate(seed,
                (hash, b) =>
                        unchecked((hash * 31) ^ b)
                );

    }
}
