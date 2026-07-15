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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo.VM.Core;
using Neo.VM.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.VM.Middleware
{
    public sealed class DebuggerMiddleware(IServiceProvider sp, ILogger<DebuggerMiddleware> logger) : IEngineMiddleware
    {
        public event EventHandler<DebuggerEventArgs>? OnBreakpoint;

        public bool StepMode { get; set; }

        private static readonly decimal s_factor = 1_00000000m;

        private VirtualMachineEngine Engine => sp.GetRequiredService<VirtualMachineEngine>();

        private readonly HashSet<int> _breakpoints = []; // script offset breakpoints
        private int _lastStepPosition = -1;

        public void AddBreakpoint(int scriptOffset) =>
            _breakpoints.Add(scriptOffset);

        public void Continue() =>
            Engine.State = VMState.NONE;

        public void PreExecution(ExecutionDelegate next)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var message = string.Join(", ", _breakpoints.Select(static s => $"\"L{s:D4}\""));

                logger.LogExecuteMessage(LogLevel.Debug, $"Starting debugger | Breakpoints: [{message}]");
            }

            next();
        }

        public void PostExecution(ExecutionDelegate next)
        {
            StepMode = false;

            next();
        }

        public void PreExecute(ExecutionContext? context, ExecuteDelegate next)
        {
            if (context is null)
            {
                next(context);
                return;
            }

            var position = context.InstructionPointer;
            var instruction = context.CurrentInstruction;

            // Breakpoint hit?
            if (_breakpoints.Contains(position) || (StepMode && position != _lastStepPosition))
            {
                StepMode = true; // stay in step mode
                _lastStepPosition = position;

                Engine.State = VMState.BREAK;

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    var gasLeft = (Engine.GasLimit - context.GasConsumed) / s_factor;
                    var gasConsumed = context.GasConsumed / s_factor;
                    var message = $"Breakpoint hit | GasConsumed: {gasConsumed:N8} | GasLeft: {gasLeft:N8}";

                    logger.LogBreakMessage(LogLevel.Debug, message);
                }

                OnBreakpoint?.Invoke(this, new(context));
            }

            next(context);
        }

        public void PostExecute(ExecutionContext? context, ExecuteDelegate next)
        {
            next(context);
        }
    }

    public class DebuggerEventArgs(ExecutionContext context) : EventArgs
    {
        public ExecutionContext Context => context;
    }
}
