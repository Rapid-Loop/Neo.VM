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

using Neo.Core.VM.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents an arbitrary-precision integer stack item, limited to <see cref="MaxSize"/> bytes.
    /// </summary>
    public class VMInteger : VMObject, IEquatable<VMInteger>
    {
        /// <summary>
        /// The maximum allowed size of the integer in bytes.
        /// </summary>
        public const int MaxSize = 32;

        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Integer;

        private readonly BigInteger _value = BigInteger.Zero;

        /// <summary>
        /// Initializes a new integer stack item.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> exceeds <see cref="MaxSize"/> bytes.</exception>
        public VMInteger(BigInteger value)
        {
            if (value.GetByteCount() > MaxSize)
                throw new ArgumentException($"Integer size bytes exceeds maximum allowed size of {MaxSize} bytes.", nameof(value));
            _value = value;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMInteger);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var clone = new VMInteger(_value);

            return clone;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return _value != BigInteger.Zero;
        }

        /// <inheritdoc />
        public override BigInteger GetInteger()
        {
            return _value;
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            return _value.ToByteArray();
        }

        /// <summary>
        /// Determines whether this integer is equal to another integer stack item.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns><see langword="true"/> if the values are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] VMInteger? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            return _value == other._value;
        }

        /// <summary>
        /// Adds two integer stack items.
        /// </summary>
        public static VMInteger operator +(VMInteger a, VMInteger b)
        {
            var result = new VMInteger(a._value + b._value);

            return result;
        }

        /// <summary>
        /// Subtracts two integer stack items.
        /// </summary>
        public static VMInteger operator -(VMInteger a, VMInteger b)
        {
            var result = new VMInteger(a._value - b._value);

            return result;
        }

        /// <summary>
        /// Multiplies two integer stack items.
        /// </summary>
        public static VMInteger operator *(VMInteger a, VMInteger b)
        {
            var result = new VMInteger(a._value * b._value);

            return result;
        }

        /// <summary>
        /// Divides two integer stack items.
        /// </summary>
        /// <exception cref="DivideByZeroException">Thrown when the divisor is zero.</exception>
        public static VMInteger operator /(VMInteger a, VMInteger b)
        {
            if (b._value == BigInteger.Zero)
                throw new DivideByZeroException("Division by zero in VMInteger");

            var result = new VMInteger(a._value / b._value);

            return result;
        }

        /// <summary>
        /// Computes the remainder of dividing two integer stack items.
        /// </summary>
        public static VMInteger operator %(VMInteger a, VMInteger b)
        {
            var result = new VMInteger(a._value % b._value);

            return result;
        }

        /// <summary>
        /// Negates an integer stack item.
        /// </summary>
        public static VMInteger operator -(VMInteger a) =>
            new(-a._value);

        // Comparison operators
        /// <summary>
        /// Determines whether two integer stack items are equal.
        /// </summary>
        public static bool operator ==(VMInteger a, VMInteger b)
        {
            if (ReferenceEquals(a, b)) return true;
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two integer stack items are not equal.
        /// </summary>
        public static bool operator !=(VMInteger a, VMInteger b) =>
            !(a == b);

        /// <summary>
        /// Determines whether the first integer is greater than the second.
        /// </summary>
        public static bool operator >(VMInteger a, VMInteger b) =>
            a._value > b._value;

        /// <summary>
        /// Determines whether the first integer is less than the second.
        /// </summary>
        public static bool operator <(VMInteger a, VMInteger b) =>
            a._value < b._value;

        /// <summary>
        /// Determines whether the first integer is greater than or equal to the second.
        /// </summary>
        public static bool operator >=(VMInteger a, VMInteger b) =>
            a._value >= b._value;

        /// <summary>
        /// Determines whether the first integer is less than or equal to the second.
        /// </summary>
        public static bool operator <=(VMInteger a, VMInteger b) =>
            a._value <= b._value;

        /// <summary>
        /// Converts a <see cref="VMInteger"/> to a <see cref="BigInteger"/>.
        /// </summary>
        public static implicit operator BigInteger(VMInteger value)
        {
            return value.GetInteger();
        }
    }
}
