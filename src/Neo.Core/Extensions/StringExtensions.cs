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

using Neo.Core.Text;
using System;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/> and character spans.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Trims the specified prefix from the start of the <see cref="string"/>, ignoring case.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to trim.</param>
        /// <param name="prefix">The prefix to trim.</param>
        /// <returns>
        /// The trimmed ReadOnlySpan without prefix. If no prefix is found, the input is returned unmodified.
        /// </returns>
        public static ReadOnlySpan<char> TrimStartIgnoreCase(this ReadOnlySpan<char> value, ReadOnlySpan<char> prefix)
        {
            if (value.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                return value[prefix.Length..];
            return value;
        }

        /// <summary>
        /// Pads a string on both sides so that it is centered within the specified total width.
        /// </summary>
        /// <param name="str">The string to pad; when <see langword="null"/>, an empty string is returned.</param>
        /// <param name="totalWidth">The total width of the resulting string.</param>
        /// <param name="paddingChar">The character used for padding.</param>
        /// <returns>The centered string, or the original string when it is already at least <paramref name="totalWidth"/> characters.</returns>
        public static string PadCenter(this string str, int totalWidth, char paddingChar)
        {
            if (str is null) return string.Empty;

            if (str.Length >= totalWidth)
                return str;

            var padLeft = (totalWidth - str.Length) / 2 + str.Length;
            return str.PadLeft(padLeft, paddingChar).PadRight(totalWidth, paddingChar);
        }

        /// <summary>
        /// Converts the specified address to a script hash.
        /// </summary>
        /// <param name="address">The address to convert.</param>
        /// <param name="version">The address version.</param>
        /// <returns>The converted script hash.</returns>
        /// <exception cref="FormatException">
        /// Thrown when the address is not a valid Base58Check address for the specified version.
        /// </exception>
        public static UInt160 ToScriptHash(this string address, byte version)
        {
            var data = Base58.DecodeCheck(address);

            if (data.Length != 21)
                throw new FormatException($"Invalid address format: expected 21 bytes after Base58Check decoding, but got {data.Length} bytes. The address may be corrupted or in an invalid format.");

            if (data[0] != version)
                throw new FormatException($"Invalid address version: expected version {version}, but got {data[0]}. The address may be for a different network.");

            return new(data.AsSpan(1));
        }

        /// <summary>
        /// Gets the serialized size of a UTF-8 string, including its compact-size length prefix.
        /// </summary>
        /// <param name="source">The string to measure.</param>
        /// <returns>The number of bytes required to serialize the string.</returns>
        public static int GetSerializedSize(this string source)
        {
            var byteCount = CoreUtilities.StrictUtf8Encoding.GetByteCount(source);
            return byteCount.GetCompactSize() + byteCount;
        }
    }
}
