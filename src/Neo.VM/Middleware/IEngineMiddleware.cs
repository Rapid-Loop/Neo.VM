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

using Neo.VM.Core;

namespace Neo.VM.Middleware
{
    /// <summary>
    /// Represents a middleware handler invoked around individual opcode execution.
    /// </summary>
    /// <param name="context">The current execution context, or <see langword="null"/> when none is active.</param>
    public delegate void ExecuteDelegate(ExecutionContext? context);

    /// <summary>
    /// Represents a middleware handler invoked around the entire VM run.
    /// </summary>
    public delegate void ExecutionDelegate();

    /// <summary>
    /// Defines middleware hooks that participate in the <see cref="Pipeline.VirtualMachinePipeline"/>.
    /// Implementations must invoke the provided next delegate to continue the chain.
    /// </summary>
    public interface IEngineMiddleware
    {
        /// <summary>
        /// Called when VM starts execution.
        /// </summary>
        /// <param name="next">The next handler in the pre-execution chain.</param>
        void PreExecution(ExecutionDelegate next);

        /// <summary>
        /// Called when VM finishes execution (HALT, FAULT, etc.).
        /// </summary>
        /// <param name="next">The next handler in the post-execution chain.</param>
        void PostExecution(ExecutionDelegate next);

        /// <summary>
        /// Called before each opcode is executed.
        /// </summary>
        /// <param name="context">The current execution context, if any.</param>
        /// <param name="next">The next handler in the pre-execute chain.</param>
        void PreExecute(ExecutionContext? context, ExecuteDelegate next);

        /// <summary>
        /// Called after each opcode is executed.
        /// </summary>
        /// <param name="context">The current execution context, if any.</param>
        /// <param name="next">The next handler in the post-execute chain.</param>
        void PostExecute(ExecutionContext? context, ExecuteDelegate next);
    }
}
