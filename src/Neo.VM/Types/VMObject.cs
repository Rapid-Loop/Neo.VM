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
using Neo.Core.VM.Interface;
using Neo.Core.VM.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;

namespace Neo.VM.Types
{
    /// <summary>
    /// Base type for all NeoVM stack items.
    /// Provides equality, cloning, byte serialization, reference counting, and conversion helpers.
    /// </summary>
    public abstract class VMObject : IVMComponent, IEquatable<VMObject>
    {
        /// <summary>
        /// Gets the NeoVM stack-item type of this object.
        /// </summary>
        public virtual VMObjectType Type => VMObjectType.Any;

        /// <summary>
        /// Gets the byte length of this object's serialized representation.
        /// </summary>
        public int Size => AsSpan().Length;

        /// <summary>
        /// Gets the current reference count for this stack item.
        /// </summary>
        public int RefCount => _refCount;

        private bool _disposed = false;
        private int _refCount = 0;

        #region IEquatable

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMObject);
        }

        /// <summary>
        /// Determines whether this object is equal to another stack item.
        /// </summary>
        /// <param name="other">The other stack item.</param>
        /// <returns><see langword="true"/> if the items are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] VMObject? other)
        {
            if (other is null) return false;
            if (Type != other.Type) return false;
            if (_disposed != other._disposed) return false;
            if (_refCount != other._refCount) return false;
            return AsSpan().SequenceEqual(other.AsSpan());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return AsSpan().ToHashCode(RefCount ^ 397);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases managed and unmanaged resources used by this instance.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>; otherwise <see langword="false"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup managed resources
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Releases resources used by this stack item.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer that releases unmanaged resources if <see cref="Dispose()"/> was not called.
        /// </summary>
        ~VMObject()
        {
            Dispose(false);
        }

        #endregion

        #region References

        internal void AddReference()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            Interlocked.Increment(ref _refCount);
        }

        internal void Release()
        {
            if (_disposed) return;

            var newCount = Interlocked.Decrement(ref _refCount);

            if (newCount < 0)
                Dispose(true);

        }

        #endregion

        /// <summary>
        /// Determines whether this object graph contains a circular reference.
        /// </summary>
        /// <returns><see langword="true"/> if a cycle is detected; otherwise <see langword="false"/>.</returns>
        public bool HasCircularReference()
        {
            var visited = new HashSet<VMObject>(ReferenceEqualityComparer.Instance);
            return DetectCycle(this, visited);
        }

        internal virtual IEnumerable<VMObject> GetChildren() =>
            [];

        /// <summary>
        /// Returns a deep clone of this object.
        /// </summary>
        /// <returns>A new stack item with the same value.</returns>
        public abstract VMObject Clone();

        /// <summary>
        /// Internal clone helper that shares the cycle map across the entire object graph.
        /// </summary>
        protected internal virtual VMObject Clone(Dictionary<VMObject, VMObject> objectMap)
        {
            if (objectMap.TryGetValue(this, out var existing)) return existing;
            var clone = CloneCore(objectMap); // Call derived implementation
            objectMap[this] = clone;          // Register immediately
            return clone;
        }

        /// <summary>
        /// Override this in derived classes for actual cloning logic (without map).
        /// </summary>
        protected virtual VMObject CloneCore(Dictionary<VMObject, VMObject> objectMap)
        {
            return Clone();
        }

        /// <summary>
        /// Converts this object to a <see cref="ReadOnlySpan{T}"/> of bytes for serialization or storage.
        /// </summary>
        /// <returns>The byte representation of this object.</returns>
        public ReadOnlySpan<byte> AsSpan()
        {
            var visited = new HashSet<VMObject>(ReferenceEqualityComparer.Instance);
            return GetSafeSpan(visited);
        }

        /// <summary>
        /// Internal method that derived classes should override
        /// </summary>
        protected internal ReadOnlySpan<byte> GetSafeSpan(HashSet<VMObject> visited)
        {
            if (visited.Contains(this))
                return []; // Cycle detected - return empty

            visited.Add(this);

            var result = ComputeSpan(visited);

            visited.Remove(this);

            return result;
        }

        /// <summary>
        /// Override this in derived classes to provide their byte representation
        /// </summary>
        protected virtual ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            return [];
        }

        /// <summary>
        /// Converts this object to a signed integer, if supported by the concrete type.
        /// </summary>
        /// <returns>The integer representation of this object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the type cannot be converted to an integer.</exception>
        public virtual BigInteger GetInteger()
        {
            throw new InvalidOperationException($"Cannot convert {Type} to integer");
        }

        /// <summary>
        /// Converts this object to a boolean.
        /// Non-null stack items are treated as <see langword="true"/> by default.
        /// </summary>
        /// <returns>The boolean representation of this object.</returns>
        public virtual bool GetBoolean()
        {
            return true; // NeoVM treats most non-null objects as true
        }

        private static bool DetectCycle(VMObject current, HashSet<VMObject> visited)
        {
            if (current == null)
                return false;

            if (visited.Contains(current))
                return true; // Cycle found!

            visited.Add(current);

            // Check all child objects
            foreach (var child in current.GetChildren())
            {
                if (DetectCycle(child, visited))
                    return true;
            }

            visited.Remove(current);

            return false;
        }

        /// <summary>Converts a <see cref="byte"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(byte value) =>
            new VMInteger(value);

        /// <summary>Converts an <see cref="sbyte"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(sbyte value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="short"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(short value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="ushort"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(ushort value) =>
            new VMInteger(value);

        /// <summary>Converts an <see cref="int"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(int value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="uint"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(uint value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="long"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(long value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="ulong"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(ulong value) =>
            new VMInteger(value);

        /// <summary>Converts an <see cref="Int128"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(Int128 value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="UInt128"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(UInt128 value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="BigInteger"/> to a <see cref="VMInteger"/>.</summary>
        public static implicit operator VMObject(BigInteger value) =>
            new VMInteger(value);

        /// <summary>Converts a <see cref="bool"/> to a <see cref="VMBoolean"/>.</summary>
        public static implicit operator VMObject(bool value) =>
            new VMBoolean(value);

        /// <summary>Converts a byte array to a <see cref="VMByteArray"/>.</summary>
        public static implicit operator VMObject(byte[] value) =>
            new VMByteArray(value);

        /// <summary>Converts a UTF-8 string to a <see cref="VMByteArray"/>.</summary>
        public static implicit operator VMObject(string value) =>
            new VMByteArray(CoreUtilities.StrictUtf8Encoding.GetBytes(value));

        /// <summary>Converts a stack item to a <see cref="byte"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator byte(VMObject value) =>
            (byte)value.GetInteger();

        /// <summary>Converts a stack item to an <see cref="sbyte"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator sbyte(VMObject value) =>
            (sbyte)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="short"/> via integer conversion.</summary>
        public static implicit operator short(VMObject value) =>
            new VMInteger(value);

        /// <summary>Converts a stack item to a <see cref="ushort"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator ushort(VMObject value) =>
            (ushort)value.GetInteger();

        /// <summary>Converts a stack item to an <see cref="int"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator int(VMObject value) =>
            (int)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="uint"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator uint(VMObject value) =>
            (uint)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="long"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator long(VMObject value) =>
            (long)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="ulong"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator ulong(VMObject value) =>
            (ulong)value.GetInteger();

        /// <summary>Converts a stack item to an <see cref="Int128"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator Int128(VMObject value) =>
            (Int128)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="UInt128"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator UInt128(VMObject value) =>
            (UInt128)value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="BigInteger"/> via <see cref="GetInteger"/>.</summary>
        public static implicit operator BigInteger(VMObject value) =>
            value.GetInteger();

        /// <summary>Converts a stack item to a <see cref="bool"/> via <see cref="GetBoolean"/>.</summary>
        public static implicit operator bool(VMObject value) =>
            value.GetBoolean();

        /// <summary>Converts a stack item to a byte array via <see cref="AsSpan"/>.</summary>
        public static implicit operator byte[](VMObject value) =>
            [.. value.AsSpan()];

        /// <summary>Converts a stack item to its string representation.</summary>
        public static implicit operator string(VMObject value) =>
            $"{value}";
    }
}
