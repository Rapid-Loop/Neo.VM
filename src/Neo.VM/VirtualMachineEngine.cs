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
using Neo.Core.Blockchain.Interface;
using Neo.Core.VM;
using Neo.VM.Core;
using Neo.VM.Extensions;
using Neo.VM.Pipeline;
using Neo.VM.Types;
using System;
using System.Collections.Generic;

namespace Neo.VM
{
    /// <summary>
    /// Represents the VM used to execute the script.
    /// </summary>
    public partial class VirtualMachineEngine : IDisposable
    {
        /// <summary>
        /// Gets the current execution status of the VM.
        /// </summary>
        public VMState State { get; internal set; } = VMState.NONE;

        /// <summary>
        /// Gets the total amount of gas consumed so far during execution.
        /// </summary>
        public long GasConsumed => _maxGasConsumed;

        /// <summary>
        /// Gets the remaining gas available before the limit is reached.
        /// </summary>
        public long GasLeft => _maxGasLimit - _maxGasConsumed;

        /// <summary>
        /// Gets the maximum gas allowed for this execution.
        /// </summary>
        public long GasLimit => _maxGasLimit;

        /// <summary>
        /// Restrictions on the VM.
        /// </summary>
        public ExecutionEngineLimits Limits => _limits;

        /// <summary>
        /// The invocation stack of the VM.
        /// </summary>
        public Stack<ExecutionContext> InvocationStack => _invocationStack;

        /// <summary>
        /// The top frame of the invocation stack.
        /// </summary>
        public ExecutionContext? CurrentContext => _currentContext;

        /// <summary>
        /// The bottom frame of the invocation stack.
        /// </summary>
        public ExecutionContext? EntryContext => _entryContext;

        /// <summary>
        /// The stack to store the return values.
        /// </summary>
        public Stack<VMObject> ResultStack { get; } = [];

        /// <summary>
        /// Gets the exception that caused a <see cref="VMState.FAULT"/>, if any.
        /// </summary>
        public Exception? FaultException { get; private set; }

        /// <summary>
        /// Gets the hard fork active for the current persisting block.
        /// </summary>
        public HardFork ActiveFork => _currentFork;


        protected readonly VirtualTable CurrentOpCodeTable;

        private readonly Stack<ExecutionContext> _invocationStack = [];
        private readonly ExecutionEngineLimits _limits;
        private readonly long _maxGasLimit;

        private readonly VirtualMachinePipeline _pipeline;

        private readonly ProtocolSettings _protocolSettings;
        private readonly Block _persistingBlock;
        private readonly IVerifiable _container;
        private readonly HardFork _currentFork;

        private ExecutionContext? _currentContext;
        private ExecutionContext? _entryContext;

        private long _maxGasConsumed;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualMachineEngine"/> class.
        /// </summary>
        /// <param name="persistingBlock">The block being persisted; when <see langword="null"/>, an empty block is used.</param>
        /// <param name="container">The verifiable container (typically a transaction) that supplies the gas limit.</param>
        /// <param name="protocolSettings">Protocol settings used to resolve the active hard fork; defaults to <see cref="ProtocolSettings.Default"/>.</param>
        /// <param name="opCodeTable">The opcode dispatch table; defaults to <see cref="VirtualTable.Default"/>.</param>
        /// <param name="limits">Execution limits; defaults to <see cref="ExecutionEngineLimits.Default"/>.</param>
        /// <param name="pipeline">Middleware pipeline; defaults to <see cref="VirtualMachinePipeline.Empty"/>.</param>
        public VirtualMachineEngine(
            Block? persistingBlock = default,
            IVerifiable? container = default,
            ProtocolSettings? protocolSettings = default,
            VirtualTable? opCodeTable = default,
            ExecutionEngineLimits? limits = default,
            VirtualMachinePipeline? pipeline = default)
        {
            _persistingBlock = persistingBlock ?? new();
            _container = container ?? new Transaction();
            _protocolSettings = protocolSettings ?? ProtocolSettings.Default;
            _maxGasLimit = container is Transaction tx ? tx.SystemFee : ExecutionEngineLimits.MaxGas;
            CurrentOpCodeTable = opCodeTable ?? VirtualTable.Default;
            _limits = limits ?? ExecutionEngineLimits.Default;
            _currentFork = _protocolSettings.GetActiveHardFork(_persistingBlock.Index);
            _pipeline = pipeline ?? VirtualMachinePipeline.Empty;
        }

