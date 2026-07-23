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

using Neo.Core.VM.SmartContract;
using System;

namespace Neo.Core.VM.Attributes
{
    /// <summary>
    /// Describes an interoperable service method exposed to the VM, including pricing, permissions, and type metadata.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event,
        AllowMultiple = true
    )]
    public class MethodDescriptorAttribute : Attribute
    {
        /// <summary>
        /// The name of the interoperable service.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The fixed price for calling the interoperable service. It can be 0 if the interoperable service has a variable price.
        /// </summary>
        public required long ExecutePrice { get; init; }

        /// <summary>
        /// Required hard fork to be active.
        /// </summary>
        public required HardFork Fork { get; init; }

        /// <summary>
        /// The required <see cref="SmartContract.CallFlags"/> for the interoperable service.
        /// </summary>
        public required CallFlags CallFlags { get; init; } = CallFlags.None;

        /// <summary>
        /// Indicates whether the method is safe to invoke without changing chain state.
        /// </summary>
        public required bool Safe { get; init; }

        /// <summary>
        /// The return type of the interoperable service method.
        /// </summary>
        public required MethodParameterType ReturnType { get; init; } = MethodParameterType.Any;
    }
}
