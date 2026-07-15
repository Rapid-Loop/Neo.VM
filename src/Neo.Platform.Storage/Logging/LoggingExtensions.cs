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
using System;

namespace Neo.Platform.Storage.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(
            EventId = StoreEventId.Fault,
            EventName = nameof(StoreEventId.Fault),
            Message = "{Message}"
        )]
        public static partial void LogFaultMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        [LoggerMessage(
            EventId = StoreEventId.Fault,
            Message = ""
        )]
        public static partial void LogFaultMessage(
            this ILogger logger,
            LogLevel logLevel,
            Exception exception
        );

        [LoggerMessage(
            EventId = StoreEventId.Read,
            EventName = nameof(StoreEventId.Read),
            Message = "{Message}"
        )]
        public static partial void LogReadMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        [LoggerMessage(
            EventId = StoreEventId.Write,
            EventName = nameof(StoreEventId.Write),
            Message = "{Message}"
        )]
        public static partial void LogWriteMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        [LoggerMessage(
            EventId = StoreEventId.Delete,
            EventName = nameof(StoreEventId.Delete),
            Message = "{Message}"
        )]
        public static partial void LogDeleteMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        [LoggerMessage(
            EventId = StoreEventId.CreateSnapshot,
            EventName = nameof(StoreEventId.CreateSnapshot),
            Message = "{Message}"
        )]
        public static partial void LogCreateSnapshotMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );

        [LoggerMessage(
            EventId = StoreEventId.Commit,
            EventName = nameof(StoreEventId.Commit),
            Message = "{Message}"
        )]
        public static partial void LogCommitMessage(
            this ILogger logger,
            LogLevel logLevel,
            string message
        );
    }
}
