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
    /// Represents a mutable byte buffer stack item backed by pooled memory.
    /// </summary>
    public class VMBuffer : VMObject, IEquatable<VMBuffer>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Buffer;

        /// <summary>
        /// Gets the length of the buffer in bytes.
        /// </summary>
        public int Length => _byteCount;

        private readonly IMemoryOwner<byte> _memoryOwner;
        private readonly int _byteCount;

        /// <summary>
        /// Initializes a zero-filled buffer of the specified size.
        /// </summary>
        /// <param name="size">The buffer size in bytes.</param>
        public VMBuffer(int size)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(size);
            _byteCount = size;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
        }

        /// <summary>
        /// Initializes a buffer by copying the specified data.
        /// </summary>
        /// <param name="data">The source bytes.</param>
        public VMBuffer(ReadOnlySpan<byte> data)
        {
            _byteCount = data.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            data.TryCopyTo(_memoryOwner.Memory.Span);
        }

        /// <summary>
        /// Initializes a buffer by copying the specified data.
        /// </summary>
        /// <param name="data">The source bytes.</param>
        public VMBuffer(Span<byte> data)
        {
            _byteCount = data.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            data.TryCopyTo(_memoryOwner.Memory.Span);
        }

        /// <summary>
        /// Initializes a buffer by copying the specified data.
        /// </summary>
        /// <param name="data">The source bytes.</param>
        public VMBuffer(byte[] data)
        {
            _byteCount = data.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            data.AsMemory().TryCopyTo(_memoryOwner.Memory);
        }

        /// <summary>
        /// Initializes a buffer by copying the contents of a <see cref="VMByteArray"/>.
        /// </summary>
        /// <param name="source">The source byte array stack item.</param>
        public VMBuffer(VMByteArray source)
        {
            _byteCount = source.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            source.AsSpan().TryCopyTo(_memoryOwner.Memory.Span);
        }

        /// <summary>
        /// Initializes a buffer by copying the specified memory.
        /// </summary>
        /// <param name="source">The source memory.</param>
        public VMBuffer(Memory<byte> source)
        {
            _byteCount = source.Length;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_byteCount);
            source.TryCopyTo(_memoryOwner.Memory);
        }

        /// <summary>
        /// Determines whether this buffer is equal to another buffer.
        /// </summary>
        /// <param name="other">The other buffer.</param>
        /// <returns><see langword="true"/> if the contents are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] VMBuffer? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            if (Length != other.Length) return false;
            return _memoryOwner.Memory[.._byteCount]
                .Span
                .SequenceEqual(other._memoryOwner.Memory[..other._byteCount].Span);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMBuffer);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _memoryOwner.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _memoryOwner.Memory[.._byteCount]
                .ToHashCode(RefCount ^ 397);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            foreach (var v in _memoryOwner.Memory[.._byteCount].Span)
            {
                if (char.IsAscii((char)v)) continue;
                return Convert.ToBase64String(_memoryOwner.Memory[.._byteCount].Span);
            }

            return CoreUtilities.StrictUtf8Encoding.GetString(_memoryOwner.Memory[.._byteCount].Span);
        }

        /// <summary>
        /// Copies a range of bytes from this buffer into another buffer.
        /// </summary>
        /// <param name="dstBuffer">The destination buffer.</param>
        /// <param name="startIndex">The starting index in this buffer.</param>
        /// <param name="dstIndex">The starting index in the destination buffer.</param>
        /// <param name="count">The number of bytes to copy.</param>
        public void CopyTo(VMBuffer dstBuffer, int startIndex, int dstIndex, int count)
        {
            _memoryOwner.Memory.Slice(startIndex, count)
                .CopyTo(dstBuffer._memoryOwner.Memory[dstIndex..]);
        }

        /// <summary>
        /// Reverses the order of bytes in this buffer.
        /// </summary>
        public void Reverse()
        {
            _memoryOwner.Memory[.._byteCount].Span.Reverse();
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var clone = new VMBuffer(_memoryOwner.Memory[.._byteCount].ToArray());

            return clone;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return !_memoryOwner.Memory[.._byteCount].IsEmpty;
        }

        /// <inheritdoc />
        public override BigInteger GetInteger()
        {
            var span = _memoryOwner.Memory[.._byteCount].Span;

            if (span.Length > VMInteger.MaxSize)
                throw new InvalidCastException();
            //return new(span[..VMInteger.MaxSize]);

            return new(span);
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            return _memoryOwner.Memory[.._byteCount].Span;
        }

        /// <summary>
        /// Gets or sets the byte at the specified index.
        /// </summary>
        /// <param name="index">The zero-based byte index.</param>
        /// <returns>The byte at the specified index.</returns>
        public byte this[int index]
        {
            get => _memoryOwner.Memory[.._byteCount].Span[index];
            set => _memoryOwner.Memory[.._byteCount].Span[index] = value;
        }

        /// <summary>
        /// Returns the buffer contents as a lowercase hexadecimal string.
        /// </summary>
        /// <returns>The hex representation of the buffer.</returns>
        public string ToHexString()
        {
            return Convert.ToHexStringLower(_memoryOwner.Memory[.._byteCount].Span);
        }

        /// <summary>
        /// Concatenation: VMBuffer + VMBuffer
        /// </summary>
        public static VMBuffer operator +(VMBuffer a, VMBuffer b)
        {
            return new(
                [
                    .. a._memoryOwner.Memory[..a._byteCount].Span,
                    .. b._memoryOwner.Memory[..b._byteCount].Span,
                ]
            );
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public static bool operator ==(VMBuffer a, VMBuffer b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Inequality comparison.
        /// </summary>
        public static bool operator !=(VMBuffer a, VMBuffer b) =>
            !(a == b);

        /// <summary>
        /// Converts a buffer to a <see cref="BigInteger"/> using up to <see cref="VMInteger.MaxSize"/> bytes.
        /// </summary>
        public static implicit operator BigInteger(VMBuffer value)
        {
            return new BigInteger(value._memoryOwner.Memory[..value._byteCount]
                .Span[..VMInteger.MaxSize]);
        }
    }
}
