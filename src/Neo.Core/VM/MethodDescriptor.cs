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
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo.Core.VM
{
    /// <summary>
    /// Describes an interoperable service method that can be invoked from the VM via <see cref="OpCode.SYSCALL"/>.
    /// </summary>
    public class MethodDescriptor : IEquatable<MethodDescriptor>
    {
        /// <summary>
        /// The name of the interoperable service.
        /// </summary>
        public string SystemMethodName => _systemMethodName;

        /// <summary>
        /// The hash of the interoperable service.
        /// </summary>
        public uint Hash => _hash;

        /// <summary>
        /// The parameters of the interoperable service.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters => _targetMethodInfo.GetParameters();

        /// <summary>
        /// The method info of the interoperable service.
        /// </summary>
        public MethodInfo TargetMethodInfo => _targetMethodInfo;

        /// <summary>
        /// The method used to handle the interoperable service.
        /// </summary>
        public Delegate TargetMethod => _targetMethod;

        private readonly uint _hash;
        private readonly string _systemMethodName;
        private readonly MethodInfo _targetMethodInfo;
        private readonly Delegate _targetMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescriptor"/> class.
        /// </summary>
        /// <param name="targetInstance">The instance that owns the target method, property, or event.</param>
        /// <param name="targetMethodName">The name of the method, property, or event to bind.</param>
        /// <param name="systemMethodName">
        /// Optional system method name used for the call address.
        /// When omitted, a name of the form <c>System.{Type}.{Method}</c> is generated.
        /// </param>
        /// <exception cref="TargetException">Thrown when the named target cannot be found on the instance type.</exception>
        public MethodDescriptor(object targetInstance, string targetMethodName, string? systemMethodName = default)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var type = targetInstance.GetType();

            _targetMethodInfo = type.GetMethod(targetMethodName, bindingFlags) ??
                type.GetProperty(targetMethodName, bindingFlags)?.GetMethod ??
                type.GetEvent(targetMethodName, bindingFlags)?.GetRaiseMethod(true) ??
                throw new TargetException($"Target {targetMethodName} not found in {type}.");

            _systemMethodName = string.IsNullOrWhiteSpace(systemMethodName) ?
                $"System.{type.Name}.{_targetMethodInfo.Name}" :
                systemMethodName;

            var paramTypes = _targetMethodInfo.GetParameters().Select(static s => s.ParameterType);
            var delegateType = Expression.GetDelegateType([.. paramTypes, _targetMethodInfo.ReturnType]);

            _targetMethod = _targetMethodInfo.IsStatic ?
                _targetMethodInfo.CreateDelegate(delegateType) :
                _targetMethodInfo.CreateDelegate(delegateType, targetInstance);

            _hash = CreateCallAddress(_systemMethodName);
        }

        /// <summary>
        /// Determines whether this descriptor refers to the same interoperable service as another.
        /// </summary>
        /// <param name="other">The other descriptor to compare.</param>
        /// <returns><see langword="true"/> if both descriptors share the same hash; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MethodDescriptor? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            return _hash == other._hash;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as MethodDescriptor);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            checked
            {
                return (int)_hash;
            }
        }

        /// <summary>
        /// Creates a 32-bit call address from a system method name by hashing it with SHA-256.
        /// </summary>
        /// <param name="systemMethodName">The interoperable service name to hash.</param>
        /// <returns>The first four little-endian bytes of the SHA-256 hash.</returns>
        public static uint CreateCallAddress(string systemMethodName) =>
            BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes(systemMethodName).ToSha256());

        /// <summary>
        /// Converts the descriptor to its interoperable service hash.
        /// </summary>
        /// <param name="descriptor">The descriptor to convert.</param>
        public static implicit operator uint(MethodDescriptor descriptor)
        {
            return descriptor._hash;
        }
    }
}
