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

using Neo.Configuration.Json;
using Neo.Core.Interfaces;
using Neo.Wallet.Cryptography;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// JSON model for SCrypt key-derivation parameters.
    /// </summary>
    public class SCryptModel : JsonModel, IMap<ScryptParameters>
    {
        /// <summary>
        /// The default SCrypt parameters (<c>N=16384</c>, <c>R=8</c>, <c>P=8</c>).
        /// </summary>
        public static readonly SCryptModel Default = new()
        {
            N = 16384,
            R = 8,
            P = 8,
        };

        /// <summary>
        /// CPU/Memory cost parameter. Must be larger than 1, a power of 2 and less than 2^(128 * r / 8).
        /// </summary>
        public int N { get; set; }

        /// <summary>
        /// The block size, must be >= 1.
        /// </summary>
        public int R { get; set; }

        /// <summary>
        /// Parallelization parameter. Must be a positive integer less than or equal to Int32.MaxValue / (128 * r * 8).
        /// </summary>
        public int P { get; set; }

        /// <summary>
        /// Converts this model to a <see cref="ScryptParameters"/> instance.
        /// </summary>
        /// <returns>A <see cref="ScryptParameters"/> with the same N, R, and P values.</returns>
        public ScryptParameters ToObject() =>
            new(N, R, P);
    }
}
