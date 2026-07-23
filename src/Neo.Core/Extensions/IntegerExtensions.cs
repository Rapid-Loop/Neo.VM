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
    /// Provides extension methods for integer types.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Gets the number of bytes required to encode the value as a Bitcoin-style compact size integer.
        /// </summary>
        /// <typeparam name="T">The integer type.</typeparam>
        /// <param name="value">The value to measure.</param>
        /// <returns>The encoded size in bytes (1, 3, 5, or 9).</returns>
        public static byte GetCompactSize<T>(this T value)
            where T : unmanaged, IBinaryInteger<T> =>
            Convert.ToUInt64(value) switch
            {
                < 0xfd => sizeof(byte),
                <= ushort.MaxValue => sizeof(byte) + sizeof(ushort),
                <= uint.MaxValue => sizeof(byte) + sizeof(uint),
                _ => sizeof(byte) + sizeof(ulong),
            };
    }
}
