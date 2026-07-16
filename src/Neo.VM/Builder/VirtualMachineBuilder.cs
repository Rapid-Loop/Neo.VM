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
    public sealed class VirtualMachineBuilder
    {
        private VirtualTable? _vtable;
        private ExecutionEngineLimits? _engineLimits;
        private VirtualMachinePipeline? _pipeline;

        private ProtocolSettings? _protocolSettings;

        private Transaction? _tx;
        private Block? _persistingBlock;

        private VirtualMachineBuilder() { }

        public static VirtualMachineBuilder Create() =>
            new();

        public VirtualMachineBuilder UsePipeline(VirtualMachinePipeline pipeline)
        {
            _pipeline = pipeline;
            return this;
        }

        public VirtualMachineBuilder UsePipeline(Action<VirtualMachinePipelineBuilder> config)
        {
            var pb = VirtualMachinePipelineBuilder.Create();
            config(pb);
            UsePipeline(pb.Build());
            return this;
        }

        public VirtualMachineBuilder UseLimits(ExecutionEngineLimits limits)
        {
            _engineLimits = limits;
            return this;
        }

        public VirtualMachineBuilder UseBlock(Block persistingBlock)
        {
            _persistingBlock = persistingBlock;
            return this;
        }

        public VirtualMachineBuilder UseProtocolSettings(ProtocolSettings protocolSettings)
        {
            _protocolSettings = protocolSettings;
            return this;
        }

        public VirtualMachineBuilder UseVirtualTable(VirtualTable vtable)
        {
            _vtable = vtable;
            return this;
        }

        public VirtualMachineBuilder UseTransaction(Transaction tx)
        {
            _tx = tx;
            return this;
        }

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
