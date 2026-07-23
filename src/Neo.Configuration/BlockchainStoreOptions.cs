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

using Neo.Configuration.Json;
using System;
using System.IO;

namespace Neo.Configuration
{
    /// <summary>
    /// Configuration options for the blockchain store database.
    /// </summary>
    public class BlockchainStoreOptions : JsonModel
    {
        /// <summary>
        /// The directory path of the blockchain database.
        /// Defaults to <c>data/chain</c> under the application base directory.
        /// </summary>
        public string DatabasePath { get; init; } = Path.Combine(AppContext.BaseDirectory, "data", "chain");

        /// <summary>
        /// Whether to create the database if it does not already exist. Defaults to <see langword="true"/>.
        /// </summary>
        public bool CreateIfMissing { get; init; } = true;
    }
}
