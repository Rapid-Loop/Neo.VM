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
using Neo.Configuration.Json.Converters;
using Neo.Core;
using System.Text.Json.Serialization;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// Base JSON model for a wallet account.
    /// </summary>
    /// <typeparam name="TExtra">The type of account-level extra data.</typeparam>
    public class WalletAccountModel<TExtra> : JsonModel
        where TExtra : class?, new()
    {
        /// <summary>
        /// Gets or sets the account address as a script hash.
        /// </summary>
        [JsonConverter(typeof(JsonStringAddressConverter))]
        public UInt160? Address { get; set; }

        /// <summary>
        /// Gets or sets an optional human-readable label for the account.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default account.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account is locked.
        /// </summary>
        public bool Lock { get; set; }

        /// <summary>
        /// Gets or sets the account key material (format depends on the concrete account model).
        /// </summary>
        public virtual byte[]? Key { get; set; }

        /// <summary>
        /// Gets or sets the witness contract associated with the account.
        /// </summary>
        public ContractModel? Contract { get; set; }

        /// <summary>
        /// Gets or sets optional account-level extra data.
        /// </summary>
        public TExtra? Extra { get; set; }
    }
}
