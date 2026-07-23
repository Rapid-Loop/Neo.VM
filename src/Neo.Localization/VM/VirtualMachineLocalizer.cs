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
using System.Diagnostics.CodeAnalysis;

namespace Neo.Localization.VM
{
    /// <summary>
    /// Resolves localized virtual machine messages from resource files.
    /// </summary>
    public static class VirtualMachineLocalizer
    {
        private static readonly IStringLocalizer s_localizer = ResourceFactory.Instance.Create(typeof(VirtualMachineLocalizer));

        /// <summary>
        /// Gets a localized virtual machine message by name, optionally formatting it with the provided arguments.
        /// </summary>
        /// <param name="messageName">The resource key of the message, typically from <see cref="VirtualMachineMessageNames"/>.</param>
        /// <param name="args">Optional format arguments for the message template.</param>
        /// <returns>The localized message string.</returns>
        [return: NotNullIfNotNull(nameof(s_localizer))]
        public static string GetMessage(string messageName, params object[] args) =>
            s_localizer[messageName, args];
    }
}
