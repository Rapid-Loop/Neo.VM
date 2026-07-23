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

using Neo.Configuration.Json.Converters;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neo.Configuration.Json
{
    /// <summary>
    /// Provides default <see cref="JsonSerializerOptions"/> used by configuration models.
    /// </summary>
    public static class JsonDefaults
    {
        /// <summary>
        /// The default serializer options for Neo configuration JSON, including camel-case naming
        /// and converters for enums, <see cref="Neo.Core.Cryptography.ECC.ECPoint"/>, <see cref="Neo.Core.UInt160"/>, and <see cref="System.Net.IPEndPoint"/>.
        /// </summary>
        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            WriteIndented = true,
            RespectNullableAnnotations = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                // TODO: Add more defaults for NEP-6
                // NOTE: JsonConverterAttribute overrides these converters
                new JsonStringEnumConverter(),
                new JsonStringECPointConverter(),
                new JsonStringUInt160Converter(),
                new JsonIPEndPointConverter(),
            },
        };
    }
}
