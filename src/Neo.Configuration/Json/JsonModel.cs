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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Neo.Configuration.Json
{
    /// <summary>
    /// Base type for configuration models that can be serialized to and deserialized from JSON.
    /// </summary>
    public abstract class JsonModel
    {
        protected JsonSerializerOptions _jsonSerializerOptions = JsonDefaults.SerializerOptions;

        /// <summary>
        /// Serializes this model to a JSON string using the specified options, or the instance defaults when omitted.
        /// </summary>
        /// <param name="options">Optional serializer options; when <see langword="null"/>, instance defaults are used.</param>
        /// <returns>The JSON representation of this model.</returns>
        [return: NotNull]
        public virtual string? ToString(JsonSerializerOptions? options = default) =>
            ToJson(options);

        /// <summary>
        /// Serializes this model to a JSON string.
        /// </summary>
        /// <param name="options">Optional serializer options; when <see langword="null"/>, instance defaults are used.</param>
        /// <returns>The JSON representation of this model.</returns>
        public virtual string ToJson(JsonSerializerOptions? options) =>
            JsonSerializer.Serialize<object>(this, options ?? _jsonSerializerOptions);

        /// <summary>
        /// Deserializes a JSON string into a <typeparamref name="TModel"/> instance.
        /// </summary>
        /// <typeparam name="TModel">The model type to deserialize.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <param name="options">Optional serializer options; when <see langword="null"/>, <see cref="JsonDefaults.SerializerOptions"/> is used.</param>
        /// <returns>The deserialized model, or <see langword="null"/> if deserialization yields no value.</returns>
        public static TModel? FromJson<TModel>(string jsonString, JsonSerializerOptions? options = default)
            where TModel : notnull, JsonModel =>
            JsonSerializer.Deserialize<TModel>(
                jsonString,
                options ?? JsonDefaults.SerializerOptions);

        /// <summary>
        /// Deserializes the contents of a JSON file into a <typeparamref name="TModel"/> instance.
        /// </summary>
        /// <typeparam name="TModel">The model type to deserialize.</typeparam>
        /// <param name="file">The JSON file to read.</param>
        /// <param name="options">Optional serializer options; when <see langword="null"/>, <see cref="JsonDefaults.SerializerOptions"/> is used.</param>
        /// <returns>The deserialized model, or <see langword="null"/> if deserialization yields no value.</returns>
        /// <exception cref="FileNotFoundException">Thrown when <paramref name="file"/> does not exist.</exception>
        /// <exception cref="FormatException">Thrown when the file is empty.</exception>
        public static TModel? FromJson<TModel>(FileInfo file, JsonSerializerOptions? options = default)
            where TModel : notnull, JsonModel
        {
            if (file.Exists == false)
                throw new FileNotFoundException($"{file}");

            var jsonOptions = options ?? JsonDefaults.SerializerOptions;
            var jsonString = File.ReadAllText(file.FullName);

            if (string.IsNullOrEmpty(jsonString))
                throw new FormatException($"\'{file}\' is empty.");

            return FromJson<TModel>(jsonString, jsonOptions);
        }
    }
}
