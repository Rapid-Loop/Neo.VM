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
    /// Represents a boolean stack item.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public class VMBoolean(bool value) : VMObject, IEquatable<VMBoolean>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Boolean;

        private readonly bool _value = value;

        /// <summary>
        /// Determines whether this boolean is equal to another boolean stack item.
        /// </summary>
        /// <param name="other">The other boolean.</param>
        /// <returns><see langword="true"/> if the values are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals(VMBoolean? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            return _value == other._value;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMBoolean);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value ? 1 : 0;
        }

        /// <inheritdoc />
        [return: MaybeNull]
        public override string? ToString()
        {
            return _value.ToString();
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var clone = new VMBoolean(_value);

            return clone;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return _value;
        }

        /// <inheritdoc />
        public override BigInteger GetInteger()
        {
            return _value ? BigInteger.One : BigInteger.Zero;
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            return _value ? [1] : [0];
        }
    }
}
