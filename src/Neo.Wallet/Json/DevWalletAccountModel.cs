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

using Neo.Configuration;
using Neo.Configuration.Json.Converters;
using Neo.Core;
using Neo.Core.Interfaces;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// JSON model for a <see cref="DevWalletAccount"/>, storing the private key as hex.
    /// </summary>
    public class DevWalletAccountModel : WalletAccountModel<ProtocolSettingsOptions>, IMap<DevWalletAccount>
    {
        /// <summary>
        /// Gets or sets the private key bytes, serialized as a hex string.
        /// </summary>
        [JsonConverter(typeof(JsonStringHexFormatConverter))]
        public override byte[]? Key { get => base.Key; set => base.Key = value; }

        // TODO: Add support for MultiSigAddresses
        /// <summary>
        /// Converts this model to a <see cref="DevWalletAccount"/>.
        /// </summary>
        /// <returns>
        /// A signature account when <see cref="Key"/> is present; otherwise a contract or watch-only account.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when creating a keyless account without an address.</exception>
        public DevWalletAccount ToObject() =>
            (Key is not null && Key.Length > 0)
            ? new(
                Key ?? [],
                Extra?.ToObject() ?? ProtocolSettings.Default)
            {
                Label = Label,
                IsDefault = IsDefault,
                IsLocked = Lock,
            }
            : new(
                Address ?? throw new InvalidOperationException(),
                Extra?.ToObject() ?? ProtocolSettings.Default,
                [.. Contract?.Parameters?.Select(s => s.ToObject()) ?? []],
                Key)
            {
                Label = Label,
                IsDefault = IsDefault,
                IsLocked = Lock,
            };
    }
}
