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
using Microsoft.Extensions.Hosting;
using Neo.Platform.Hosting.Middleware;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Neo.Platform.Hosting.Builder
{
    /// <summary>
    /// Fluent builder that wires System.CommandLine parsing with a Microsoft.Extensions host.
    /// </summary>
    /// <param name="rootCommand">
    /// The root command to parse. When <see langword="null"/>, a new <see cref="RootCommand"/> is used.
    /// </param>
    /// <param name="args">
    /// Command-line arguments to parse when <see cref="Build"/> is called.
    /// When <see langword="null"/>, an empty argument list is used.
    /// </param>
    public class PlatformCommandLineBuilder(Command? rootCommand = default, string[]? args = default)
    {
        internal Command Command { get; } = rootCommand ?? new RootCommand();
        internal ParserConfiguration ParserConfiguration { get; } = new();
        internal InvocationConfiguration InvocationConfiguration { get; } = new();

        private readonly List<Action<ParseResult, PlatformCommandLineNextDelegate>> _middleware = [];
        private IHostBuilder? _hostBuilder;

        internal void AddMiddleware(Action<ParseResult, PlatformCommandLineNextDelegate> middleware) =>
            _middleware.Add(middleware);

        /// <summary>
        /// Configures host creation middleware that runs after parsing.
        /// </summary>
        /// <param name="hostBuilderFactory">
        /// Factory that creates an <see cref="IHostBuilder"/> from unmatched tokens.
        /// When <see langword="null"/>, a new <see cref="HostBuilder"/> is created.
        /// </param>
        /// <param name="configure">Optional callback that further configures the host builder.</param>
        /// <returns>The same builder for chaining.</returns>
        /// <remarks>
        /// The middleware stores the <see cref="ParseResult"/> on the host builder properties,
        /// registers it for DI, and enables <see cref="HostingExtensions.UseNeoPlatformLifetime"/>.
        /// </remarks>
        public PlatformCommandLineBuilder UseHost(
            Func<string[], IHostBuilder>? hostBuilderFactory,
            Action<IHostBuilder>? configure = default)
        {
            AddMiddleware(async (parseResult, next) =>
            {
                var argsRemaining = parseResult.UnmatchedTokens.ToArray();
                _hostBuilder = hostBuilderFactory?.Invoke(argsRemaining) ??
                    new HostBuilder();

                _hostBuilder.Properties[typeof(ParseResult)] = parseResult;

                _hostBuilder.ConfigureServices(services =>
                {
                    services.AddTransient(_ => parseResult);
                });

                _hostBuilder.UseNeoPlatformLifetime();
                configure?.Invoke(_hostBuilder);

                next(parseResult);
            });

            return this;
        }

        /// <summary>
        /// Enables or disables the default System.CommandLine exception handler.
        /// </summary>
        /// <param name="value"><see langword="true"/> to enable the handler; otherwise, <see langword="false"/>.</param>
        /// <returns>The same builder for chaining.</returns>
        public PlatformCommandLineBuilder EnableDefaultExceptionHandler(bool value = true)
        {
            InvocationConfiguration.EnableDefaultExceptionHandler = value;
            return this;
        }

        /// <summary>
        /// Sets the process termination timeout used during command invocation.
        /// </summary>
        /// <param name="timeout">The termination timeout.</param>
        /// <returns>The same builder for chaining.</returns>
        public PlatformCommandLineBuilder SetProcessTerminationTimeout(TimeSpan timeout)
        {
            InvocationConfiguration.ProcessTerminationTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Enables or disables POSIX-style option bundling for the parser.
        /// </summary>
        /// <param name="value"><see langword="true"/> to enable bundling; otherwise, <see langword="false"/>.</param>
        /// <returns>The same builder for chaining.</returns>
        public PlatformCommandLineBuilder EnablePosixBundling(bool value = true)
        {
            ParserConfiguration.EnablePosixBundling = value;
            return this;
        }

        /// <summary>
        /// Parses the configured arguments, runs middleware, builds the host, and returns a runnable command line.
        /// </summary>
        /// <returns>A <see cref="PlatformCommandLine"/> that can start the host and invoke the command.</returns>
        public PlatformCommandLine Build()
        {
            var parseResults = Command.Parse(args ?? [], ParserConfiguration);

            BuildChain().Invoke(parseResults);

            var host = (_hostBuilder ?? new HostBuilder()).Build();
            return new(parseResults, host, InvocationConfiguration);
        }

        private PlatformCommandLineNextDelegate BuildChain()
        {
            PlatformCommandLineNextDelegate app = _ => { }; // terminal handler (no-op)

            // Build chain in reverse so first registered middleware runs first
            for (var i = _middleware.Count - 1; i >= 0; i--)
            {
                var middleware = _middleware[i];
                var current = middleware;
                var next = app;

                app = context => current(context, next);
            }

            return app;
        }
    }
}
