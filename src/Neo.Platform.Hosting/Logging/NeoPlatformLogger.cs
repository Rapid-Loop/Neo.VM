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
using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Hosting.Logging
{
    /// <summary>
    /// Console logger implementation used by the Neo platform host.
    /// </summary>
    internal sealed class NeoPlatformLogger(
        string name,
        Func<NeoPlatformLoggerOptions> config) : ILogger
    {
        private DateTime DateTimeNow => _getConfig().UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        private bool ShowExceptionStackTrace => _getConfig().ShowExceptionStackTrace;

        private readonly string _name = name;
        private readonly Func<NeoPlatformLoggerOptions> _getConfig = config;

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            default!;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel) == false)
                return;

            ArgumentNullException.ThrowIfNull(formatter);

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            message = $"{_name}[{eventId.Name ?? $"{eventId.Id:d}"}] {message}";

            switch (logLevel)
            {
                case LogLevel.Trace:
                    TraceMessage("{0}", message);
                    break;
                case LogLevel.Debug:
                    DebugMessage("{0}", message);
                    break;
                case LogLevel.Information:
                    InfoMessage("{0}", message);
                    break;
                case LogLevel.Warning:
                    WarningMessage("{0}", message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    ErrorMessage("{0}", message);
                    if (exception is not null)
                        ErrorMessage(exception, ShowExceptionStackTrace);
                    break;
                default:
                    break;
            }
        }

        private void WriteDateTime(bool isError = false)
        {
            var format = $"[{BuildFormatString(_getConfig().TimestampFormat)}] ";
            if (isError)
                ErrorWrite(format, DateTimeNow);
            else
                Write(format, DateTimeNow);
        }

        /// <summary>
        /// Writes a formatted message to standard output without a trailing newline.
        /// </summary>
        public static void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            var message = string.Format(format, args);

            Console.Out.Write(message);
        }

        /// <summary>
        /// Writes a formatted message to standard error without a trailing newline.
        /// </summary>
        public static void ErrorWrite([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            var message = string.Format(format, args);

            Console.Error.Write(message);
        }

        /// <summary>
        /// Writes a newline to standard output.
        /// </summary>
        public static void WriteLine() =>
            Write(Environment.NewLine);

        /// <summary>
        /// Writes a formatted message to standard output followed by a newline.
        /// </summary>
        public static void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
            Write(string.Format(format, args) + Environment.NewLine);

        /// <summary>
        /// Writes a newline to standard error.
        /// </summary>
        public static void ErrorWriteLine() =>
            ErrorWrite(Environment.NewLine);

        /// <summary>
        /// Writes a formatted message to standard error followed by a newline.
        /// </summary>
        public static void ErrorWriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
            ErrorWrite(string.Format(format, args) + Environment.NewLine);

        /// <summary>
        /// Writes an information-level message with platform coloring.
        /// </summary>
        public void InfoMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            WriteDateTime();
            SetTerminalForegroundColor(ConsoleColor.Blue);
            Write($"Info: ");
            SetTerminalForegroundColor(ConsoleColor.White);
            WriteLine(format, args);
            ResetColor();
        }

        /// <summary>
        /// Writes a warning-level message with platform coloring.
        /// </summary>
        public void WarningMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            WriteDateTime();
            SetTerminalForegroundColor(ConsoleColor.Yellow);
            Write("Warn: ");
            SetTerminalForegroundColor(ConsoleColor.White);
            WriteLine(format, args);
            ResetColor();
        }

        /// <summary>
        /// Writes a debug-level message with platform coloring.
        /// </summary>
        public void DebugMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            SetTerminalForegroundColor(ConsoleColor.DarkGray);
            WriteDateTime();
            Write("Debug: ");
            WriteLine(format, args);
            ResetColor();
        }

        /// <summary>
        /// Writes a trace-level message with platform coloring.
        /// </summary>
        public void TraceMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            SetTerminalForegroundColor(ConsoleColor.DarkGray);
            WriteDateTime();
            Write("Trace: ");
            WriteLine(format, args);
            ResetColor();
        }

        /// <summary>
        /// Writes exception details to standard error, optionally including a stack trace.
        /// </summary>
        /// <param name="exception">Exception to report.</param>
        /// <param name="showStackTrace">When <see langword="true"/>, includes stack trace output.</param>
        public void ErrorMessage(Exception exception, bool showStackTrace = true)
        {
            var stackTrace = exception.InnerException?.StackTrace ?? exception.StackTrace;

            SetTerminalForegroundColor(ConsoleColor.Red);
            WriteDateTime(isError: true);
            SetTerminalForegroundColor(ConsoleColor.DarkRed);
            ErrorWriteLine("{0}: ", exception.InnerException?.GetType().Name ?? exception.GetType().Name);
            SetTerminalForegroundColor(ConsoleColor.Red);
            ErrorWriteLine("   {0}", exception.InnerException?.Message ?? exception.Message);

            if (showStackTrace)
            {
                ErrorWriteLine("Stack Trace: ");
                SetTerminalForegroundColor(ConsoleColor.DarkRed);
                ErrorWriteLine("   {0}", stackTrace?.Trim() ?? string.Empty);
            }

            ResetColor();
        }

        /// <summary>
        /// Writes an error-level formatted message to standard error.
        /// </summary>
        public void ErrorMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            SetTerminalForegroundColor(ConsoleColor.Red);
            WriteDateTime(isError: true);
            SetTerminalForegroundColor(ConsoleColor.DarkRed);
            ErrorWrite("Error: ");
            SetTerminalForegroundColor(ConsoleColor.Red);
            ErrorWriteLine(format, args);
            ResetColor();
        }

        /// <summary>
        /// Sets the console foreground color.
        /// </summary>
        public static void SetTerminalForegroundColor(ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
        }

        /// <summary>
        /// Sets the console background color.
        /// </summary>
        public static void SetTerminalBackgroundColor(ConsoleColor consoleColor)
        {
            Console.BackgroundColor = consoleColor;
        }

        /// <summary>
        /// Resets console colors to the default.
        /// </summary>
        public static void ResetColor()
        {
            Console.ResetColor();
        }

        private static string BuildFormatString(string format, int index = 0) =>
            $"{{{index}:{format}}}";
    }
}
