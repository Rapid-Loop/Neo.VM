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
using System.Text;

namespace Neo.IO.Hashing
{
    /// <summary>
    /// Implements the 32-bit MurmurHash3 non-cryptographic hash algorithm.
    /// </summary>
    public sealed class Murmur32 : NonCryptographicHashAlgorithm
    {
        private const uint DefaultSeed = 0xdeadc0deu;

        private const uint C1 = 0xcc9e2d51;
        private const uint C2 = 0x1b873593;
        private const uint R1 = 15;
        private const uint R2 = 13;
        private const uint M = 5;
        private const uint N = 0xe6546b64;

        private readonly uint _seed;

        private uint _hash;
        private uint _length;

        // Internal buffer to hold up to 3 bytes between Append() calls
        private readonly byte[] _buffer = new byte[3];
        private int _bufferLength;

        private Murmur32(uint seed = DefaultSeed) : base(sizeof(uint)) // 4 bytes outout
        {
            Reset();

            _seed = seed;
            _hash = _seed;
        }

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the specified data.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 4-byte array containing the hash digest.</returns>
        public static byte[] Hash(byte[] data, uint seed = DefaultSeed) =>
            Hash(data.AsSpan(), seed);

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the specified data.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 4-byte array containing the hash digest.</returns>
        public static byte[] Hash(ReadOnlySpan<byte> data, uint seed = DefaultSeed)
        {
            var hasher = new Murmur32(seed);
            hasher.Append(data);
            return hasher.GetCurrentHash();
        }

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the UTF-8 encoding of the specified text.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>A 4-byte array containing the hash digest.</returns>
        public static byte[] Hash(string text, uint seed = DefaultSeed)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Hash(bytes, seed);
        }

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the specified data and returns it as a <see cref="uint"/>.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="uint"/>.</returns>
        public static uint HashToUInt32(byte[] data, uint seed = DefaultSeed) =>
            HashToUInt32(data.AsSpan(), seed);

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the specified data and returns it as a <see cref="uint"/>.
        /// </summary>
        /// <param name="data">The input data to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="uint"/>.</returns>
        public static uint HashToUInt32(ReadOnlySpan<byte> data, uint seed = DefaultSeed) =>
            BitConverter.ToUInt32(Hash(data, seed), 0);

        /// <summary>
        /// Computes the 32-bit MurmurHash3 of the UTF-8 encoding of the specified text and returns it as a <see cref="uint"/>.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <param name="seed">The hash seed. Defaults to a fixed non-zero seed.</param>
        /// <returns>The hash digest as a <see cref="uint"/>.</returns>
        public static uint HashToUInt32(string text, uint seed = DefaultSeed)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return HashToUInt32(bytes, seed);
        }

        /// <summary>
        /// Appends data to the hash computation.
        /// </summary>
        /// <param name="source">The data to include in the hash.</param>
        public override void Append(ReadOnlySpan<byte> source)
        {
            _length += (uint)source.Length;

            // If we have remaining bytes from a previous Append, process them first
            if (_bufferLength > 0)
            {
                var needed = Math.Min(source.Length, 3 - _bufferLength);
                source[..needed].CopyTo(_buffer.AsSpan(_bufferLength));
                _bufferLength += needed;

                if (_bufferLength != 4)
                    return;
                else
                {
                    var k = BitConverter.ToUInt32(_buffer, 0);
                    ProcessBlock(k);
                    _bufferLength = 0;
                    source = source[needed..];
                }
            }

            // Process full 4-byte blocks
            while (source.Length >= 4)
            {
                var k = BitConverter.ToUInt32(source);
                ProcessBlock(k);
                source = source[4..];
            }

            // Save any remaining bytes for the next Append call or Finalize
            if (source.Length > 0)
            {
                source.CopyTo(_buffer);
                _bufferLength = source.Length;
            }
        }

        /// <summary>
        /// Resets the hasher to its initial state.
        /// </summary>
        public override void Reset()
        {
            _hash = 0;
            _length = 0;
            _bufferLength = 0;
        }

        protected override void GetCurrentHashCore(Span<byte> destination)
        {
            var finalHash = FinalizeHash();
            BitConverter.TryWriteBytes(destination, finalHash);
        }

        protected override void GetHashAndResetCore(Span<byte> destination)
        {
            var finalHash = FinalizeHash();
            BitConverter.TryWriteBytes(destination, finalHash);
            Reset();
        }

        private void ProcessBlock(uint k)
        {
            k *= C1;
            k = (k << (int)R1) | (k >> (32 - (int)R1));
            k *= C2;

            _hash ^= k;
            _hash = ((_hash << (int)R2) | (_hash >> (32 - (int)R2))) * M + N;
        }

        private uint FinalizeHash()
        {
            var tail = 0u;

            // Process remaining bytes
            if (_bufferLength > 0)
            {
                if (_bufferLength >= 3) tail ^= (uint)_buffer[2] << 16;
                if (_bufferLength >= 2) tail ^= (uint)_buffer[1] << 8;
                if (_bufferLength >= 1) tail ^= (uint)_buffer[0];

                tail *= C1;
                tail = (tail << (int)R1) | (tail >> (32 - (int)R1));
                tail *= C2;
                _hash ^= tail;
            }

            // Finalization - mixing the length
            _hash ^= _length;

            _hash ^= _hash >> 16;
            _hash *= 0x85ebca6b;
            _hash ^= _hash >> 13;
            _hash *= 0xc2b2ae35;
            _hash ^= _hash >> 16;

            return _hash;
        }
    }
}
