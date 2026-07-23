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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Platform.CLI.Commands
{
    /// <summary>
    /// CLI command that displays configuration samples and related information.
    /// </summary>
    internal class ShowCommand : Command
    {
        /// <summary>
        /// Initializes the <c>show</c> command.
        /// </summary>
        public ShowCommand() : base("show", "Shows many configurations")
        {

        }

        /// <summary>
        /// Handles the <c>show</c> command by writing sample configuration output.
        /// </summary>
        /// <param name="loggerFactory">The factory used to create the command logger.</param>
        public class Handler(ILoggerFactory loggerFactory) : AsynchronousCommandLineAction
        {
            private readonly ILogger _logger = loggerFactory.CreateLogger<ShowCommand>();

            /// <summary>
            /// Invokes the <c>show</c> command handler.
            /// </summary>
            /// <param name="parseResult">The parse result for the current command invocation.</param>
            /// <param name="cancellationToken">A token that can cancel the operation.</param>
            /// <returns>A completed task with exit code 0.</returns>
            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                _logger.LogInformation("Hello World!!!!");

                return Task.FromResult(0);
            }
        }
    }
}
