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

using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Platform.Hosting
{
    /// <summary>
    /// Runs a System.CommandLine parse result against a configured <see cref="IHost"/>.
    /// </summary>
    /// <param name="parseResult">The parsed command-line result to invoke.</param>
    /// <param name="host">The host that supplies services for the application lifetime.</param>
    /// <param name="invocationConfiguration">Invocation settings for System.CommandLine.</param>
    public sealed class PlatformCommandLine(ParseResult parseResult, IHost host, InvocationConfiguration invocationConfiguration)
    {
        /// <summary>
        /// Starts the host, invokes the command-line action, then stops the host.
        /// </summary>
        /// <param name="cancellationToken">A token that can cancel host start, command invocation, or host stop.</param>
        /// <returns>
        /// The process exit code from the command invocation, or the exception
        /// <see cref="Exception.HResult"/> if invocation fails.
        /// </returns>
        public async Task<int> InvokeAsync(CancellationToken cancellationToken = default)
        {
            await host.StartAsync(cancellationToken);

            var exitCode = 0;

            try
            {
                exitCode = await parseResult.InvokeAsync(invocationConfiguration, cancellationToken);
            }
            catch (Exception ex)
            {
                exitCode = ex.HResult;
            }
            finally
            {
                await host.StopAsync(cancellationToken);
            }


            return exitCode;
        }
    }
}
