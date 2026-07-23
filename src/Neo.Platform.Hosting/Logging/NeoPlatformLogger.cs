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
    internal sealed class NeoPlatformLogger(
        string name,
        Func<NeoPlatformLoggerOptions> config) : ILogger
    {
        private DateTime DateTimeNow => _getConfig().UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        private bool ShowExceptionStackTrace => _getConfig().ShowExceptionStackTrace;

        private readonly string _name = name;
        private readonly Func<NeoPlatformLoggerOptions> _getConfig = config;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            default!;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None;

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

        public static void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            var message = string.Format(format, args);

            Console.Out.Write(message);
        }
        public static void ErrorWrite([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            var message = string.Format(format, args);

            Console.Error.Write(message);
        }

        public static void WriteLine() =>
            Write(Environment.NewLine);

        public static void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
            Write(string.Format(format, args) + Environment.NewLine);

        public static void ErrorWriteLine() =>
            ErrorWrite(Environment.NewLine);

        public static void ErrorWriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
            ErrorWrite(string.Format(format, args) + Environment.NewLine);

        public void InfoMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            WriteDateTime();
            SetTerminalForegroundColor(ConsoleColor.Blue);
            Write($"Info: ");
            SetTerminalForegroundColor(ConsoleColor.White);
            WriteLine(format, args);
            ResetColor();
        }

        public void WarningMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            WriteDateTime();
            SetTerminalForegroundColor(ConsoleColor.Yellow);
            Write("Warn: ");
            SetTerminalForegroundColor(ConsoleColor.White);
            WriteLine(format, args);
            ResetColor();
        }

        public void DebugMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            SetTerminalForegroundColor(ConsoleColor.DarkGray);
            WriteDateTime();
            Write("Debug: ");
            WriteLine(format, args);
            ResetColor();
        }

        public void TraceMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            SetTerminalForegroundColor(ConsoleColor.DarkGray);
            WriteDateTime();
            Write("Trace: ");
            WriteLine(format, args);
            ResetColor();
        }

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

        public static void SetTerminalForegroundColor(ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
        }

        public static void SetTerminalBackgroundColor(ConsoleColor consoleColor)
        {
            Console.BackgroundColor = consoleColor;
        }

        public static void ResetColor()
        {
            Console.ResetColor();
        }

        private static string BuildFormatString(string format, int index = 0) =>
            $"{{{index}:{format}}}";
    }
}
