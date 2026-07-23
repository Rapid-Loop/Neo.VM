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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Platform.Hosting
{
    /// <summary>
    /// Host lifetime that logs Neo platform start and shutdown status messages.
    /// </summary>
    /// <param name="options">Lifetime options, including whether status messages are suppressed.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <param name="applicationLifetime">Application lifetime notifications used to log status.</param>
    /// <param name="loggerFactory">Optional logger factory. When omitted, a null logger is used.</param>
    public class NeoPlatformLifetime(
        IOptions<NeoPlatformLifetimeOptions> options,
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory? loggerFactory = default) : IHostLifetime, IDisposable
    {
        /// <summary>
        /// Gets the configured lifetime options.
        /// </summary>
        public NeoPlatformLifetimeOptions Options { get; } = options?.Value ??
                throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Gets the hosting environment.
        /// </summary>
        public IHostEnvironment Environment { get; } = environment ??
                throw new ArgumentNullException(nameof(environment));

        /// <summary>
        /// Gets the application lifetime used for start and stop notifications.
        /// </summary>
        public IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime ??
                throw new ArgumentNullException(nameof(applicationLifetime));

        private CancellationTokenRegistration _appStartedReg;
        private CancellationTokenRegistration _appStoppingReg;

        private readonly ILogger _logger = (loggerFactory ?? NullLoggerFactory.Instance)
                .CreateLogger("Neo.Platform.Hosting.Lifetime");

        /// <summary>
        /// Releases registrations for application started and stopping callbacks.
        /// </summary>
        public void Dispose()
        {
            _appStartedReg.Dispose();
            _appStoppingReg.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called when the host is stopping. This implementation performs no work.
        /// </summary>
        /// <param name="cancellationToken">A token that can cancel the stop operation.</param>
        /// <returns>A completed task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Nothing to do
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registers status logging for application start and stop when messages are not suppressed.
        /// </summary>
        /// <param name="cancellationToken">A token that can cancel waiting for start.</param>
        /// <returns>A completed task.</returns>
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (Options.SuppressStatusMessages == false)
            {
                _appStartedReg = ApplicationLifetime.ApplicationStarted.Register
                (
                    static state =>
                    {
                        if (state is NeoPlatformLifetime pl)
                            pl.OnApplicationStarted();
                    },
                    this
                );

                _appStoppingReg = ApplicationLifetime.ApplicationStopping.Register
                (
                    static state =>
                    {
                        if (state is NeoPlatformLifetime pl)
                            pl.OnApplicationStopping();
                    },
                    this
                );
            }

            return Task.CompletedTask;
        }

        private void OnApplicationStarted()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Neo platform started. Press Ctrl+C to shut down.");
                _logger.LogInformation("Hosting environment: {envName}", Environment.EnvironmentName);
                _logger.LogInformation("Content root path: {contentRoot}", Environment.ContentRootPath);
            }
        }

        private void OnApplicationStopping()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Neo platform is shutting down...");
        }
    }
}
