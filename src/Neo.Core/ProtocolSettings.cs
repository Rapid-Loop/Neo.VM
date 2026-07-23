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

using Neo.Core.Cryptography.ECC;
using Neo.Core.VM;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace Neo.Core
{
    /// <summary>
    /// Network and chain protocol parameters for a NEO network.
    /// </summary>
    public class ProtocolSettings
    {
        /// <summary>
        /// The magic number of the NEO network.
        /// </summary>
        public uint Network { get; init; }

        /// <summary>
        /// The address version of the NEO system.
        /// </summary>
        public byte AddressVersion { get; init; }

        /// <summary>
        /// The public keys of the standby committee members.
        /// </summary>
        public IReadOnlyList<ECPoint> StandbyCommittee { get; init; } = [];

        /// <summary>
        /// The number of members of the committee in NEO system.
        /// </summary>
        public int CommitteeMembersCount => StandbyCommittee.Count;

        /// <summary>
        /// The number of the validators in NEO system.
        /// </summary>
        public int ValidatorsCount { get; init; }

        /// <summary>
        /// The default seed nodes list.
        /// </summary>
        public IPEndPoint[] SeedList { get; init; } = [];

        /// <summary>
        /// Indicates the time in milliseconds between two blocks. Note that starting from
        /// HF_Echidna block generation time is managed by native Policy contract, hence
        /// use NeoSystemExtensions.GetTimePerBlock extension method instead of direct access
        /// to this property.
        /// </summary>
        public uint MillisecondsPerBlock { get; init; }

        /// <summary>
        /// The maximum increment of the transaction valid until block field.
        /// </summary>
        public uint MaxValidUntilBlockIncrement { get; init; }

        /// <summary>
        /// Indicates the maximum number of transactions that can be contained in a block.
        /// </summary>
        public uint MaxTransactionsPerBlock { get; init; }

        /// <summary>
        /// Indicates the maximum number of transactions that can be contained in the memory pool.
        /// </summary>
        public int MemoryPoolMaxTransactions { get; init; }

        /// <summary>
        /// Indicates the maximum number of blocks that can be traced in the smart contract. Note
        /// that starting from HF_Echidna the maximum number of traceable blocks is managed by
        /// native Policy contract method instead of direct access to this property.
        /// </summary>
        public uint MaxTraceableBlocks { get; init; }

        /// <summary>
        /// Sets the block height from which a hard fork is activated.
        /// </summary>
        public IReadOnlyDictionary<HardFork, uint> HardForks { get; init; } = Enum.GetValues<HardFork>().ToImmutableDictionary(k => k, v => 0u);

        /// <summary>
        /// Indicates the amount of gas to distribute during initialization.
        /// In the unit of datoshi, 1 GAS = 1e8 datoshi
        /// </summary>
        public ulong InitialGasDistribution { get; init; }

        /// <summary>
        /// The public keys of the standby validators.
        /// </summary>
        public IReadOnlyList<ECPoint> StandbyValidators => [.. StandbyCommittee.Take(ValidatorsCount)];

        /// <summary>
        /// The default protocol settings for NEO MainNet.
        /// </summary>
        public static ProtocolSettings Default { get; } = new()
        {
            Network = 0u,
            AddressVersion = 53,
            StandbyCommittee = [],
            ValidatorsCount = 0,
            SeedList = [],
            MillisecondsPerBlock = 15000u,
            MaxTransactionsPerBlock = 512u,
            MaxValidUntilBlockIncrement = 86400000u / 15000u,
            MemoryPoolMaxTransactions = 50_000,
            MaxTraceableBlocks = 2_102_400u,
            InitialGasDistribution = 52_000_000_00000000ul,
        };

        /// <summary>
        /// Gets the highest hard fork that is active at the specified block height.
        /// </summary>
        /// <param name="blockHeight">The block height to evaluate.</param>
        /// <returns>The active hard fork at <paramref name="blockHeight"/>.</returns>
        public HardFork GetActiveHardFork(long blockHeight) =>
            HardForks
                .OrderBy(static o => o.Key)
                .LastOrDefault(l => l.Value <= blockHeight)
            .Key;
    }
}
