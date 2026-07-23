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

using Neo.Core.Extensions;
using Neo.Core.VM.Type;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents a code pointer stack item: a script image plus an instruction position.
    /// </summary>
    public class VMPointer : VMObject, IEquatable<VMPointer>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Pointer;

        /// <summary>
        /// Gets the script bytes associated with this pointer.
        /// </summary>
        public ReadOnlyMemory<byte> Script => _memoryOwner.Memory[.._byteCount];

        /// <summary>
        /// Gets the length of the script in bytes.
        /// </summary>
        public int Length => _byteCount;

        /// <summary>
        /// Gets the instruction position within the script.
        /// </summary>
        public int Position => _ip;

        private readonly IMemoryOwner<byte> _memoryOwner;
        private readonly int _ip = 0;
        private readonly int _byteCount;

        /// <summary>
        /// Initializes a new pointer from a script array and instruction position.
        /// </summary>
        /// <param name="script">The script bytecode.</param>
        /// <param name="ip">The instruction pointer offset.</param>
        public VMPointer(byte[] script, int ip)
        {
            _ip = ip;
            _byteCount = script.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            script.AsMemory().TryCopyTo(_memoryOwner.Memory);
        }

        /// <summary>
        /// Initializes a new pointer from script memory and instruction position.
        /// </summary>
        /// <param name="script">The script bytecode.</param>
        /// <param name="ip">The instruction pointer offset.</param>
        public VMPointer(Memory<byte> script, int ip)
        {
            _ip = ip;
            _byteCount = script.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            script.TryCopyTo(_memoryOwner.Memory);
        }

        /// <summary>
        /// Initializes a new pointer from script memory and instruction position.
        /// </summary>
        /// <param name="script">The script bytecode.</param>
        /// <param name="ip">The instruction pointer offset.</param>
        public VMPointer(ReadOnlyMemory<byte> script, int ip)
        {
            _ip = ip;
            _byteCount = script.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            script.TryCopyTo(_memoryOwner.Memory);
        }

        /// <summary>
        /// Initializes a new pointer from a <see cref="VMByteArray"/> script and instruction position.
        /// </summary>
        /// <param name="script">The script as a byte-string stack item.</param>
        /// <param name="ip">The instruction pointer offset.</param>
        public VMPointer(VMByteArray script, int ip)
        {
            _ip = ip;
            _byteCount = script.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            script.AsSpan().TryCopyTo(_memoryOwner.Memory.Span);
        }

        /// <summary>
        /// Determines whether this pointer is equal to another pointer.
        /// </summary>
        /// <param name="other">The other pointer.</param>
        /// <returns><see langword="true"/> if the script and position match; otherwise <see langword="false"/>.</returns>
        public bool Equals(VMPointer? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            if (Length != other.Length) return false;
            return _ip == other._ip &&
                _memoryOwner.Memory[.._byteCount]
                .Span
                .SequenceEqual(other._memoryOwner.Memory[..other._byteCount].Span);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMPointer);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (31 * _ip) ^
                _memoryOwner.Memory[.._byteCount]
                .ToHashCode(RefCount ^ 397);
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var clone = new VMPointer(_memoryOwner.Memory[.._byteCount].ToArray(), _ip);

            return clone;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return true;
        }

        /// <inheritdoc />
        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            throw new InvalidOperationException($"Cannot convert {Type} to integer");
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            return _memoryOwner.Memory[.._byteCount].Span;
        }
    }
}
