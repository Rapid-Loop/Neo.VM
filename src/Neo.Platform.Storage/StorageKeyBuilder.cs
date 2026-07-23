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
using Neo.Core.VM;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Neo.Platform.Storage
{
    /// <summary>
    /// Builds binary storage keys by appending typed values into a rented buffer.
    /// </summary>
    /// <remarks>
    /// Keys begin with a little-endian contract <c>id</c> and an optional prefix byte
    /// (<see cref="PrefixLength"/>). Dispose the builder when finished to return pooled memory.
    /// </remarks>
    public class StorageKeyBuilder : IDisposable, IComparable, IComparable<StorageKeyBuilder>, IEquatable<StorageKeyBuilder>, IEnumerable<byte>
    {
        /// <summary>
        /// The fixed prefix length: 4-byte little-endian id plus 1 prefix byte.
        /// </summary>
        public const int PrefixLength = sizeof(int) + sizeof(byte);

        /// <summary>
        /// Gets the number of bytes currently written to the key.
        /// </summary>
        public int Length => _byteCount;

        /// <summary>
        /// Gets the maximum capacity of the key buffer, including the fixed prefix.
        /// </summary>
        public int MaxLength => _maxLength;

        private readonly IMemoryOwner<byte> _memoryOwner;
        private readonly int _maxLength;
        private int _byteCount;

        private StorageKeyBuilder(int id, byte prefix = 0, int maxLength = -1)
        {
            _maxLength = maxLength <= 0 ?
                ExecutionEngineLimits.Default.MaxStorageKeySize :
                maxLength;
            _maxLength += PrefixLength;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_maxLength);

            BinaryPrimitives.WriteInt32LittleEndian(_memoryOwner.Memory.Span, id);

            _byteCount = sizeof(int);
            _memoryOwner.Memory.Span[_byteCount++] = prefix;
        }

        private StorageKeyBuilder(ReadOnlySpan<byte> data, int maxLength = -1)
        {
            _byteCount = data.Length;
            _maxLength = maxLength <= 0 ?
                _byteCount + ExecutionEngineLimits.Default.MaxStorageKeySize :
                maxLength;
            _maxLength += PrefixLength;
            _memoryOwner = MemoryPool<byte>.Shared.Rent(_maxLength);
            data.CopyTo(_memoryOwner.Memory.Span);
        }

        /// <summary>
        /// Clears and returns the rented key buffer to the shared pool.
        /// </summary>
        public void Dispose()
        {
            _memoryOwner.Memory.Span.Clear();
            _memoryOwner.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines whether the specified object is a <see cref="StorageKeyBuilder"/> with the same key bytes.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if the objects represent the same key; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as StorageKeyBuilder);
        }

        /// <summary>
        /// Determines whether another builder has the same key bytes.
        /// </summary>
        /// <param name="other">The other builder.</param>
        /// <returns><see langword="true"/> if the keys are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] StorageKeyBuilder? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (_byteCount != other._byteCount) return false;
            return _memoryOwner.Memory[.._byteCount]
                .Span
                .SequenceEqual(other._memoryOwner.Memory[..other._byteCount].Span);
        }

        /// <summary>
        /// Compares this key to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// A value less than zero if this key is less than <paramref name="obj"/>;
        /// zero if equal; greater than zero if greater.
        /// </returns>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            if (obj is null) return 1;
            return CompareTo(obj as StorageKeyBuilder);
        }

        /// <summary>
        /// Compares this key to another builder by length, then by byte sequence.
        /// </summary>
        /// <param name="other">The other builder.</param>
        /// <returns>
        /// A value less than zero if this key is less than <paramref name="other"/>;
        /// zero if equal; greater than zero if greater.
        /// </returns>
        public int CompareTo(StorageKeyBuilder? other)
        {
            if (ReferenceEquals(other, this)) return 0;
            if (other is null) return 1;

            if (_byteCount != other._byteCount)
                return _byteCount.CompareTo(other._byteCount);

            return _memoryOwner.Memory[.._byteCount]
                .Span
                .SequenceCompareTo(other._memoryOwner.Memory[..other._byteCount].Span);
        }

        /// <summary>
        /// Returns a hash code for the current key bytes.
        /// </summary>
        /// <returns>A hash code for the key content.</returns>
        public override int GetHashCode() =>
            _memoryOwner.Memory[.._byteCount]
                .ToHashCode();

        /// <summary>
        /// Returns a lowercase hexadecimal representation of the key bytes.
        /// </summary>
        /// <returns>The key as a hex string.</returns>
        public override string ToString() =>
            Convert.ToHexStringLower(_memoryOwner.Memory[.._byteCount].Span);

        /// <summary>
        /// Concatenates the key bytes of two builders into a new builder.
        /// </summary>
        /// <param name="a">The left-hand key.</param>
        /// <param name="b">The right-hand key.</param>
        /// <returns>A new builder containing the concatenated bytes.</returns>
        public static StorageKeyBuilder operator +(StorageKeyBuilder a, StorageKeyBuilder b) =>
            new
            (
                [
                    .. a._memoryOwner.Memory[..a._byteCount].Span,
                    .. b._memoryOwner.Memory[..b._byteCount].Span
                ],
                a._maxLength + b._maxLength
            );

        /// <summary>
        /// Determines whether two builders have equal key bytes.
        /// </summary>
        /// <param name="left">The left-hand builder.</param>
        /// <param name="right">The right-hand builder.</param>
        /// <returns><see langword="true"/> if the keys are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(StorageKeyBuilder left, StorageKeyBuilder right) =>
            left.Equals(right);

        /// <summary>
        /// Determines whether two builders have different key bytes.
        /// </summary>
        /// <param name="left">The left-hand builder.</param>
        /// <param name="right">The right-hand builder.</param>
        /// <returns><see langword="true"/> if the keys differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(StorageKeyBuilder left, StorageKeyBuilder right) =>
            !(left == right);

        /// <summary>
        /// Creates a new key builder for the specified contract id and prefix.
        /// </summary>
        /// <param name="id">The contract id written as a little-endian <see cref="int"/>.</param>
        /// <param name="prefix">An optional prefix byte written after the id.</param>
        /// <returns>A new <see cref="StorageKeyBuilder"/> instance.</returns>
        public static StorageKeyBuilder Create(int id, byte prefix = 0) =>
            new(id, prefix);

        /// <summary>
        /// Appends a single byte to the key.
        /// </summary>
        /// <param name="data">The byte to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(byte data)
        {
            _memoryOwner.Memory.Span[_byteCount++] = data;
            return this;
        }

        /// <summary>
        /// Appends a 16-bit signed integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(short data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt16BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt16LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(short);
            return this;
        }

        /// <summary>
        /// Appends a 16-bit unsigned integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ushort data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt16BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ushort);
            return this;
        }

        /// <summary>
        /// Appends a 32-bit signed integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(int data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt32BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt32LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(int);
            return this;
        }

        /// <summary>
        /// Appends a 32-bit unsigned integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(uint data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt32BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(uint);
            return this;
        }

        /// <summary>
        /// Appends a 64-bit signed integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(long data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt64BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt64LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(long);
            return this;
        }

        /// <summary>
        /// Appends a 64-bit unsigned integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ulong data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt64BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ulong);
            return this;
        }

        /// <summary>
        /// Appends a 128-bit signed integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(Int128 data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt128BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt128LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(long) * 2;
            return this;
        }

        /// <summary>
        /// Appends a 128-bit unsigned integer to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(UInt128 data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt128BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt128LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ulong) * 2;
            return this;
        }

        /// <summary>
        /// Appends a double-precision floating-point value to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(double data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteDoubleBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteDoubleLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(double);
            return this;
        }

        /// <summary>
        /// Appends a single-precision floating-point value to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(float data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteSingleBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteSingleLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(float);
            return this;
        }

        /// <summary>
        /// Appends a half-precision floating-point value to the key.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(Half data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteHalfBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteHalfLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(short);
            return this;
        }

        /// <summary>
        /// Appends a <see cref="BigInteger"/> as its unsigned byte representation.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">
        /// When <see langword="true"/>, writes big-endian; otherwise little-endian.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(BigInteger data, bool isBigEndian = false) =>
            Append(data.ToByteArray(true, isBigEndian));

        /// <summary>
        /// Appends raw bytes to the key.
        /// </summary>
        /// <param name="data">The bytes to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlySpan<byte> data)
        {
            data.CopyTo(_memoryOwner.Memory.Span[_byteCount..]);
            _byteCount += data.Length;
            return this;
        }

        /// <summary>
        /// Appends a <see cref="decimal"/> as its four 32-bit integer parts (little-endian).
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">Reserved for API consistency; currently unused.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(decimal data, bool isBigEndian = false)
        {
            Span<int> span = stackalloc int[4];

            _ = decimal.GetBits(data, span);

            foreach (var bit in span)
                _ = Append(bit);

            return this;
        }

        /// <summary>
        /// Appends a <see cref="Guid"/> as its 16-byte representation.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <param name="isBigEndian">Controls the byte order of the GUID encoding.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(Guid data, bool isBigEndian = false) =>
            Append(data.ToByteArray(isBigEndian));

        /// <summary>
        /// Appends a sequence of bytes to the key.
        /// </summary>
        /// <param name="data">The sequence to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlySequence<byte> data) =>
            Append(data.ToArray());

        /// <summary>
        /// Appends an array segment of bytes to the key.
        /// </summary>
        /// <param name="data">The segment to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ArraySegment<byte> data) =>
            Append(data.AsSpan());

        /// <summary>
        /// Appends a <see cref="DateTime"/> as Unix milliseconds (UTC adjusted by <paramref name="offset"/>).
        /// </summary>
        /// <param name="stamp">The timestamp to append.</param>
        /// <param name="offset">Optional offset applied when converting to Unix time.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(DateTime stamp, TimeSpan offset = default) =>
            Append((ulong)CoreUtilities.ToUnixTimeMilliseconds(stamp, offset));

        /// <summary>
        /// Appends a <see cref="TimeSpan"/> as Unix milliseconds (adjusted by <paramref name="offset"/>).
        /// </summary>
        /// <param name="span">The time span to append.</param>
        /// <param name="offset">Optional offset applied when converting to Unix time.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(TimeSpan span, TimeSpan offset = default) =>
            Append((ulong)CoreUtilities.ToUnixTimeMilliseconds(span, offset));

        /// <summary>
        /// Appends a <see cref="DateTimeOffset"/> as Unix milliseconds.
        /// </summary>
        /// <param name="stamp">The timestamp to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(DateTimeOffset stamp) =>
            Append((ulong)stamp.ToUnixTimeMilliseconds());

        /// <summary>
        /// Appends a <see cref="TimeOnly"/> value as its time-of-day span encoding.
        /// </summary>
        /// <param name="time">The time to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(TimeOnly time) =>
            Append(time.ToTimeSpan());

        /// <summary>
        /// Appends a <see cref="DateOnly"/> combined with an optional <see cref="TimeOnly"/> as Unix milliseconds.
        /// </summary>
        /// <param name="date">The date to append.</param>
        /// <param name="time">The time of day to combine with the date. Defaults to midnight.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(DateOnly date, TimeOnly time = default) =>
            Append(date.ToDateTime(time));

        /// <summary>
        /// Appends a sequence of characters encoded as UTF-8.
        /// </summary>
        /// <param name="data">The characters to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlySequence<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        /// <summary>
        /// Appends characters encoded as UTF-8.
        /// </summary>
        /// <param name="data">The characters to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlyMemory<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        /// <summary>
        /// Appends characters encoded as UTF-8.
        /// </summary>
        /// <param name="data">The characters to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlySpan<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        /// <summary>
        /// Appends a character array encoded as UTF-8.
        /// </summary>
        /// <param name="data">The characters to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(char[] data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data)) :
                Append(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// Appends a single character encoded as UTF-8.
        /// </summary>
        /// <param name="data">The character to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(char data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data.ToString())) :
                Append(Encoding.UTF8.GetBytes(data.ToString()));

        /// <summary>
        /// Appends a signed byte as an unsigned byte.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(sbyte data) =>
            Append((byte)data);

        /// <summary>
        /// Appends a Boolean as a single byte (<c>1</c> for <see langword="true"/>, <c>0</c> for <see langword="false"/>).
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(bool data) =>
            Append(data ? (byte)1 : (byte)0);

        /// <summary>
        /// Appends a byte array to the key.
        /// </summary>
        /// <param name="data">The bytes to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(byte[] data) =>
            Append(data.AsSpan());

        /// <summary>
        /// Appends a memory region of bytes to the key.
        /// </summary>
        /// <param name="data">The bytes to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(ReadOnlyMemory<byte> data) =>
            Append(data.Span);

        /// <summary>
        /// Appends the key bytes of another builder.
        /// </summary>
        /// <param name="other">The builder whose current bytes are appended.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(StorageKeyBuilder other) =>
            Append(other._memoryOwner.Memory[..other._byteCount].Span);

        /// <summary>
        /// Appends a <see cref="UInt160"/> as its 20-byte representation.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(UInt160 data) =>
            Append(data.ToArray());

        /// <summary>
        /// Appends a <see cref="UInt256"/> as its 32-byte representation.
        /// </summary>
        /// <param name="data">The value to append.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(UInt256 data) =>
            Append(data.ToArray());

        /// <summary>
        /// Appends a string encoded as UTF-8.
        /// </summary>
        /// <param name="data">The string to encode and append.</param>
        /// <param name="isStrictUtf8">
        /// When <see langword="true"/>, uses strict UTF-8 encoding; otherwise standard UTF-8.
        /// </param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(string data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data)) :
                Append(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// Appends a prefix byte followed by raw bytes.
        /// </summary>
        /// <param name="prefix">The prefix byte.</param>
        /// <param name="data">The bytes to append after the prefix.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(byte prefix, ReadOnlySpan<byte> data) =>
            Append(prefix).Append(data);

        /// <summary>
        /// Appends a prefix byte followed by a <see cref="UInt160"/>.
        /// </summary>
        /// <param name="prefix">The prefix byte.</param>
        /// <param name="data">The value to append after the prefix.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(byte prefix, UInt160 data) =>
            Append(prefix).Append(data);

        /// <summary>
        /// Appends a prefix byte followed by a <see cref="UInt256"/>.
        /// </summary>
        /// <param name="prefix">The prefix byte.</param>
        /// <param name="data">The value to append after the prefix.</param>
        /// <returns>This builder for chaining.</returns>
        public StorageKeyBuilder Append(byte prefix, UInt256 data) =>
            Append(prefix).Append(data);

        /// <summary>
        /// Copies the current key bytes into a new array.
        /// </summary>
        /// <returns>A new array containing the key bytes written so far.</returns>
        public byte[] ToArray() =>
            _memoryOwner.Memory[.._byteCount].ToArray();

        /// <summary>
        /// Creates a new builder with a copy of the current key bytes.
        /// </summary>
        /// <returns>A cloned <see cref="StorageKeyBuilder"/>.</returns>
        public StorageKeyBuilder Clone() =>
            new(_memoryOwner.Memory[.._byteCount].Span, _maxLength);

        /// <summary>
        /// Returns an enumerator that iterates over the key bytes.
        /// </summary>
        /// <returns>An enumerator for the key content.</returns>
        public IEnumerator<byte> GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ToArray().GetEnumerator();
    }
}
