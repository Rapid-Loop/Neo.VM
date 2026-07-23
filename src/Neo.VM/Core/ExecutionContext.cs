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

using Neo.Core.VM;
using Neo.Core.VM.Specs;
using Neo.VM.Types;
using System;
using System.Collections.Generic;

namespace Neo.VM.Core
{
    /// <summary>
    /// Represents a single invocation frame: script, instruction pointer, gas budget, and evaluation stack.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Gets the script being executed (bytecode), including a trailing <c>RET</c> appended at construction.
        /// </summary>
        public ReadOnlyMemory<byte> Script => _script;

        /// <summary>
        /// Gets the pointer indicating the current instruction.
        /// </summary>
        public int InstructionPointer { get; internal set; }

        /// <summary>
        /// Gets the current <see cref="OpCodeInst"/> at <see cref="InstructionPointer"/>.
        /// </summary>
        public OpCodeInst CurrentInstruction => new(_script[InstructionPointer..]);

        /// <summary>
        /// Gets the next <see cref="OpCodeInst"/> after the current instruction.
        /// </summary>
        public OpCodeInst NextInstruction => new(_script[(InstructionPointer + CurrentInstruction.Size)..]);

        /// <summary>
        /// Gets the current stack frame that owns the evaluation stack and locals.
        /// </summary>
        public StackFrame Frame { get; }

        /// <summary>
        /// Gets the amount of gas consumed by this context so far.
        /// </summary>
        public long GasConsumed => _maxGasConsumed;

        /// <summary>
        /// Gets a value indicating whether this context is still eligible to execute instructions.
        /// </summary>
        public bool IsExecuting { get; internal set; } = true;

        /// <summary>
        /// Gets the parent context for nested calls, if any.
        /// </summary>
        public ExecutionContext? Parent => _parentContext;

        /// <summary>
        /// Gets a dictionary of custom state objects keyed by type (for example, runtime services).
        /// </summary>
        public Dictionary<Type, object> State { get; } = [];

        /// <summary>
        /// Gets the hard fork under which this context executes.
        /// </summary>
        public HardFork HardFork => _fork;

        /// <summary>
        /// Gets the block height associated with this context.
        /// </summary>
        public uint BlockHeight => _blockHeight;

        /// <summary>
        /// Gets the depth of this context in the invocation chain.
        /// </summary>
        public int Depth => _contextDepth;

        private readonly ReadOnlyMemory<byte> _script;
        private readonly HardFork _fork;
        private readonly uint _blockHeight;

        private readonly ExecutionContext? _parentContext;
        private readonly int _contextDepth;

        private long _initialGasLeft;
        private long _maxGasConsumed;

        /// <summary>
        /// Initializes a new execution context for the specified script.
        /// </summary>
        /// <param name="script">The bytecode to execute. A trailing <c>RET</c> is appended automatically.</param>
        /// <param name="fork">The hard fork that determines gas costs and opcode behavior.</param>
        /// <param name="blockHeight">The block height associated with this execution.</param>
        /// <param name="initialGas">The gas budget available to this context.</param>
        /// <param name="depth">The invocation depth of this context.</param>
        /// <param name="parent">The parent context for nested calls, or <see langword="null"/> for a root context.</param>
        public ExecutionContext(byte[] script, HardFork fork = HardFork.Genesis, uint blockHeight = 0, long initialGas = 1_000000L, int depth = 0, ExecutionContext? parent = null)
        {
            script = [.. script, (byte)OpCode.RET];

            _script = script.Clone() as byte[] ?? throw new ArgumentNullException(nameof(script));
            _blockHeight = blockHeight;
            _fork = fork;

            _initialGasLeft = initialGas;
            _contextDepth = depth;
            _parentContext = parent;

            Frame = new(_parentContext?.Frame);
        }

        /// <summary>
        /// Determines whether execution should continue for this context.
        /// </summary>
        /// <returns><see langword="true"/> if the context is active, has remaining script bytes, and has gas remaining.</returns>
        public bool ShouldContinue()
        {
            return IsExecuting && InstructionPointer < Script.Length && _initialGasLeft > 0;
        }

        /// <summary>
        /// Attempts to consume the gas cost for the specified opcode.
        /// </summary>
        /// <param name="opcode">The opcode whose gas cost should be charged.</param>
        /// <returns><see langword="true"/> if gas was available and deducted; otherwise <see langword="false"/> and execution is stopped.</returns>
        public bool ConsumeGas(OpCode opcode)
        {
            var cost = GasTable.GetGasCost(opcode, HardFork);

            if (_initialGasLeft < cost)
            {
                IsExecuting = false;
                return false;
            }

            _initialGasLeft -= cost;
            _maxGasConsumed += cost;
            return true;
        }

        /// <summary>
        /// Pushes a value onto the current frame's evaluation stack.
        /// </summary>
        /// <param name="item">The stack item to push.</param>
        /// <param name="addReferenceItem">Whether to increment the item's reference count.</param>
        /// <param name="addReferenceChildren">Whether to increment reference counts of child items.</param>
        public void Push(VMObject item, bool addReferenceItem = true, bool addReferenceChildren = true)
        {
            Frame.Push(item, addReferenceItem, addReferenceChildren);
        }

        /// <summary>
        /// Pops a value from the current frame's evaluation stack.
        /// </summary>
        /// <param name="releaseReferenceItem">Whether to release the item's reference count.</param>
        /// <param name="releaseReferenceChildren">Whether to release reference counts of child items.</param>
        /// <returns>The popped stack item.</returns>
        public VMObject Pop(bool releaseReferenceItem = true, bool releaseReferenceChildren = true)
        {
            return Frame.Pop(releaseReferenceItem, releaseReferenceChildren);
        }

        /// <summary>
        /// Peeks at the top of the current frame's evaluation stack without removing it.
        /// </summary>
        /// <returns>The top stack item.</returns>
        public VMObject Peek()
        {
            return Frame.Peek();
        }

        /// <summary>
        /// Peeks at an item on the evaluation stack by depth without removing it.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <returns>The stack item at the specified depth.</returns>
        public VMObject Peek(int index)
        {
            return Frame.Peek(index);
        }

        /// <summary>
        /// Inserts an item into the evaluation stack at the specified depth.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, VMObject item)
        {
            Frame.Insert(index, item);
        }

        /// <summary>
        /// Swaps two items on the evaluation stack by depth.
        /// </summary>
        /// <param name="fromIndex">The first depth index.</param>
        /// <param name="toIndex">The second depth index.</param>
        public void Swap(int fromIndex, int toIndex)
        {
            Frame.Swap(fromIndex, toIndex);
        }

        /// <summary>
        /// Reverses the order of the top <paramref name="n"/> items on the evaluation stack.
        /// </summary>
        /// <param name="n">The number of items from the top to reverse.</param>
        public void Reverse(int n)
        {
            Frame.Reverse(n);
        }

        /// <summary>
        /// Removes and returns the evaluation-stack item at the specified depth.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <returns>The removed stack item.</returns>
        public VMObject RemoveAt(int index)
        {
            return Frame.RemoveAt(index);
        }

        /// <summary>
        /// Clears the current frame's evaluation stack and releases all references.
        /// </summary>
        public void Clear()
        {
            Frame.Clear();
        }

        /// <summary>
        /// Releases stack resources and marks this context as no longer executing.
        /// </summary>
        public void Cleanup()
        {
            Frame.Cleanup();
            IsExecuting = false;

            // Clear custom state
            State.Clear();
        }
    }
}
