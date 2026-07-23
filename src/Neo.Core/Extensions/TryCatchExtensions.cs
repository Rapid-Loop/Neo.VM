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

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides fluent try/catch helpers for chaining operations with optional error handling.
    /// </summary>
    public static class TryCatchExtensions
    {
        /// <summary>
        /// Invokes a function and returns its result, or the default value when any exception is thrown.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The receiver passed to <paramref name="func"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The function result, or <see langword="default"/> if an exception is thrown.</returns>
        public static TResult? TryCatch<TSource, TResult>(this TSource obj, Func<TSource?, TResult?> func)
        {
            try
            {
                return func(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Invokes an action and optionally handles a specific exception type, then returns the receiver.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type to catch.</typeparam>
        /// <param name="obj">The receiver passed to the action.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">An optional handler invoked when <typeparamref name="TException"/> is thrown.</param>
        /// <returns>The original receiver.</returns>
        public static TSource TryCatch<TSource, TException>(this TSource obj, Action<TSource> action, Action<TSource?, TException>? onError = default)
            where TException : Exception
        {
            try
            {
                action(obj);
            }
            catch (TException ex)
            {
                onError?.Invoke(obj, ex);
            }

            return obj;
        }

        /// <summary>
        /// Invokes a function and optionally maps a specific exception type to a result.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type to catch.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The receiver passed to the function.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="onError">
        /// An optional handler that produces a result when <typeparamref name="TException"/> is thrown.
        /// When omitted, the exception is rethrown.
        /// </param>
        /// <returns>The function result, or the value produced by <paramref name="onError"/>.</returns>
        public static TResult TryCatch<TSource, TException, TResult>(this TSource obj, Func<TSource, TResult> func, Func<TSource, TException, TResult>? onError = default)
            where TException : Exception
        {
            try
            {
                return func(obj);
            }
            catch (TException ex)
            {
                if (onError == null) throw;
                return onError(obj, ex);
            }
        }

        /// <summary>
        /// Invokes an action and rethrows exceptions of type <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type that is rethrown.</typeparam>
        /// <param name="obj">The receiver passed to the action.</param>
        /// <param name="action">The action to invoke.</param>
        /// <returns>The original receiver when the action completes successfully.</returns>
        public static TSource TryCatchThrow<TSource, TException>(this TSource obj, Action<TSource?> action)
            where TException : Exception
        {
            try
            {
                action(obj);

                return obj;
            }
            catch (TException)
            {
                throw;
            }
        }

        /// <summary>
        /// Invokes a function and rethrows exceptions of type <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type that is rethrown.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The receiver passed to the function.</param>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The function result when it completes successfully.</returns>
        public static TResult TryCatchThrow<TSource, TException, TResult>(this TSource obj, Func<TSource, TResult> func)
            where TException : Exception
        {
            try
            {
                return func(obj);
            }
            catch (TException)
            {
                throw;
            }
        }

        /// <summary>
        /// Invokes an action and, on failure, optionally wraps <typeparamref name="TException"/> with a new message.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type to catch and optionally wrap.</typeparam>
        /// <param name="obj">The receiver passed to the action.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="errorMessage">
        /// When provided, a new <typeparamref name="TException"/> is thrown with this message and the original exception as the inner exception.
        /// </param>
        /// <returns>The original receiver when the action completes successfully.</returns>
        public static TSource TryCatchThrow<TSource, TException>(this TSource obj, Action<TSource?> action, string? errorMessage = default)
            where TException : Exception, new()
        {
            try
            {
                action(obj);

                return obj;
            }
            catch (TException innerException)
            {
                if (string.IsNullOrEmpty(errorMessage))
                    throw;
                else
                {
                    if (Activator.CreateInstance(typeof(TException), errorMessage, innerException) is not TException ex)
                        throw;
                    else
                        throw ex;
                }

            }
        }

        /// <summary>
        /// Invokes a function and, on failure, optionally wraps <typeparamref name="TException"/> with a new message.
        /// </summary>
        /// <typeparam name="TSource">The type of the receiver.</typeparam>
        /// <typeparam name="TException">The exception type to catch and optionally wrap.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The receiver passed to the function.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="errorMessage">
        /// When provided, a new <typeparamref name="TException"/> is thrown with this message and the original exception as the inner exception.
        /// </param>
        /// <returns>The function result when it completes successfully.</returns>
        public static TResult? TryCatchThrow<TSource, TException, TResult>(this TSource obj, Func<TSource?, TResult?> func, string? errorMessage = default)
            where TException : Exception
        {
            try
            {
                return func(obj);
            }
            catch (TException innerException)
            {
                if (string.IsNullOrEmpty(errorMessage))
                    throw;
                else
                {
                    if (Activator.CreateInstance(typeof(TException), errorMessage, innerException) is not TException ex)
                        throw;
                    else
                        throw ex;
                }

            }
        }
    }
}
