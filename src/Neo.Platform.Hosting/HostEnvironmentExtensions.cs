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

namespace Neo.Platform.Hosting
{
    /// <summary>
    /// Extension methods for checking Neo hosting environment names.
    /// </summary>
    public static class HostEnvironmentExtensions
    {
        /// <summary>
        /// Determines whether the host is running in the localnet environment.
        /// </summary>
        /// <param name="hostEnvironment">The host environment.</param>
        /// <returns><see langword="true"/> if the environment is localnet; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostEnvironment"/> is <see langword="null"/>.</exception>
        public static bool IsLocalNet(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment, nameof(hostEnvironment));
            return hostEnvironment.IsEnvironment(NeoHostingEnvironmentNames.LocalNet);
        }

        /// <summary>
        /// Determines whether the host is running in the testnet environment.
        /// </summary>
        /// <param name="hostEnvironment">The host environment.</param>
        /// <returns><see langword="true"/> if the environment is testnet; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostEnvironment"/> is <see langword="null"/>.</exception>
        public static bool IsTestNet(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment, nameof(hostEnvironment));
            return hostEnvironment.IsEnvironment(NeoHostingEnvironmentNames.TestNet);
        }

        /// <summary>
        /// Determines whether the host is running in the mainnet environment.
        /// </summary>
        /// <param name="hostEnvironment">The host environment.</param>
        /// <returns><see langword="true"/> if the environment is mainnet; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostEnvironment"/> is <see langword="null"/>.</exception>
        public static bool IsMainNet(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment, nameof(hostEnvironment));
            return hostEnvironment.IsEnvironment(NeoHostingEnvironmentNames.MainNet);
        }

        /// <summary>
        /// Determines whether the host is running in the privatenet environment.
        /// </summary>
        /// <param name="hostEnvironment">The host environment.</param>
        /// <returns><see langword="true"/> if the environment is privatenet; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostEnvironment"/> is <see langword="null"/>.</exception>
        public static bool IsPrivateNet(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment, nameof(hostEnvironment));
            return hostEnvironment.IsEnvironment(NeoHostingEnvironmentNames.PrivateNet);
        }
    }
}
