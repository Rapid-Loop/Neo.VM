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

using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Hosting.Logging
{
    /// <summary>
    /// Options that control Neo platform console log formatting.
    /// </summary>
    public sealed class NeoPlatformLoggerOptions
    {
        /// <summary>
        /// The default timestamp format string used by <see cref="TimestampFormat"/>.
        /// </summary>
        public const string DefaultDateTimeFormatString = "yyyy-MM-dd HH:mm:ss.ffff";

        /// <summary>
        /// Gets or sets a value indicating whether log timestamps use UTC.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool UseUtcTimestamp { get; set; } = true;

        /// <summary>
        /// Gets or sets the .NET date/time format string used for log timestamps.
        /// Defaults to <see cref="DefaultDateTimeFormatString"/>.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        public string TimestampFormat { get; set; } = DefaultDateTimeFormatString;

        /// <summary>
        /// Gets or sets a value indicating whether exception stack traces are written to the console.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool ShowExceptionStackTrace { get; set; } = true;
    }
}
