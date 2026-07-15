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
using Neo.Localization.VM;
using Neo.VM.Core;
using Neo.VM.Extensions;
using System;

namespace Neo.VM.Middleware
{
    public sealed class ExecuteLoggerMiddleware(IServiceProvider sp, ILogger<VirtualMachineEngine> logger) : IEngineMiddleware
    {
        private static readonly decimal s_factor = 1_00000000m;

        private VirtualMachineEngine Engine => sp.GetRequiredService<VirtualMachineEngine>();

        public void PostExecute(ExecutionContext? context, ExecuteDelegate next)
        {
            next(context);
        }

        public void PreExecution(ExecutionDelegate next)
        {
            var logLevel = LogLevel.Information;

            if (logger.IsEnabled(logLevel))
            {
                var message = VirtualMachineLocalizer.GetMessage(
                    VirtualMachineMessageNames.ExecuteStartup,
                    Engine.ActiveFork,
                    Engine.GasLimit / s_factor
                );

                logger.LogExecuteMessage(logLevel, message);

                if (Engine.State == VMState.FAULT)
                    logger.LogFaultMessage(LogLevel.Critical, Engine.FaultException!);
            }

            next();
        }

        public void PreExecute(ExecutionContext? context, ExecuteDelegate next)
        {
            var logLevel = LogLevel.Trace;

            if (context is not null && logger.IsEnabled(logLevel))
            {
                var opcodeInst = context.CurrentInstruction;
                var message = VirtualMachineLocalizer.GetMessage(
                    VirtualMachineMessageNames.ExecuteOpCode,
                    context.InstructionPointer,
                    opcodeInst.OpCode,
                    opcodeInst.DecodeOperand()
                );

                logger.LogExecuteMessage(logLevel, message);
            }

            next(context);
        }

        public void PostExecution(ExecutionDelegate next)
        {
            var logLevel = LogLevel.Information;

            if (logger.IsEnabled(logLevel))
            {
                var message = VirtualMachineLocalizer.GetMessage(
                    VirtualMachineMessageNames.ExecuteSuccessfully,
                    Engine.GasConsumed / s_factor,
                    Engine.GasLeft / s_factor,
                    Engine.State
                );

                logger.LogExecuteMessage(logLevel, message);
            }

            next();
        }
    }
}
