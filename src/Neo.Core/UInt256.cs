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
using Neo.Core.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Neo.Core
{
    /// <summary>
    /// Represents a 256-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public class UInt256 : IComparable, IComparable<UInt256>, IEquatable<UInt256>, INeoSerializable
    {
        /// <summary>
        /// The length of <see cref="UInt256"/> values.
        /// </summary>
        public const int Length = 32;

        /// <summary>
        /// Represents 0.
        /// </summary>
        public static UInt256 Zero => new();

        [FieldOffset(0)] private ulong _value1;
        [FieldOffset(8)] private ulong _value2;
        [FieldOffset(16)] private ulong _value3;
        [FieldOffset(24)] private ulong _value4;

        /// <summary>
        /// Gets the serialized size of this value in bytes.
        /// </summary>
        public int Size => Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt256"/> class.
        /// </summary>
        public UInt256() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt256"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="UInt256"/>.</param>
        public UInt256(ReadOnlySpan<byte> value)
        {
            if (value.Length != Length)
                throw new FormatException($"Invalid UInt256 length: expected {Length} bytes, but got {value.Length} bytes. UInt256 values must be exactly 32 bytes long.");

            var span = MemoryMarshal.CreateSpan(ref Unsafe.As<ulong, byte>(ref _value1), Length);

            value.CopyTo(span);
        }

        /// <summary>
        /// Compares this instance to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or <see langword="null"/>.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="obj"/>.
        /// </returns>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            return CompareTo(obj as UInt256);
        }

        /// <summary>
        /// Compares this instance to another <see cref="UInt256"/>.
        /// </summary>
        /// <param name="other">An <see cref="UInt256"/> to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="other"/>.
        /// </returns>
        public int CompareTo(UInt256? other)
        {
            if (other is null) return 1;
            var result = _value4.CompareTo(other._value4);
            if (result != 0) return result;
            result = _value3.CompareTo(other._value3);
            if (result != 0) return result;
            result = _value2.CompareTo(other._value2);
            if (result != 0) return result;
            return _value1.CompareTo(other._value1);
        }

        /// <summary>
        /// Deserializes this value from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        public void Deserialize(Stream reader)
        {
            _value1 = reader.Read<ulong>();
            _value2 = reader.Read<ulong>();
            _value3 = reader.Read<ulong>();
            _value4 = reader.Read<ulong>();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="UInt256"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as UInt256);
        }

        /// <summary>
        /// Determines whether the specified <see cref="UInt256"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="UInt256"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(UInt256? other)
        {
            if (other is null) return false;
            return _value1 == other._value1
                && _value2 == other._value2
                && _value3 == other._value3
                && _value4 == other._value4;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="UInt256"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 397;   // ← Prime seed

                hash *= 31 ^ _value1.GetHashCode();
                hash *= 31 ^ _value2.GetHashCode();
                hash *= 31 ^ _value3.GetHashCode();
                hash *= 31 ^ _value4.GetHashCode();

                return hash;
            }
        }

        /// <summary>
        /// Serializes this value to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(_value1);
            writer.Write(_value2);
            writer.Write(_value3);
            writer.Write(_value4);
        }

        /// <summary>
        /// Returns the hex string representation of this value with a <c>0x</c> prefix.
        /// </summary>
        /// <returns>A hex string of this <see cref="UInt256"/>.</returns>
        public override string ToString()
        {
            return "0x" + Convert.ToHexStringLower([.. this.ToArray().Reverse()]);
        }

        /// <summary>
        /// Parses an <see cref="UInt256"/> from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">An <see cref="UInt256"/> represented by a <see cref="string"/>.</param>
        /// <param name="result">The parsed <see cref="UInt256"/>.</param>
        /// <returns>
        /// <see langword="true"/> if an <see cref="UInt256"/> is successfully parsed; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryParse(string value, [NotNullWhen(true)] out UInt256? result)
        {
            result = null;
            var data = value.AsSpan().TrimStartIgnoreCase("0x");
            if (data.Length != Length * 2) return false;
            try
            {
                result = new UInt256([.. Convert.FromHexString(data).Reverse()]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses an <see cref="UInt256"/> from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">An <see cref="UInt256"/> represented by a <see cref="string"/>.</param>
        /// <returns>The parsed <see cref="UInt256"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> is not in the correct format.</exception>
        public static UInt256 Parse(string value)
        {
            var data = value.AsSpan().TrimStartIgnoreCase("0x");
            if (data.Length != Length * 2)
                throw new FormatException($"Invalid UInt256 string format: expected {Length * 2} hexadecimal characters, but got {data.Length}. UInt256 values must be represented as 64 hexadecimal characters (with or without '0x' prefix).");
            return new UInt256([.. Convert.FromHexString(data).Reverse()]);
        }

        /// <summary>
        /// Implicitly converts a hex string to a <see cref="UInt256"/>.
        /// </summary>
        /// <param name="s">A hex string representation of a <see cref="UInt256"/>.</param>
        public static implicit operator UInt256(string s)
        {
            return Parse(s);
        }

        /// <summary>
        /// Implicitly converts a 32-byte array to a <see cref="UInt256"/>.
        /// </summary>
        /// <param name="b">The little-endian 32-byte value.</param>
        public static implicit operator UInt256(byte[] b)
        {
            return new UInt256(b);
        }

        /// <summary>
        /// Determines whether two <see cref="UInt256"/> values are equal.
        /// </summary>
        public static bool operator ==(UInt256? left, UInt256? right)
        {
            if (left is null || right is null)
                return Equals(left, right);
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="UInt256"/> values are not equal.
        /// </summary>
        public static bool operator !=(UInt256? left, UInt256? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether one <see cref="UInt256"/> is greater than another.
        /// </summary>
        public static bool operator >(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether one <see cref="UInt256"/> is greater than or equal to another.
        /// </summary>
        public static bool operator >=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Determines whether one <see cref="UInt256"/> is less than another.
        /// </summary>
        public static bool operator <(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether one <see cref="UInt256"/> is less than or equal to another.
        /// </summary>
        public static bool operator <=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) <= 0;
        }
    }
}
