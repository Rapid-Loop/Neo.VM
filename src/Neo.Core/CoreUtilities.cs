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
    public static class CoreUtilities
    {
        public static Encoding StrictUtf8Encoding => Encoding.GetEncoding("utf-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

        public static long ToUnixTimeMilliseconds(DateTime dateTime, TimeSpan offset = default) =>
            new DateTimeOffset(dateTime, offset)
                .ToUnixTimeMilliseconds();

        public static long ToUnixTimeMilliseconds(TimeSpan timeSpan, TimeSpan offset = default) =>
            new DateTimeOffset(timeSpan.Ticks, offset)
                .ToUnixTimeMilliseconds();

        public static DateTime FromUnixTimeMilliseconds(ulong milliseconds, bool isLocalDateTime = false) =>
            FromUnixTimeMilliseconds((long)milliseconds, isLocalDateTime);

        public static DateTime FromUnixTimeMilliseconds(long milliseconds, bool isLocalDateTime = false)
        {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            return isLocalDateTime ? dateTimeOffset.LocalDateTime : dateTimeOffset.UtcDateTime;
        }
    }
}
