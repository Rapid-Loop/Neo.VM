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
using Neo.Core;
using Neo.Core.Cryptography.ECC;
using Neo.Core.Interfaces;
using Neo.Core.VM;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace Neo.Configuration
{
    /// <summary>
    /// JSON-bindable configuration options that map to <see cref="ProtocolSettings"/>.
    /// </summary>
    public class ProtocolSettingsOptions : JsonModel, IMap<ProtocolSettings>
    {
        /// <summary>
        /// The magic number of the NEO network.
        /// </summary>
        public uint Network { get; set; }

        /// <summary>
        /// The address version of the NEO system.
        /// </summary>
        public byte AddressVersion { get; set; }

        /// <summary>
        /// The time in milliseconds between two blocks.
        /// </summary>
        public uint MillisecondsPerBlock { get; set; }

        /// <summary>
        /// The maximum number of transactions that can be contained in a block.
        /// </summary>
        public uint MaxTransactionsPerBlock { get; set; }

        /// <summary>
        /// The maximum number of transactions that can be contained in the memory pool.
        /// </summary>
        public int MemoryPoolMaxTransactions { get; set; }

        /// <summary>
        /// The maximum number of blocks that can be traced in a smart contract.
        /// </summary>
        public uint MaxTraceableBlocks { get; set; }

        /// <summary>
        /// The amount of GAS to distribute during initialization, in datoshi (1 GAS = 1e8 datoshi).
        /// </summary>
        public ulong InitialGasDistribution { get; set; }

        /// <summary>
        /// The number of validators in the NEO system.
        /// </summary>
        public int ValidatorsCount { get; set; }

        /// <summary>
        /// The default seed node endpoints, or <see langword="null"/> if none are configured.
        /// </summary>
        public IPEndPoint[]? SeedList { get; set; }

        /// <summary>
        /// The block heights from which hard forks are activated, or <see langword="null"/> if none are configured.
        /// </summary>
        public IDictionary<HardFork, uint>? HardForks { get; set; }

        /// <summary>
        /// The public keys of the standby committee members, or <see langword="null"/> if none are configured.
        /// </summary>
        public ECPoint[]? StandbyCommittee { get; set; }

        /// <summary>
        /// Converts these options into a <see cref="ProtocolSettings"/> instance.
        /// Nullable collections are replaced with empty defaults.
        /// </summary>
        /// <returns>A new <see cref="ProtocolSettings"/> populated from this instance.</returns>
        public ProtocolSettings ToObject() =>
            new()
            {
                Network = Network,
                AddressVersion = AddressVersion,
                MillisecondsPerBlock = MillisecondsPerBlock,
                MaxTransactionsPerBlock = MaxTransactionsPerBlock,
                MemoryPoolMaxTransactions = MemoryPoolMaxTransactions,
                MaxTraceableBlocks = MaxTraceableBlocks,
                InitialGasDistribution = InitialGasDistribution,
                ValidatorsCount = ValidatorsCount,
                SeedList = SeedList ?? [],
                HardForks = HardForks?.ToImmutableDictionary() ?? [],
                StandbyCommittee = StandbyCommittee?.ToList() ?? [],
            };
    }
}
