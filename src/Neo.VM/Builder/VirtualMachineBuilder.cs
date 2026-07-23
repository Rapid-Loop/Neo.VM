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
using Neo.Core.Blockchain;
using Neo.Core.VM;
using Neo.VM.Core;
using Neo.VM.Pipeline;
using System;

namespace Neo.VM.Builder
{
    /// <summary>
    /// Fluent builder for constructing a <see cref="VirtualMachineEngine"/>.
    /// </summary>
    public sealed class VirtualMachineBuilder
    {
        private VirtualTable? _vtable;
        private ExecutionEngineLimits? _engineLimits;
        private VirtualMachinePipeline? _pipeline;

        private ProtocolSettings? _protocolSettings;

        private Transaction? _tx;
        private Block? _persistingBlock;

        private VirtualMachineBuilder() { }

        /// <summary>
        /// Creates a new <see cref="VirtualMachineBuilder"/> instance.
        /// </summary>
        /// <returns>A new builder.</returns>
        public static VirtualMachineBuilder Create() =>
            new();

        /// <summary>
        /// Configures the middleware pipeline used during execution.
        /// </summary>
        /// <param name="pipeline">The pipeline to use.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UsePipeline(VirtualMachinePipeline pipeline)
        {
            _pipeline = pipeline;
            return this;
        }

        /// <summary>
        /// Configures the middleware pipeline via a nested <see cref="VirtualMachinePipelineBuilder"/>.
        /// </summary>
        /// <param name="config">An action that registers middleware on the pipeline builder.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UsePipeline(Action<VirtualMachinePipelineBuilder> config)
        {
            var pb = VirtualMachinePipelineBuilder.Create();
            config(pb);
            UsePipeline(pb.Build());
            return this;
        }

        /// <summary>
        /// Sets the execution limits for the engine.
        /// </summary>
        /// <param name="limits">The limits to apply.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UseLimits(ExecutionEngineLimits limits)
        {
            _engineLimits = limits;
            return this;
        }

        /// <summary>
        /// Sets the block being persisted, used to resolve the active hard fork.
        /// </summary>
        /// <param name="persistingBlock">The block under persistence.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UseBlock(Block persistingBlock)
        {
            _persistingBlock = persistingBlock;
            return this;
        }

        /// <summary>
        /// Sets the protocol settings used by the engine.
        /// </summary>
        /// <param name="protocolSettings">The protocol settings.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UseProtocolSettings(ProtocolSettings protocolSettings)
        {
            _protocolSettings = protocolSettings;
            return this;
        }

        /// <summary>
        /// Sets the opcode dispatch table used by the engine.
        /// </summary>
        /// <param name="vtable">The virtual table of opcode handlers.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UseVirtualTable(VirtualTable vtable)
        {
            _vtable = vtable;
            return this;
        }

        /// <summary>
        /// Sets the transaction container, which supplies the system-fee gas limit.
        /// </summary>
        /// <param name="tx">The transaction to execute against.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachineBuilder UseTransaction(Transaction tx)
        {
            _tx = tx;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="VirtualMachineEngine"/> from the configured options.
        /// Unspecified options use engine defaults.
        /// </summary>
        /// <returns>A new engine instance.</returns>
        public VirtualMachineEngine Build() =>
            new
            (
                _persistingBlock,
                _tx,
                _protocolSettings,
                _vtable,
                _engineLimits,
                _pipeline
            );
    }
}
