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

using Neo.Core;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.VM.Extensions
{
    /// <summary>
    /// Extension methods for converting CLR values to <see cref="VMObject"/> stack items.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts a CLR value into the corresponding <see cref="VMObject"/> representation.
        /// Supported primitives become integer, boolean, or byte-string items; sequences become arrays;
        /// other reference types become <see cref="VMInteropInterface"/> wrappers.
        /// </summary>
        /// <typeparam name="TSource">The source value type.</typeparam>
        /// <param name="source">The value to convert; <see langword="null"/> becomes <see cref="VMNull.Instance"/>.</param>
        /// <returns>A stack item suitable for evaluation-stack operations.</returns>
        public static VMObject ToStackItem<TSource>(this TSource? source) =>
            source switch
            {
                null => VMNull.Instance,
                VMObject vm => vm,
                bool b => new VMBoolean(b),
                byte[] ba => new VMByteArray(ba),
                byte b => new VMInteger(b),
                sbyte b => new VMInteger(b),
                short s => new VMInteger(s),
                ushort s => new VMInteger(s),
                int i => new VMInteger(i),
                uint i => new VMInteger(i),
                long l => new VMInteger(l),
                ulong l => new VMInteger(l),
                BigInteger bi => new VMInteger(bi),
                string s => new VMByteArray(CoreUtilities.StrictUtf8Encoding.GetBytes(s)),
                Memory<byte> m => new VMByteArray(m),
                ReadOnlyMemory<byte> rm => new VMByteArray(rm.ToArray()),
                Memory<TSource> m => new VMArray(m.ToArray().Select(static s => s.ToStackItem())),
                ReadOnlyMemory<TSource> rm => new VMArray(rm.ToArray().Select(static s => s.ToStackItem()), true),
                TSource[] a => new VMArray(a.Select(static s => s.ToStackItem())),
                IEnumerable<TSource> e => new VMArray(e.Select(static s => s.ToStackItem())),
                _ => new VMInteropInterface(source, source.GetType().Name)
            };
    }
}
