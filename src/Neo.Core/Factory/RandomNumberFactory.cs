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
using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

namespace Neo.Core.Factory
{
    /// <summary>
    /// Factory methods for generating random numbers and byte sequences.
    /// </summary>
    public static class RandomNumberFactory
    {
        /// <summary>
        /// Returns a non-negative random <see cref="sbyte"/> less than <see cref="sbyte.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="sbyte.MaxValue"/>).</returns>
        public static sbyte NextSByte() =>
            NextSByte(0, sbyte.MaxValue);

        /// <summary>
        /// Returns a non-negative random <see cref="sbyte"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is negative.</exception>
        public static sbyte NextSByte(sbyte maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue));

            return NextSByte(0, maxValue);
        }

        /// <summary>
        /// Returns a random <see cref="sbyte"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static sbyte NextSByte(sbyte minValue, sbyte maxValue)
        {
            if (minValue == maxValue) return maxValue;

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            return (sbyte)(NextUInt32((uint)(maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Returns a random <see cref="byte"/> less than <see cref="byte.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="byte.MaxValue"/>).</returns>
        public static byte NextByte() =>
            NextByte(0, byte.MaxValue);

        /// <summary>
        /// Returns a random <see cref="byte"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        public static byte NextByte(byte maxValue) =>
            NextByte(0, maxValue);

        /// <summary>
        /// Returns a random <see cref="byte"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static byte NextByte(byte minValue, byte maxValue)
        {
            if (minValue == maxValue) return maxValue;

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            return (byte)(NextUInt32((uint)(maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Returns a non-negative random <see cref="short"/> less than <see cref="short.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="short.MaxValue"/>).</returns>
        public static short NextInt16() =>
            NextInt16(0, short.MaxValue);

        /// <summary>
        /// Returns a non-negative random <see cref="short"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is negative.</exception>
        public static short NextInt16(short maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue));

            return NextInt16(0, maxValue);
        }

        /// <summary>
        /// Returns a random <see cref="short"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static short NextInt16(short minValue, short maxValue)
        {
            if (minValue == maxValue) return maxValue;

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            return (short)(NextUInt32((uint)(maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Returns a random <see cref="ushort"/> less than <see cref="ushort.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="ushort.MaxValue"/>).</returns>
        public static ushort NextUInt16() =>
            NextUInt16(0, ushort.MaxValue);

        /// <summary>
        /// Returns a random <see cref="ushort"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        public static ushort NextUInt16(ushort maxValue) =>
            NextUInt16(0, maxValue);

        /// <summary>
        /// Returns a random <see cref="ushort"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static ushort NextUInt16(ushort minValue, ushort maxValue)
        {
            if (minValue == maxValue) return maxValue;

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            return (ushort)(NextUInt32((uint)(maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Returns a non-negative random <see cref="int"/> less than <see cref="int.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="int.MaxValue"/>).</returns>
        public static int NextInt32() =>
            NextInt32(0, int.MaxValue);

        /// <summary>
        /// Returns a non-negative random <see cref="int"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is negative.</exception>
        public static int NextInt32(int maxValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

            return NextInt32(0, maxValue);
        }

        /// <summary>
        /// Returns a random <see cref="int"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static int NextInt32(int minValue, int maxValue)
        {
            if (minValue == maxValue) return maxValue;

            ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

            return (int)NextUInt32((uint)(maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Returns a random <see cref="uint"/> using cryptographic randomness.
        /// </summary>
        /// <returns>A random 32-bit unsigned integer.</returns>
        public static uint NextUInt32() =>
            BinaryPrimitives.ReadUInt32LittleEndian(NextBytes(4, true));

        /// <summary>
        /// Returns a random <see cref="uint"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        public static uint NextUInt32(uint maxValue)
        {
            var randomProduct = (ulong)maxValue * NextUInt32();
            var lowPart = (uint)randomProduct;

            if (lowPart < maxValue)
            {
                var remainder = (0u - maxValue) % maxValue;

                while (lowPart < remainder)
                {
                    randomProduct = (ulong)maxValue * NextUInt32();
                    lowPart = (uint)randomProduct;
                }
            }

            return (uint)(randomProduct >> 32);
        }

        /// <summary>
        /// Returns a random <see cref="uint"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static uint NextUInt32(uint minValue, uint maxValue)
        {
            if (minValue == maxValue) return maxValue;

            ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

            return NextUInt32(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a non-negative random <see cref="long"/> less than <see cref="long.MaxValue"/>.
        /// </summary>
        /// <returns>A random value in the range [0, <see cref="long.MaxValue"/>).</returns>
        public static long NextInt64() =>
            NextInt64(0L, long.MaxValue);

        /// <summary>
        /// Returns a non-negative random <see cref="long"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        public static long NextInt64(long maxValue)
        {
            return NextInt64(0L, maxValue);
        }

        /// <summary>
        /// Returns a random <see cref="long"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static long NextInt64(long minValue, long maxValue)
        {
            if (minValue == maxValue) return maxValue;

            ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

            return (long)NextUInt64((ulong)(maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Returns a random <see cref="ulong"/> using cryptographic randomness.
        /// </summary>
        /// <returns>A random 64-bit unsigned integer.</returns>
        public static ulong NextUInt64() =>
            BinaryPrimitives.ReadUInt64LittleEndian(NextBytes(8, true));

        /// <summary>
        /// Returns a random <see cref="ulong"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        public static ulong NextUInt64(ulong maxValue)
        {
            var randomProduct = Math.BigMul(maxValue, NextUInt64(), out var lowPart);

            if (lowPart < maxValue)
            {
                var remainder = (0ul - maxValue) % maxValue;

                while (lowPart < remainder)
                {
                    randomProduct = Math.BigMul(maxValue, NextUInt64(), out lowPart);
                }
            }

            return randomProduct;
        }

        /// <summary>
        /// Returns a random <see cref="ulong"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            if (minValue == maxValue) return maxValue;

            ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

            return NextUInt64(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a random <see cref="BigInteger"/> within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random value in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static BigInteger NextBigInteger(BigInteger minValue, BigInteger maxValue)
        {
            if (minValue == maxValue) return maxValue;

            ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

            return NextBigInteger(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a non-negative random <see cref="BigInteger"/> that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random value in the range [0, <paramref name="maxValue"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is negative.</exception>
        public static BigInteger NextBigInteger(BigInteger maxValue)
        {
            if (maxValue.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue));

            if (maxValue == 0 || maxValue == 1)
                return BigInteger.Zero;

            var maxValueBits = maxValue.GetByteCount() * 8;
            var maxMaxValue = BigInteger.One << maxValueBits;

            var randomProduct = maxValue * NextBigInteger(maxValueBits);
            var lowPart = randomProduct % maxMaxValue;

            if (lowPart < maxValue)
            {
                var threshold = (maxMaxValue - maxValue) % maxValue;

                while (lowPart < threshold)
                {
                    randomProduct = maxValue * NextBigInteger(maxValueBits);
                    lowPart = randomProduct % maxMaxValue;
                }
            }

            return randomProduct >> maxValueBits;
        }

        /// <summary>
        /// Returns a non-negative random <see cref="BigInteger"/> with the specified bit length.
        /// </summary>
        /// <param name="sizeInBits">The number of bits in the generated value.</param>
        /// <returns>A random non-negative integer with at most <paramref name="sizeInBits"/> bits.</returns>
        /// <exception cref="ArgumentException"><paramref name="sizeInBits"/> is negative.</exception>
        public static BigInteger NextBigInteger(int sizeInBits)
        {
            if (sizeInBits < 0)
                throw new ArgumentException("sizeInBits must be non-negative.");

            if (sizeInBits == 0)
                return BigInteger.Zero;

            Span<byte> b = stackalloc byte[sizeInBits / 8 + 1];
            RandomNumberGenerator.Fill(b);

            if (sizeInBits % 8 == 0)
                b[^1] = 0;
            else
                b[^1] &= (byte)((1 << sizeInBits % 8) - 1);

            return new BigInteger(b);
        }

        /// <summary>
        /// Returns a random byte array of the specified length.
        /// </summary>
        /// <param name="length">The number of random bytes to generate.</param>
        /// <param name="cryptography">
        /// <see langword="true"/> to use <see cref="RandomNumberGenerator"/>; otherwise <see cref="Random.Shared"/>.
        /// </param>
        /// <returns>A new array filled with random bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        public static byte[] NextBytes(int length, bool cryptography = false)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 0, nameof(length));

            var bytes = new byte[length];

            if (cryptography)
                RandomNumberGenerator.Fill(bytes);
            else
                Random.Shared.NextBytes(bytes);

            return bytes;
        }
    }
}
