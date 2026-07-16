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

namespace Neo.Core.VM
{
    /// <summary>
    /// Represents the restrictions on the VM.
    /// </summary>
    public sealed class ExecutionEngineLimits
    {
        public const long MaxGas = 20_00000000;

        /// <summary>
        /// The default strategy.
        /// </summary>
        public static ExecutionEngineLimits Default => new();

        /// <summary>
        /// The maximum size of storage keys.
        /// </summary>
        public int MaxStorageKeySize { get; init; } = 64;

        /// <summary>
        /// The maximum size of storage values.
        /// </summary>
        public int MaxStorageValueSize { get; init; } = ushort.MaxValue;

        /// <summary>
        /// The maximum number of bits that <see cref="OpCode.SHL"/> and <see cref="OpCode.SHR"/> can shift.
        /// </summary>
        public int MaxShift { get; init; } = 256;

        /// <summary>
        /// The maximum number of items that can be contained in the VM's evaluation stacks and slots.
        /// </summary>
        public uint MaxStackSize { get; init; } = 2 * 1024;

        /// <summary>
        /// The maximum size of an item in the VM.
        /// </summary>
        public uint MaxItemSize { get; init; } = ushort.MaxValue * 2;

        /// <summary>
        /// The largest comparable size.
        /// </summary>
        public uint MaxComparableSize { get; init; } = 65536;

        /// <summary>
        /// The maximum number of frames in the invocation stack of the VM.
        /// </summary>
        public uint MaxInvocationStackSize { get; init; } = 1024;

        /// <summary>
        /// The maximum nesting depth of <see langword="try"/>-<see langword="catch"/>-<see langword="finally"/> blocks.
        /// </summary>
        public uint MaxTryNestingDepth { get; init; } = 16;

        /// <summary>
        /// Allow to catch the ExecutionEngine Exceptions
        /// </summary>
        public bool CatchEngineExceptions { get; init; } = true;

        /// <summary>
        /// Assert that the size of the item meets the limit.
        /// </summary>
        /// <param name="size">The size to be checked.</param>
        public void AssertMaxItemSize(int size)
        {
            if (size < 0 || size > MaxItemSize)
                throw new InvalidOperationException($"MaxItemSize exceed: {size}/{MaxItemSize}");
        }

        /// <summary>
        /// Assert that the number of bits shifted meets the limit.
        /// </summary>
        /// <param name="shift">The number of bits shifted.</param>
        public void AssertShift(int shift)
        {
            if (shift > MaxShift || shift < 0)
                throw new InvalidOperationException($"Invalid shift value: {shift}/{MaxShift}");
        }
    }
}
