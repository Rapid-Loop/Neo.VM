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
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents a null stack item. All null values compare equal.
    /// </summary>
    public class VMNull : VMObject, IEquatable<VMNull>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Any;

        /// <summary>
        /// Gets a new null stack item instance.
        /// </summary>
        public static VMNull Instance => new();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // Never dispose the singleton instance
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return true;
            return Equals(obj as VMNull);
        }

        /// <summary>
        /// Determines whether this null item is equal to another null item.
        /// </summary>
        /// <param name="other">The other null item.</param>
        /// <returns>Always <see langword="true"/> for any <see cref="VMNull"/> comparison.</returns>
        public bool Equals([NotNullWhen(true)] VMNull? other)
        {
            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 0;
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            return this;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return false;
        }

        /// <inheritdoc />
        public override BigInteger GetInteger()
        {
            return BigInteger.Zero;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "\0";
        }

        /// <summary>
        /// Always returns <see langword="true"/>; all null stack items are equal.
        /// </summary>
        public static bool operator ==(VMNull left, VMNull right) =>
            true;

        /// <summary>
        /// Always returns <see langword="false"/>; all null stack items are equal.
        /// </summary>
        public static bool operator !=(VMNull left, VMNull right) =>
            false;

    }
}