        /// <summary>
        /// Releases resources used by the engine.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads a script as a new root <see cref="ExecutionContext"/> and pushes it onto the invocation stack.
        /// </summary>
        /// <param name="script">The bytecode to execute.</param>
        /// <param name="initialPosition">Reserved for future use; the context starts at offset zero.</param>
        /// <returns>The loaded root execution context.</returns>
        public ExecutionContext LoadScript(byte[] script, int initialPosition = 0)
        {
            var activeFork = _protocolSettings.GetActiveHardFork(_persistingBlock.Index);

            var rootContext = new ExecutionContext(
                script,
                activeFork,
                _persistingBlock.Index,
                _maxGasLimit - _maxGasConsumed
            );

            LoadContext(rootContext);
            return rootContext;
        }

        /// <summary>
        /// Loads the specified context into the invocation stack.
        /// </summary>
        /// <param name="context">The context to load.</param>
        internal virtual void LoadContext(ExecutionContext context)
        {
            if (_invocationStack.Count >= Limits.MaxInvocationStackSize)
                throw new InvalidOperationException($"{nameof(Limits.MaxInvocationStackSize)} exceed: {InvocationStack.Count}");

            _entryContext ??= context;
            _currentContext = context;

            // NOTE: May need to update the context gas with leftover gas

            InvocationStack.Push(context);
        }

        /// <summary>
        /// Called when a context is unloaded.
        /// </summary>
        /// <param name="context">The context being unloaded.</param>
        internal virtual void UnloadedContext(ExecutionContext context)
        {
            _maxGasConsumed += context.GasConsumed;

            if (context.Parent is not null)
                _currentContext = context.Parent;
            else if (InvocationStack.Count > 0)
                _currentContext = InvocationStack.Peek();
            else
            {
                _currentContext = null;
                _entryContext = null;
            }

            context.Cleanup();
        }

        /// <summary>
        /// Runs the loaded script until completion, a fault, or a breakpoint.
        /// </summary>
        /// <returns>The final <see cref="VMState"/> after execution.</returns>
        public VMState Execute()
        {
            _maxGasConsumed = 0;

            _pipeline.PreExecution();

            try
            {
                while (_currentContext is not null && State >= VMState.BREAK)
                {
                    var context = _currentContext;

                    _pipeline.PreExecute(context);

                    if (context.ConsumeGas(context.CurrentInstruction.OpCode))
                    {
                        CurrentOpCodeTable[context.CurrentInstruction.OpCode]
                        (
                            this,
                            context.CurrentInstruction
                        );
                    }

                    if (_invocationStack.Count > 0 &&
                        context == _invocationStack.Peek()
                        && context.ShouldContinue())
                        context.InstructionPointer += context.CurrentInstruction.Size;

                    _pipeline.PostExecute(context);

                    if (State == VMState.BREAK) break;
                }

            }
            catch (Exception ex)
            {
                _maxGasConsumed += _currentContext.GetTotalGasConsumed();
                OnFault(ex);
            }
            finally
            {
                Cleanup();
                _pipeline.PostExecution();
            }

            return State;
        }

        /// <summary>
        /// Called when an exception that cannot be caught by the VM is thrown.
        /// </summary>
        /// <param name="ex">The exception that caused the <see cref="VMState.FAULT"/> state.</param>
        internal void OnFault(Exception ex)
        {
            State = VMState.FAULT;
            FaultException = ex;
        }

        private void Cleanup()
        {
            while (_invocationStack.Count > 0)
            {
                var context = _invocationStack.Pop();
                context.Cleanup();
            }
        }
    }
}
