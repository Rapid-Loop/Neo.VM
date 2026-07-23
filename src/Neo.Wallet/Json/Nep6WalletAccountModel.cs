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
using Neo.Core.Interfaces;
using System.Text.Json.Serialization;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// JSON model for a <see cref="Nep6WalletAccount"/>, storing the NEP-2 key material.
    /// </summary>
    public class Nep6WalletAccountModel : WalletAccountModel<ProtocolSettingsOptions>, IMap<Nep6WalletAccount>
    {
        /// <summary>
        /// Gets or sets the NEP-2 key material as a UTF-8 byte array.
        /// </summary>
        [JsonConverter(typeof(JsonStringByteArrayConverter))]
        public override byte[]? Key { get => base.Key; set => base.Key = value; }

        /// <summary>
        /// Moves property values from <see cref="Nep6WalletAccountModel"/> to <see cref="Nep6WalletAccount"/> with <see cref="SCryptModel.Default"/> setting.
        /// </summary>
        /// <returns>A new <see cref="Nep6WalletAccount"/> object.</returns>
        public Nep6WalletAccount ToObject() =>
            new(this);
    }
}
