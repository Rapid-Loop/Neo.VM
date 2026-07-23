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
using Neo.VM.Middleware;
using Neo.VM.Pipeline;
using System;
using System.Collections.Generic;

namespace Neo.VM.Builder
{
    /// <summary>
    /// Fluent builder for constructing a <see cref="VirtualMachinePipeline"/>.
    /// Middleware is executed in the order it is registered (first registered = first to run).
    /// </summary>
    public sealed class VirtualMachinePipelineBuilder
    {
        private readonly List<IEngineMiddleware> _middleware = [];


        private VirtualMachinePipelineBuilder() { }

        /// <summary>
        /// Creates a new <see cref="VirtualMachinePipelineBuilder"/> instance.
        /// </summary>
        /// <returns>A new builder.</returns>
        public static VirtualMachinePipelineBuilder Create() =>
            new();

        /// <summary>
        /// Registers a middleware instance.
        /// </summary>
        /// <param name="middleware">The middleware to register.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachinePipelineBuilder Use(IEngineMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        /// <summary>
        /// Registers a middleware by type (requires parameterless constructor).
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type to instantiate and register.</typeparam>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachinePipelineBuilder UseMiddleware<TMiddleware>()
            where TMiddleware : IEngineMiddleware, new()
        {
            return Use(new TMiddleware());
        }

        /// <summary>
        /// Registers multiple middleware's at once.
        /// </summary>
        /// <param name="middlewares">The middleware instances to register, in execution order.</param>
        /// <returns>This builder for chaining.</returns>
        public VirtualMachinePipelineBuilder Use(IEnumerable<IEngineMiddleware> middlewares)
        {
            foreach (var middleware in middlewares)
                Use(middleware);

            return this;
        }

        /// <summary>
        /// Builds the final optimized pipeline with separate chains for each hook.
        /// </summary>
        /// <returns>A pipeline ready for use by <see cref="VirtualMachineEngine"/>.</returns>
        public VirtualMachinePipeline Build()
        {
            var preExecution = BuildExecutionChain(mw => mw.PreExecution);
            var postExecution = BuildExecutionChain(mw => mw.PostExecution);
            var preExecute = BuildExecuteChain(mw => mw.PreExecute);
            var postExecute = BuildExecuteChain(mw => mw.PostExecute);

            return new
            (
                preExecution,
                postExecution,
                preExecute,
                postExecute
            );
        }

        private ExecuteDelegate BuildExecuteChain(Func<IEngineMiddleware, Action<ExecutionContext?, ExecuteDelegate>> selector)
        {
            ExecuteDelegate app = _ => { }; // terminal handler (no-op)

            // Build chain in reverse so first registered middleware runs first
            for (var i = _middleware.Count - 1; i >= 0; i--)
            {
                var middleware = _middleware[i];
                var current = selector(middleware);
                var next = app;

                app = context => current(context, next);
            }

            return app;
        }

        private ExecutionDelegate BuildExecutionChain(Func<IEngineMiddleware, Action<ExecutionDelegate>> selector)
        {
            ExecutionDelegate app = () => { }; // terminal handler (no-op)

            // Build chain in reverse so first registered middleware runs first
            for (var i = _middleware.Count - 1; i >= 0; i--)
            {
                var middleware = _middleware[i];
                var current = selector(middleware);
                var next = app;

                app = () => current(next);
            }

            return app;
        }
    }
}
