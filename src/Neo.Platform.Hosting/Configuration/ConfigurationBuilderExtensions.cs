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
using System.Collections.Generic;

namespace Neo.Platform.Hosting.Configuration
{
    /// <summary>
    /// Extension methods for adding Neo platform configuration sources.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds Neo platform defaults and <c>NEO_</c>-prefixed environment variables
        /// as a configuration source.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="initialData">
        /// Optional key/value pairs that override platform defaults before environment variables are applied.
        /// </param>
        /// <returns>The same <paramref name="builder"/> for chaining.</returns>
        public static IConfigurationBuilder AddNeoPlatformConfiguration(this IConfigurationBuilder builder, IEnumerable<KeyValuePair<string, string?>>? initialData = default)
        {
            builder.Add(new NeoEnvironmentConfigurationSource() { InitialData = initialData, });
            return builder;
        }
    }
}
