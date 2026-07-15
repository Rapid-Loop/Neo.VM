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
using Neo.VM.Builder;
using Neo.VM.Middleware;
using System;

namespace Neo.VM.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExecutionEngine(this IServiceCollection services)
        {
            // Register VirtualMachineEngine using the built pipeline
            services.AddScoped(
                sp =>
                {
                    var middleware = sp.GetServices<IEngineMiddleware>(); // NOTE: Get all custom middleware
                    var debugger = sp.GetService<DebuggerMiddleware>(); // NOTE: get the debugger middleware

                    if (debugger is not null)
                        middleware = [debugger, .. middleware];

                    var pipeline = VirtualMachinePipelineBuilder.Create()
                        .Use(middleware) // NOTE: ORDER MATTERS HERE
                        .Build();

                    return VirtualMachineBuilder.Create()
                        .UsePipeline(pipeline)
                        .Build();
                }
            );

            return services;
        }

        public static IServiceCollection AddEngineMiddleware<TMiddleware>(this IServiceCollection services)
            where TMiddleware : class, IEngineMiddleware
        {
            services.AddSingleton<IEngineMiddleware, TMiddleware>();

            return services;
        }

        public static IServiceCollection AddEngineMiddlewareDebugger(this IServiceCollection services)
        {
            services.AddScoped<DebuggerMiddleware, DebuggerMiddleware>();

            return services;
        }

        public static IServiceCollection AddEngineMiddlewareLogger(this IServiceCollection services)
        {
            services.AddScoped<IEngineMiddleware, ExecuteLoggerMiddleware>();

            return services;
        }

        public static IServiceCollection AddEngineMiddleware(this IServiceCollection services,
            params Type[] middlewareTypes)
        {
            foreach (var type in middlewareTypes)
            {
                if (typeof(IEngineMiddleware).IsAssignableFrom(type) == false)
                    throw new ArgumentException($"Type {type} must implement {nameof(IEngineMiddleware)}");

                services.AddSingleton(typeof(IEngineMiddleware), type);
            }

            return services;
        }
    }
}
