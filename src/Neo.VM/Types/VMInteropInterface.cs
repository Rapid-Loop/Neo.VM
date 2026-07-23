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
using Neo.Core.VM.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents an interop interface stack item that wraps a CLR object.
    /// </summary>
    /// <param name="obj">The underlying object.</param>
    /// <param name="name">A display name for the interface type.</param>
    public class VMInteropInterface(object obj, string name) : VMObject, IEquatable<VMInteropInterface>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Interop;

        private readonly object _underlyingObject = obj;
        private readonly string _interfaceName = name;

        /// <summary>
        /// Gets the underlying CLR object.
        /// </summary>
        public object UnderlyingObject => _underlyingObject;

        /// <summary>
        /// Gets the interface name associated with this wrapper.
        /// </summary>
        public string InterfaceName => _interfaceName;

        /// <summary>
        /// Initializes a new interop interface wrapping a default <see cref="object"/>.
        /// </summary>
        public VMInteropInterface() : this(new()) { }

        /// <summary>
        /// Initializes a new interop interface wrapping the specified object.
        /// The interface name defaults to the object's type name.
        /// </summary>
        /// <param name="obj">The object to wrap.</param>
        public VMInteropInterface(object obj) : this(obj, obj.GetType().Name) { }

        /// <summary>
        /// Determines whether this interop item is equal to another.
        /// </summary>
        /// <param name="other">The other interop item.</param>
        /// <returns><see langword="true"/> if the underlying objects and names match; otherwise <see langword="false"/>.</returns>
        public bool Equals(VMInteropInterface? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            return Equals(_underlyingObject, other._underlyingObject) &&
                string.Equals(_interfaceName, other._interfaceName);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMInteropInterface);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(RefCount, _underlyingObject, _interfaceName);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // Do not dispose the underlying object - it's managed externally
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var clone = new VMInteropInterface(_underlyingObject, _interfaceName);

            return clone;
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            var ptr = nint.Zero;
            byte[] bytes;
            try
            {
                var size = Marshal.SizeOf(_underlyingObject);

                if (size <= 0)
                    return CoreUtilities.StrictUtf8Encoding.GetBytes(_interfaceName);

                bytes = GC.AllocateUninitializedArray<byte>(size);

                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(_underlyingObject, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);

                return bytes;
            }
            catch
            {
                return CoreUtilities.StrictUtf8Encoding.GetBytes(_interfaceName);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <inheritdoc />
        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            // Most interop objects cannot be converted to integer
            throw new InvalidOperationException($"Cannot convert InteropInterface ({_interfaceName}) to integer");
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return _underlyingObject != null;
        }

        /// <summary>
        /// Casts the underlying object to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <returns>The underlying object as <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when the underlying object is not assignable to <typeparamref name="T"/>.</exception>
        public T CastTo<T>()
        {
            if (_underlyingObject is T t)
                return t;

            throw new InvalidCastException($"This {_interfaceName} can't be casted to type {typeof(T)}.");
        }

        /// <summary>
        /// Invokes a public method on the underlying object by name.
        /// </summary>
        /// <typeparam name="TResult">The expected return type (must be a reference type).</typeparam>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The method result cast to <typeparamref name="TResult"/>, or <see langword="null"/> if the method is missing or returns a different type.</returns>
        public TResult? CallMethod<TResult>(string methodName, params object[] args)
            where TResult : class?
        {
            var method = _underlyingObject.GetType().GetMethod(methodName);

            return method?.Invoke(_underlyingObject, args) as TResult;
        }
    }
}
