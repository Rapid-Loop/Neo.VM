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

using System;
using System.Text;

namespace Neo.Core
{
    /// <summary>
    /// Shared helpers for encoding and Unix time conversion.
    /// </summary>
    public static class CoreUtilities
    {
        /// <summary>
        /// Gets a UTF-8 encoding that throws on invalid sequences.
        /// </summary>
        public static Encoding StrictUtf8Encoding => Encoding.GetEncoding("utf-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

        /// <summary>
        /// Converts a <see cref="DateTime"/> to Unix time in milliseconds.
        /// </summary>
        /// <param name="dateTime">The date and time to convert.</param>
        /// <param name="offset">The UTC offset of <paramref name="dateTime"/>.</param>
        /// <returns>The Unix timestamp in milliseconds.</returns>
        public static long ToUnixTimeMilliseconds(DateTime dateTime, TimeSpan offset = default) =>
            new DateTimeOffset(dateTime, offset)
                .ToUnixTimeMilliseconds();

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> duration from the Unix epoch to milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span from the Unix epoch.</param>
        /// <param name="offset">The UTC offset applied to the conversion.</param>
        /// <returns>The Unix timestamp in milliseconds.</returns>
        public static long ToUnixTimeMilliseconds(TimeSpan timeSpan, TimeSpan offset = default) =>
            new DateTimeOffset(timeSpan.Ticks, offset)
                .ToUnixTimeMilliseconds();

        /// <summary>
        /// Converts a <see cref="DateTime"/> to Unix time in seconds.
        /// </summary>
        /// <param name="dateTime">The date and time to convert.</param>
        /// <param name="offset">The UTC offset of <paramref name="dateTime"/>.</param>
        /// <returns>The Unix timestamp in seconds.</returns>
        public static uint ToUnixTimeSeconds(DateTime dateTime, TimeSpan offset = default) =>
            (uint)new DateTimeOffset(dateTime, offset)
                .ToUnixTimeSeconds();

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> duration from the Unix epoch to seconds.
        /// </summary>
        /// <param name="timeSpan">The time span from the Unix epoch.</param>
        /// <param name="offset">The UTC offset applied to the conversion.</param>
        /// <returns>The Unix timestamp in seconds.</returns>
        public static uint ToUnixTimeSeconds(TimeSpan timeSpan, TimeSpan offset = default) =>
            (uint)new DateTimeOffset(timeSpan.Ticks, offset)
                .ToUnixTimeSeconds();

        /// <summary>
        /// Converts a Unix timestamp in milliseconds to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="milliseconds">The Unix timestamp in milliseconds.</param>
        /// <param name="isLocalDateTime">
        /// <see langword="true"/> to return local time; otherwise UTC.
        /// </param>
        /// <returns>The converted date and time.</returns>
        public static DateTime FromUnixTimeMilliseconds(ulong milliseconds, bool isLocalDateTime = false) =>
            FromUnixTimeMilliseconds((long)milliseconds, isLocalDateTime);

        /// <summary>
        /// Converts a Unix timestamp in seconds to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="seconds">The Unix timestamp in seconds.</param>
        /// <param name="isLocalDateTime">
        /// <see langword="true"/> to return local time; otherwise UTC.
        /// </param>
        /// <returns>The converted date and time.</returns>
        public static DateTime FromUnixTimeSeconds(uint seconds, bool isLocalDateTime = false) =>
            FromUnixTimeMilliseconds(seconds * TimeSpan.MillisecondsPerSecond, isLocalDateTime);

        /// <summary>
        /// Converts a Unix timestamp in milliseconds to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="milliseconds">The Unix timestamp in milliseconds.</param>
        /// <param name="isLocalDateTime">
        /// <see langword="true"/> to return local time; otherwise UTC.
        /// </param>
        /// <returns>The converted date and time.</returns>
        public static DateTime FromUnixTimeMilliseconds(long milliseconds, bool isLocalDateTime = false)
        {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            return isLocalDateTime ? dateTimeOffset.LocalDateTime : dateTimeOffset.UtcDateTime;
        }
    }
}
