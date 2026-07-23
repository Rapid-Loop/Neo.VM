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

using Neo.Core.Interfaces;
using Neo.Wallet.Json;

namespace Neo.Wallet.Cryptography
{
    /// <summary>
    /// Represents the parameters of the SCrypt algorithm.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ScryptParameters"/> class.
    /// </remarks>
    /// <param name="n">CPU/Memory cost parameter.</param>
    /// <param name="r">The block size.</param>
    /// <param name="p">Parallelization parameter.</param>
    public class ScryptParameters(int n, int r, int p) : IMap<SCryptModel>
    {
        /// <summary>
        /// The default parameters
        /// </summary>
        public static ScryptParameters Default { get; } = new ScryptParameters(16384, 8, 8);

        /// <summary>
        /// CPU/Memory cost parameter. Must be larger than 1, a power of 2 and less than 2^(128 * r / 8).
        /// </summary>
        public int N => n;

        /// <summary>
        /// The block size, must be >= 1.
        /// </summary>
        public int R => r;

        /// <summary>
        /// Parallelization parameter. Must be a positive integer less than or equal to Int32.MaxValue / (128 * r * 8).
        /// </summary>
        public int P => p;

        /// <summary>
        /// Converts these parameters to their JSON model representation.
        /// </summary>
        /// <returns>A <see cref="SCryptModel"/> with the same N, R, and P values.</returns>
        public SCryptModel ToObject() =>
            new()
            {
                N = N,
                R = R,
                P = P,
            };
    }
}
