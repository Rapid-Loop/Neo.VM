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
using System.Numerics;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="BigInteger"/>.
    /// </summary>
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Computes the integer square root of a non-negative value (floor of the true square root).
        /// </summary>
        /// <param name="value">The non-negative value whose square root is computed.</param>
        /// <returns>The largest integer whose square is less than or equal to <paramref name="value"/>.</returns>
        /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is negative.</exception>
        public static BigInteger Sqrt(this BigInteger value)
        {
            if (value < 0)
                throw new OverflowException("Cannot compute the square root of a negative number.");

            if (value.IsZero || value.IsOne)
                return value;

            // Establish an initial estimate using the bit length
            // Math.Ceiling(BigInteger.Log(value, 2)) approximation via bit length
            var bitLength = value.ToByteArray().Length * 8;
            var currentEstimate = BigInteger.One << (bitLength / 2);
            BigInteger previousEstimate;

            do
            {
                previousEstimate = currentEstimate;
                // Newton-Raphson iteration step: x = (x + n / x) / 2
                currentEstimate = (currentEstimate + (value / currentEstimate)) >> 1;
            }
            while (BigInteger.Abs(currentEstimate - previousEstimate) > 1);

            // Standard correction step to ensure floor rounding rule
            while (currentEstimate * currentEstimate > value)
                currentEstimate--;

            return currentEstimate;
        }

        /// <summary>
        /// Calculates the modular multiplicative inverse of <paramref name="a"/> modulo <paramref name="m"/>.
        /// </summary>
        /// <param name="a">The value for which the modular inverse is computed.</param>
        /// <param name="m">The positive modulus.</param>
        /// <returns>The modular multiplicative inverse of <paramref name="a"/> modulo <paramref name="m"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="m"/> is not positive.</exception>
        /// <exception cref="ArithmeticException">Thrown when the inverse does not exist because the values are not coprime.</exception>
        public static BigInteger ModInverse(this BigInteger a, BigInteger m)
        {
            if (m <= 0)
                throw new ArgumentOutOfRangeException(nameof(m), "Modulus must be positive.");

            BigInteger t = 0, newT = 1;
            BigInteger r = m, newR = (a % m + m) % m; // Handles negative 'a' safely

            while (newR > 0)
            {
                var quotient = r / newR;

                var tempT = t;
                t = newT;
                newT = tempT - quotient * newT;

                var tempR = r;
                r = newR;
                newR = tempR - quotient * newR;
            }

            if (r > 1)
                throw new ArithmeticException($"{a} and {m} are not coprime. Modular inverse does not exist.");

            if (t < 0)
                t += m;

            return t;
        }
    }
}
