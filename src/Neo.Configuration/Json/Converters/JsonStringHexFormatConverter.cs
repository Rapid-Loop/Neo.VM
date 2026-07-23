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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neo.Configuration.Json.Converters
{
    /// <summary>
    /// Converts <see cref="T:byte[]"/> values to and from lowercase hexadecimal JSON strings.
    /// </summary>
    public class JsonStringHexFormatConverter : JsonConverter<byte[]?>
    {
        /// <summary>
        /// Reads a byte array from a hexadecimal JSON string.
        /// </summary>
        /// <param name="reader">The reader positioned at the JSON value.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The decoded bytes, or <see langword="null"/> when the string is empty.</returns>
        /// <exception cref="FormatException">Thrown when the token is not a JSON string.</exception>
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new FormatException();

            var valueString = reader.GetString();

            if (string.IsNullOrEmpty(valueString))
                return default;

            return Convert.FromHexString(valueString);
        }

        /// <summary>
        /// Writes a byte array as a lowercase hexadecimal JSON string, or JSON null when the value is <see langword="null"/>.
        /// </summary>
        /// <param name="writer">The writer to which the value is written.</param>
        /// <param name="value">The bytes to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(Convert.ToHexStringLower(value));
        }
    }
}
