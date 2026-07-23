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
using System.Text.Json.Serialization;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// Base JSON model for a wallet file.
    /// </summary>
    /// <typeparam name="TExtras">The type of wallet-level extra data.</typeparam>
    /// <typeparam name="TAccountModel">The type of account model entries.</typeparam>
    public class WalletModel<TExtras, TAccountModel> : JsonModel
        where TExtras : class?, new()
        where TAccountModel : class?, new()
    {
        /// <summary>
        /// Gets or sets the optional display name of the wallet.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the wallet format version.
        /// </summary>
        public Version? Version { get; set; }

        /// <summary>
        /// Gets or sets the SCrypt parameters used for key derivation.
        /// </summary>
        [JsonPropertyName("scrypt")]
        public SCryptModel? SCrypt { get; set; }

        /// <summary>
        /// Gets or sets the accounts contained in the wallet.
        /// </summary>
        public TAccountModel[]? Accounts { get; set; }

        /// <summary>
        /// Gets or sets optional wallet-level extra data.
        /// </summary>
        public TExtras? Extra { get; set; }
    }
}
