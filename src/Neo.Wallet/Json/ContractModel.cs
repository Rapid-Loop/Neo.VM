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
using Neo.Core.Blockchain;
using Neo.Core.Interfaces;
using System.Linq;
using System.Text.Json.Serialization;

namespace Neo.Wallet.Json
{
    /// <summary>
    /// JSON model for a wallet account witness contract.
    /// </summary>
    public class ContractModel : JsonModel, IMap<WitnessContract>
    {
        /// <summary>
        /// Gets or sets the contract script bytes, serialized as a hex string.
        /// </summary>
        [JsonConverter(typeof(JsonStringHexFormatConverter))]
        public byte[]? Script { get; set; }

        /// <summary>
        /// Gets or sets the contract parameter definitions.
        /// </summary>
        public ContractParameterModel[]? Parameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the contract is considered deployed.
        /// </summary>
        public bool Deployed { get; set; }

        /// <summary>
        /// Converts this model to a <see cref="WitnessContract"/>.
        /// </summary>
        /// <returns>A witness contract built from <see cref="Script"/> and <see cref="Parameters"/>.</returns>
        public WitnessContract ToObject() =>
            WitnessContract.Create(
                Script ?? [],
                [
                    .. Parameters?.Select(
                        static s =>
                            s.ToObject()) ?? [],
                ]
            );
    }
}
