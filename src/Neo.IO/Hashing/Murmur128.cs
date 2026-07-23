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
using System.IO.Hashing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Neo.IO.Hashing
{
    /// <summary>
    /// Implements the 128-bit MurmurHash3 non-cryptographic hash algorithm.
    /// </summary>
    public class Murmur128 : NonCryptographicHashAlgorithm
    {
        private const uint DefaultSeed = 0xdeadc0deu;

        private const ulong C1 = 0x87c37b91114253d5ul;
        private const ulong C2 = 0x4cf5ad432745937ful;

        private readonly uint _seed;

        private ulong _h1;
        private ulong _h2;
        private ulong _length;

        private Murmur128(uint seed = DefaultSeed) : base(16)
        {
            _seed = seed;
            Reset();
        }

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the specified data.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 16-byte array containing the hash digest.</returns>
        public static byte[] Hash(byte[] data, uint seed = DefaultSeed) =>
                Hash(data.AsSpan(), seed);

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the specified data.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 16-byte array containing the hash digest.</returns>
        public static byte[] Hash(ReadOnlySpan<byte> data, uint seed = DefaultSeed)
        {
            var hasher = new Murmur128(seed);
            hasher.Append(data);
            return hasher.GetCurrentHash();
        }

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the UTF-8 encoding of the specified text.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 16-byte array containing the hash digest.</returns>
        public static byte[] Hash(string text, uint seed = DefaultSeed)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Hash(bytes, seed);
        }

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the specified data and returns it as a <see cref="UInt128"/>.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="UInt128"/>.</returns>
        public static UInt128 HashToUInt128(byte[] data, uint seed = DefaultSeed) =>
            HashToUInt128(data.AsSpan(), seed);

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the specified data and returns it as a <see cref="UInt128"/>.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="UInt128"/>.</returns>
        public static UInt128 HashToUInt128(ReadOnlySpan<byte> data, uint seed = DefaultSeed) =>
            BitConverter.ToUInt128(Hash(data, seed), 0);

        /// <summary>
        /// Computes the 128-bit MurmurHash3 of the UTF-8 encoding of the specified text and returns it as a <see cref="UInt128"/>.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="UInt128"/>.</returns>
        public static UInt128 HashToUInt128(string text, uint seed = DefaultSeed)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return HashToUInt128(bytes, seed);
        }

        /// <summary>
        /// Resets the hasher to its initial state for the configured seed.
        /// </summary>
        public override void Reset()
        {
            _h1 = _seed;
            _h2 = _seed;
            _length = 0;
        }

        /// <summary>
        /// Appends data to the hash computation.
        /// </summary>
        /// <param name="source">The data to include in the hash.</param>
        public override void Append(ReadOnlySpan<byte> source)
        {
            _length += (uint)source.Length;

            var index = 0;
            var bytesRemaining = source.Length;

            for (; bytesRemaining >= 16; index += 16, bytesRemaining -= 16)
            {
                var k1 = Unsafe.ReadUnaligned<ulong>(in source[index]);
                var k2 = Unsafe.ReadUnaligned<ulong>(in source[index + 8]);

                ProcessBlock(k1, k2);
            }

            // Process leftover bytes if the source isn't an exact multiple of 16 bytes
            if (bytesRemaining > 0)
            {
                ulong k1 = 0;
                ulong k2 = 0;

                if (bytesRemaining >= 1) k1 ^= (ulong)source[index + 0];
                if (bytesRemaining >= 2) k1 ^= (ulong)source[index + 1] << 8;
                if (bytesRemaining >= 3) k1 ^= (ulong)source[index + 2] << 16;
                if (bytesRemaining >= 4) k1 ^= (ulong)source[index + 3] << 24;
                if (bytesRemaining >= 5) k1 ^= (ulong)source[index + 4] << 32;
                if (bytesRemaining >= 6) k1 ^= (ulong)source[index + 5] << 40;
                if (bytesRemaining >= 7) k1 ^= (ulong)source[index + 6] << 48;
                if (bytesRemaining >= 8) k1 ^= (ulong)source[index + 7] << 56;

                if (bytesRemaining >= 9) k2 ^= (ulong)source[index + 8];
                if (bytesRemaining >= 10) k2 ^= (ulong)source[index + 9] << 8;
                if (bytesRemaining >= 11) k2 ^= (ulong)source[index + 10] << 16;
                if (bytesRemaining >= 12) k2 ^= (ulong)source[index + 11] << 24;
                if (bytesRemaining >= 13) k2 ^= (ulong)source[index + 12] << 32;
                if (bytesRemaining >= 14) k2 ^= (ulong)source[index + 13] << 40;
                if (bytesRemaining >= 15) k2 ^= (ulong)source[index + 14] << 48;

                _h1 ^= MixK1(k1);
                _h2 ^= MixK2(k2);
            }
        }

        protected override void GetCurrentHashCore(Span<byte> destination)
        {
            // Finalization
            _h1 ^= _length;
            _h2 ^= _length;

            _h1 += _h2;
            _h2 += _h1;

            _h1 = MixFinal(_h1);
            _h2 = MixFinal(_h2);

            _h1 += _h2;
            _h2 += _h1;

            Unsafe.WriteUnaligned(ref destination[0], _h1);
            Unsafe.WriteUnaligned(ref destination[8], _h2);
        }

        protected override void GetHashAndResetCore(Span<byte> destination)
        {
            GetCurrentHashCore(destination);
            Reset(); // Reset after reading
        }

        private void ProcessBlock(ulong k1, ulong k2)
        {
            k1 *= C1;
            k1 = BitOperations.RotateLeft(k1, 31);
            k1 *= C2;
            _h1 ^= k1;

            _h1 = BitOperations.RotateLeft(_h1, 27);
            _h1 += _h2;
            _h1 = _h1 * 5 + 0x52dce729;

            k2 *= C2;
            k2 = BitOperations.RotateLeft(k2, 33);
            k2 *= C1;
            _h2 ^= k2;

            _h2 = BitOperations.RotateLeft(_h2, 31);
            _h2 += _h1;
            _h2 = _h2 * 5 + 0x38495ab5;
        }

        private static ulong MixK1(ulong k1)
        {
            k1 *= C1;
            k1 = BitOperations.RotateLeft(k1, 31);
            k1 *= C2;
            return k1;
        }

        private static ulong MixK2(ulong k2)
        {
            k2 *= C2;
            k2 = BitOperations.RotateLeft(k2, 33);
            k2 *= C1;
            return k2;
        }

        private static ulong MixFinal(ulong k)
        {
            k ^= k >> 33;
            k *= 0xff51afd7ed558ccdul;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53ul;
            k ^= k >> 33;
            return k;
        }
    }
}
