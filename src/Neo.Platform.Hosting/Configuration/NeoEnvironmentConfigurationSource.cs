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

using Microsoft.Extensions.Configuration;
using Neo.Platform.Hosting.Provider;
using System.Collections.Generic;

namespace Neo.Platform.Hosting.Configuration
{
    /// <summary>
    /// Configuration source that supplies Neo environment-related settings.
    /// </summary>
    internal class NeoEnvironmentConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Optional initial key/value pairs merged into the configuration provider.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string?>>? InitialData { get; set; }

        /// <summary>
        /// Builds a configuration provider for this source.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>A <see cref="NeoEnvironmentConfigurationProvider"/> instance.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new NeoEnvironmentConfigurationProvider(this);
    }
}
