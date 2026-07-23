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

using Neo.VM.Core;
using System.Collections.Generic;

namespace Neo.VM.Extensions
{
    /// <summary>
    /// Extension methods for inspecting gas usage across an <see cref="ExecutionContext"/> chain.
    /// </summary>
    public static class ExecutionContextExtensions
    {
        /// <summary>
        /// Returns the total <see cref="ExecutionContext.GasConsumed"/> across this context and all its parents (call stack).
        /// </summary>
        /// <param name="context">The leaf context, or <see langword="null"/>.</param>
        /// <returns>The summed gas consumed along the parent chain.</returns>
        public static long GetTotalGasConsumed(this ExecutionContext? context)
        {
            var total = 0L;
            var current = context;

            while (current != null)
            {
                total += current.GasConsumed;
                current = current.Parent;
            }

            return total;
        }

        /// <summary>
        /// Returns a list of <see cref="ExecutionContext.GasConsumed"/> values from bottom (root) to top (current).
        /// Useful for debugging / logging the full call stack gas usage.
        /// </summary>
        /// <param name="context">The leaf context, or <see langword="null"/>.</param>
        /// <returns>Gas-consumed values ordered from root to leaf.</returns>
        public static List<long> GetGasConsumedChain(this ExecutionContext? context)
        {
            var chain = new List<long>();
            var current = context;

            while (current != null)
            {
                chain.Add(current.GasConsumed);
                current = current.Parent;
            }

            chain.Reverse(); // root first
            return chain;
        }
    }
}
