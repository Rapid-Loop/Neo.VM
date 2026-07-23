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

using Neo.IO.Hashing;
using System;
using System.Collections;

namespace Neo.IO.Data
{
    /// <summary>
    /// A probabilistic set that supports membership tests with a configurable false-positive rate.
    /// </summary>
    public class BloomFilter
    {
        private readonly BitArray _bits;
        private readonly int _bitSize;
        private readonly int _hashCount;
        private readonly uint _tweak;

        /// <summary>
        /// Gets the number of hash functions used by this filter.
        /// </summary>
        public int HashCount => _hashCount;

        /// <summary>
        /// Gets the size of the underlying bit array in bits.
        /// </summary>
        public int BitSize => _bitSize;

        /// <summary>
        /// Gets the tweak value mixed into hash seeds for this filter.
        /// </summary>
        public uint Tweak => _tweak;

        /// <summary>
        /// Creates a Bloom Filter with optimal size and hash count
        /// </summary>
        /// <param name="expectedElements">Expected number of elements to insert</param>
        /// <param name="falsePositiveRate">Desired false positive probability (default 0.01 = 1%)</param>
        /// <param name="tweak">Random tweak for hash seeds (optional)</param>
        /// <param name="elementBytes">The initial elements contained in <see cref="BloomFilter"/></param>
        public BloomFilter(int expectedElements, double falsePositiveRate = 0.01, uint tweak = 0, byte[]? elementBytes = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(expectedElements, nameof(expectedElements));

            _bitSize = (int)Math.Ceiling(-expectedElements * Math.Log(falsePositiveRate)
                        / (Math.Log(2) * Math.Log(2)));

            _hashCount = Math.Max(1, (int)Math.Ceiling((_bitSize / (double)expectedElements) * Math.Log(2)));

            _bits = elementBytes is null ?
                new(_bitSize) :
                new(elementBytes) { Length = _bitSize };

            _tweak = tweak;
        }

        /// <summary>
        /// Adds the specified data to the filter.
        /// </summary>
        /// <param name="data">The element data to insert.</param>
        public void Add(ReadOnlySpan<byte> data)
        {
            for (var i = 0; i < _hashCount; i++)
            {
                var hash = Murmur32.HashToUInt32(data, (uint)i * 0xfba4c795u + _tweak);
                var index = (int)(hash % (uint)_bitSize);

                _bits[index] = true;
            }
        }

        /// <summary>
        /// Tests whether the specified data may be present in the filter.
        /// </summary>
        /// <param name="data">The element data to test.</param>
        /// <returns>
        /// <see langword="false"/> if the element is definitely not present;
        /// otherwise <see langword="true"/> if it is probably present.
        /// </returns>
        public bool Contains(ReadOnlySpan<byte> data)
        {
            for (var i = 0; i < _hashCount; i++)
            {
                var hash = Murmur32.HashToUInt32(data, (uint)i * 0xfba4c795u + _tweak);
                var index = (int)(hash % (uint)_bitSize);

                if (_bits[index] == false)
                    return false; // Definitely not present
            }

            return true; // Probably present
        }

        /// <summary>
        /// Adds the specified data to the filter.
        /// </summary>
        /// <param name="data">The element data to insert.</param>
        public void Add(byte[] data) =>
            Add(data.AsSpan());

        /// <summary>
        /// Tests whether the specified data may be present in the filter.
        /// </summary>
        /// <param name="data">The element data to test.</param>
        /// <returns>
        /// <see langword="false"/> if the element is definitely not present;
        /// otherwise <see langword="true"/> if it is probably present.
        /// </returns>
        public bool Contains(byte[] data) =>
            Contains(data.AsSpan());

        /// <summary>
        /// Estimates the false-positive probability after the specified number of insertions.
        /// </summary>
        /// <param name="insertedCount">The number of elements that have been inserted.</param>
        /// <returns>The approximate false-positive probability in the range [0, 1].</returns>
        public double GetFalsePositiveProbability(int insertedCount)
        {
            return Math.Pow(1 - Math.Exp(-_hashCount * insertedCount / (double)_bitSize), _hashCount);
        }

        /// <summary>
        /// Copies the underlying bit array into the provided buffer.
        /// </summary>
        /// <param name="bytes">The destination buffer that receives the bit array contents.</param>
        public void GetBits(byte[] bytes) =>
            _bits.CopyTo(bytes, 0);
    }
}
