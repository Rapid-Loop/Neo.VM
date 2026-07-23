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
using System;
using System.Diagnostics.CodeAnalysis;

namespace Neo.VM.Core
{
    /// <summary>
    /// Represents an opcode handler invoked by the engine for a single instruction.
    /// </summary>
    /// <param name="engine">The execution engine.</param>
    /// <param name="instruction">The instruction being executed.</param>
    public delegate void VTableFunc(VirtualMachineEngine engine, OpCodeInst instruction);

    /// <summary>
    /// Dispatch table that maps <see cref="OpCode"/> values to handler methods.
    /// Methods named after opcodes are auto-bound in the constructor via reflection.
    /// </summary>
    public partial class VirtualTable
    {
        /// <summary>
        /// Gets the default virtual table with standard NeoVM opcode handlers.
        /// </summary>
        public static readonly VirtualTable Default = new();

        /// <summary>
        /// Gets the array of handlers indexed by opcode byte value.
        /// </summary>
        public VTableFunc[] Functions { get; protected set; } = new VTableFunc[byte.MaxValue];

        /// <summary>
        /// Gets or sets the handler for the specified opcode.
        /// </summary>
        /// <param name="opCode">The opcode to look up.</param>
        /// <returns>The registered handler for <paramref name="opCode"/>.</returns>
        public VTableFunc this[OpCode opCode]
        {
            get => Functions[(byte)opCode];
            protected set => Functions[(byte)opCode] = value;
        }

        /// <summary>
        /// Initializes a new virtual table, filling undefined opcodes with <see cref="InvalidOpcode"/>
        /// and binding public methods whose names match <see cref="OpCode"/> members.
        /// </summary>
        public VirtualTable()
        {
            Array.Fill(Functions, InvalidOpcode);

            foreach (var mi in GetType().GetMethods())
            {
                if (Enum.TryParse<OpCode>(mi.Name, true, out var opCode))
                    Functions[(byte)opCode] = mi.CreateDelegate<VTableFunc>(this);
            }
        }


        /// <summary>
        /// Default handler for opcodes that have no registered implementation.
        /// </summary>
        /// <param name="engine">The execution engine.</param>
        /// <param name="instruction">The undefined instruction.</param>
        /// <exception cref="InvalidOperationException">Always thrown for undefined opcodes.</exception>
        [DoesNotReturn]
        public static void InvalidOpcode(VirtualMachineEngine engine, OpCodeInst instruction)
        {
            throw new InvalidOperationException($"Opcode {instruction.OpCode} is undefined.");
        }
    }
}
