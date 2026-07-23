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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace Neo.Platform.Hosting.Logging
{
    /// <summary>
    /// <see cref="ILoggerProvider"/> that creates <see cref="NeoPlatformLogger"/> instances.
    /// </summary>
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("NeoPlatform")]
    internal sealed class NeoPlatformLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private readonly ConcurrentDictionary<string, NeoPlatformLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

        private NeoPlatformLoggerOptions _currentConfig;

        /// <summary>
        /// Initializes a new provider that tracks option changes from the monitor.
        /// </summary>
        /// <param name="config">Options monitor for logger configuration.</param>
        public NeoPlatformLoggerProvider(
            IOptionsMonitor<NeoPlatformLoggerOptions> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        /// <summary>
        /// Creates or returns a cached logger for the specified category.
        /// </summary>
        /// <param name="categoryName">Logger category name.</param>
        /// <returns>An <see cref="ILogger"/> for the category.</returns>
        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(
                categoryName,
                name =>
                    new(name, GetCurrentConfig)
            );

        private NeoPlatformLoggerOptions GetCurrentConfig() =>
            _currentConfig;

        /// <summary>
        /// Clears cached loggers and disposes option change subscriptions.
        /// </summary>
        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
