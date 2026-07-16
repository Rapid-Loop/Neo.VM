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
    public class StorageKeyBuilder : IDisposable, IComparable, IComparable<StorageKeyBuilder>, IEquatable<StorageKeyBuilder>, IEnumerable<byte>
    {
        public const int PrefixLength = sizeof(int) + sizeof(byte);

        public int Length => _byteCount;

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

        public void Dispose()
        {
            _memoryOwner.Memory.Span.Clear();
            _memoryOwner.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as StorageKeyBuilder);
        }

        public bool Equals([NotNullWhen(true)] StorageKeyBuilder? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (_byteCount != other._byteCount) return false;
            return _memoryOwner.Memory[.._byteCount]
                .Span
                .SequenceEqual(other._memoryOwner.Memory[..other._byteCount].Span);
        }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            if (obj is null) return 1;
            return CompareTo(obj as StorageKeyBuilder);
        }

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

        public override int GetHashCode() =>
            _memoryOwner.Memory[.._byteCount]
                .ToHashCode();

        public override string ToString() =>
            Convert.ToHexStringLower(_memoryOwner.Memory[.._byteCount].Span);

        public static StorageKeyBuilder operator +(StorageKeyBuilder a, StorageKeyBuilder b) =>
            new
            (
                [
                    .. a._memoryOwner.Memory[..a._byteCount].Span,
                    .. b._memoryOwner.Memory[..b._byteCount].Span
                ],
                a._maxLength + b._maxLength
            );

        public static bool operator ==(StorageKeyBuilder left, StorageKeyBuilder right) =>
            left.Equals(right);

        public static bool operator !=(StorageKeyBuilder left, StorageKeyBuilder right) =>
            !(left == right);

        public static StorageKeyBuilder Create(int id, byte prefix = 0) =>
            new(id, prefix);

        public StorageKeyBuilder Append(byte data)
        {
            _memoryOwner.Memory.Span[_byteCount++] = data;
            return this;
        }

        public StorageKeyBuilder Append(short data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt16BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt16LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(short);
            return this;
        }

        public StorageKeyBuilder Append(ushort data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt16BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ushort);
            return this;
        }

        public StorageKeyBuilder Append(int data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt32BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt32LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(int);
            return this;
        }

        public StorageKeyBuilder Append(uint data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt32BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(uint);
            return this;
        }

        public StorageKeyBuilder Append(long data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt64BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt64LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(long);
            return this;
        }

        public StorageKeyBuilder Append(ulong data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt64BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ulong);
            return this;
        }

        public StorageKeyBuilder Append(Int128 data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteInt128BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteInt128LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(long) * 2;
            return this;
        }

        public StorageKeyBuilder Append(UInt128 data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteUInt128BigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteUInt128LittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(ulong) * 2;
            return this;
        }

        public StorageKeyBuilder Append(double data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteDoubleBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteDoubleLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(double);
            return this;
        }

        public StorageKeyBuilder Append(float data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteSingleBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteSingleLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(float);
            return this;
        }

        public StorageKeyBuilder Append(Half data, bool isBigEndian = false)
        {
            if (isBigEndian)
                BinaryPrimitives.WriteHalfBigEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            else
                BinaryPrimitives.WriteHalfLittleEndian(_memoryOwner.Memory.Span[_byteCount..], data);
            _byteCount += sizeof(short);
            return this;
        }

        public StorageKeyBuilder Append(BigInteger data, bool isBigEndian = false) =>
            Append(data.ToByteArray(true, isBigEndian));

        public StorageKeyBuilder Append(ReadOnlySpan<byte> data)
        {
            data.CopyTo(_memoryOwner.Memory.Span[_byteCount..]);
            _byteCount += data.Length;
            return this;
        }

        public StorageKeyBuilder Append(decimal data, bool isBigEndian = false)
        {
            Span<int> span = stackalloc int[4];

            _ = decimal.GetBits(data, span);

            foreach (var bit in span)
                _ = Append(bit);

            return this;
        }

        public StorageKeyBuilder Append(Guid data, bool isBigEndian = false) =>
            Append(data.ToByteArray(isBigEndian));

        public StorageKeyBuilder Append(ReadOnlySequence<byte> data) =>
            Append(data.ToArray());

        public StorageKeyBuilder Append(ArraySegment<byte> data) =>
            Append(data.AsSpan());

        public StorageKeyBuilder Append(DateTime stamp, TimeSpan offset = default) =>
            Append((ulong)CoreUtilities.ToUnixTimeMilliseconds(stamp, offset));

        public StorageKeyBuilder Append(TimeSpan span, TimeSpan offset = default) =>
            Append((ulong)CoreUtilities.ToUnixTimeMilliseconds(span, offset));

        public StorageKeyBuilder Append(DateTimeOffset stamp) =>
            Append((ulong)stamp.ToUnixTimeMilliseconds());

        public StorageKeyBuilder Append(TimeOnly time) =>
            Append(time.ToTimeSpan());

        public StorageKeyBuilder Append(DateOnly date, TimeOnly time = default) =>
            Append(date.ToDateTime(time));

        public StorageKeyBuilder Append(ReadOnlySequence<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        public StorageKeyBuilder Append(ReadOnlyMemory<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        public StorageKeyBuilder Append(ReadOnlySpan<char> data, bool isStrictUtf8 = true) =>
            Append(data.ToArray(), isStrictUtf8);

        public StorageKeyBuilder Append(char[] data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data)) :
                Append(Encoding.UTF8.GetBytes(data));

        public StorageKeyBuilder Append(char data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data.ToString())) :
                Append(Encoding.UTF8.GetBytes(data.ToString()));

        public StorageKeyBuilder Append(sbyte data) =>
            Append((byte)data);

        public StorageKeyBuilder Append(bool data) =>
            Append(data ? (byte)1 : (byte)0);

        public StorageKeyBuilder Append(byte[] data) =>
            Append(data.AsSpan());

        public StorageKeyBuilder Append(ReadOnlyMemory<byte> data) =>
            Append(data.Span);

        public StorageKeyBuilder Append(StorageKeyBuilder other) =>
            Append(other._memoryOwner.Memory[..other._byteCount].Span);

        public StorageKeyBuilder Append(UInt160 data) =>
            Append(data.ToArray());

        public StorageKeyBuilder Append(UInt256 data) =>
            Append(data.ToArray());

        public StorageKeyBuilder Append(string data, bool isStrictUtf8 = true) =>
            isStrictUtf8 ?
                Append(CoreUtilities.StrictUtf8Encoding.GetBytes(data)) :
                Append(Encoding.UTF8.GetBytes(data));

        public StorageKeyBuilder Append(byte prefix, ReadOnlySpan<byte> data) =>
            Append(prefix).Append(data);

        public StorageKeyBuilder Append(byte prefix, UInt160 data) =>
            Append(prefix).Append(data);

        public StorageKeyBuilder Append(byte prefix, UInt256 data) =>
            Append(prefix).Append(data);

        public byte[] ToArray() =>
            _memoryOwner.Memory[.._byteCount].ToArray();

        public StorageKeyBuilder Clone() =>
            new(_memoryOwner.Memory[.._byteCount].Span, _maxLength);

        public IEnumerator<byte> GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ToArray().GetEnumerator();
    }
}
