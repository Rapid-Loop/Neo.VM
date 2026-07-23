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

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;

namespace Neo.Localization
{
    /// <summary>
    /// Provides a shared <see cref="ResourceManagerStringLocalizerFactory"/> for Neo localization resources.
    /// </summary>
    public static class ResourceFactory
    {
        /// <summary>
        /// Gets the default string localizer factory configured with the <c>Resources</c> path.
        /// </summary>
        public static readonly ResourceManagerStringLocalizerFactory Instance = Create(config => config.ResourcesPath = "Resources");

        private static ResourceManagerStringLocalizerFactory Create(Action<LocalizationOptions> configure)
        {
            var localizedOptions = new LocalizationOptions();

            configure?.Invoke(localizedOptions);

            return new(Options.Create(localizedOptions), NullLoggerFactory.Instance);
        }
    }
}
