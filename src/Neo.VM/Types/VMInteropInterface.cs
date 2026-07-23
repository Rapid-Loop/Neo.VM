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
    public class VMInteropInterface(object obj, string name) : VMObject, IEquatable<VMInteropInterface>
    {
        public override VMObjectType Type => VMObjectType.Interop;

        private readonly object _underlyingObject = obj;
        private readonly string _interfaceName = name;

        public object UnderlyingObject => _underlyingObject;
        public string InterfaceName => _interfaceName;

        public VMInteropInterface() : this(new()) { }

        public VMInteropInterface(object obj) : this(obj, obj.GetType().Name) { }

        public bool Equals(VMInteropInterface? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;
            return Equals(_underlyingObject, other._underlyingObject) &&
                string.Equals(_interfaceName, other._interfaceName);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMInteropInterface);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RefCount, _underlyingObject, _interfaceName);
        }

        protected override void Dispose(bool disposing)
        {
            // Do not dispose the underlying object - it's managed externally
            base.Dispose(disposing);
        }

        public override VMObject Clone()
        {
            var clone = new VMInteropInterface(_underlyingObject, _interfaceName);

            return clone;
        }

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

        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            // Most interop objects cannot be converted to integer
            throw new InvalidOperationException($"Cannot convert InteropInterface ({_interfaceName}) to integer");
        }

        public override bool GetBoolean()
        {
            return _underlyingObject != null;
        }

        public T CastTo<T>()
        {
            if (_underlyingObject is T t)
                return t;

            throw new InvalidCastException($"This {_interfaceName} can't be casted to type {typeof(T)}.");
        }

        public TResult? CallMethod<TResult>(string methodName, params object[] args)
            where TResult : class?
        {
            var method = _underlyingObject.GetType().GetMethod(methodName);

            return method?.Invoke(_underlyingObject, args) as TResult;
        }
    }
}
