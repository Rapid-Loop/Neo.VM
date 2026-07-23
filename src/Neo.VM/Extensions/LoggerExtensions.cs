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

using Microsoft.Extensions.Logging;
using Neo.VM.Logging;
using System;

namespace Neo.VM.Extensions
{
    /// <summary>
    /// Source-generated logging helpers for virtual machine execution events.
    /// </summary>
    internal static partial class LoggerExtensions
    {
        /// <summary>
        /// Logs a VM execute message.
        /// </summary>
        [LoggerMessage(
            EventId = VirtualMachineEventId.Execute,
            EventName = nameof(VirtualMachineEventId.Execute),
            Message = "{Message}"
        )]
        public static partial void LogExecuteMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        /// <summary>
        /// Logs a VM load message.
        /// </summary>
        [LoggerMessage(
            EventId = VirtualMachineEventId.Load,
            EventName = nameof(VirtualMachineEventId.Load),
            Message = "{Message}"
        )]
        public static partial void LogLoadMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        /// <summary>
        /// Logs a VM fault message.
        /// </summary>
        [LoggerMessage(
            EventId = VirtualMachineEventId.Fault,
            EventName = nameof(VirtualMachineEventId.Fault),
            Message = "{Message}"
        )]
        public static partial void LogFaultMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        /// <summary>
        /// Logs a VM fault with an exception.
        /// </summary>
        [LoggerMessage(
            EventId = VirtualMachineEventId.Fault,
            Message = ""
        )]
        public static partial void LogFaultMessage(
            this ILogger logger,
            LogLevel logLevel,
            Exception exception
        );

        /// <summary>
        /// Logs a VM break (debugger breakpoint) message.
        /// </summary>
        [LoggerMessage(
            EventId = VirtualMachineEventId.Break,
            EventName = nameof(VirtualMachineEventId.Break),
            Message = "{Message}"
        )]
        public static partial void LogBreakMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );
    }
}
