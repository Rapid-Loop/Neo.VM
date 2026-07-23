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

using Neo.Core;

namespace Neo.Platform.Hosting
{
    /// <summary>
    /// Configuration key names for Neo protocol settings.
    /// </summary>
    public static class ProtocolSettingNames
    {
        /// <summary>
        /// The root configuration section for protocol settings.
        /// </summary>
        public static readonly string SectionKey = "Protocol";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.Network"/>.
        /// </summary>
        public static readonly string NetworkKey = $"{SectionKey}:{nameof(ProtocolSettings.Network)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.AddressVersion"/>.
        /// </summary>
        public static readonly string AddressVersionKey = $"{SectionKey}:{nameof(ProtocolSettings.AddressVersion)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.StandbyCommittee"/>.
        /// </summary>
        public static readonly string StandbyCommitteeKey = $"{SectionKey}:{nameof(ProtocolSettings.StandbyCommittee)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.ValidatorsCount"/>.
        /// </summary>
        public static readonly string ValidatorsCountKey = $"{SectionKey}:{nameof(ProtocolSettings.ValidatorsCount)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.HardForks"/>.
        /// </summary>
        public static readonly string HardForksKey = $"{SectionKey}:{nameof(ProtocolSettings.HardForks)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.SeedList"/>.
        /// </summary>
        public static readonly string SeedListKey = $"{SectionKey}:{nameof(ProtocolSettings.SeedList)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.MillisecondsPerBlock"/>.
        /// </summary>
        public static readonly string MillisecondsPerBlockKey = $"{SectionKey}:{nameof(ProtocolSettings.MillisecondsPerBlock)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.MaxTransactionsPerBlock"/>.
        /// </summary>
        public static readonly string MaxTransactionsPerBlockKey = $"{SectionKey}:{nameof(ProtocolSettings.MaxTransactionsPerBlock)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.MemoryPoolMaxTransactions"/>.
        /// </summary>
        public static readonly string MemoryPoolMaxTransactionsKey = $"{SectionKey}:{nameof(ProtocolSettings.MemoryPoolMaxTransactions)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.MaxTraceableBlocks"/>.
        /// </summary>
        public static readonly string MaxTraceableBlocksKey = $"{SectionKey}:{nameof(ProtocolSettings.MaxTraceableBlocks)}";

        /// <summary>
        /// Configuration key for <see cref="ProtocolSettings.InitialGasDistribution"/>.
        /// </summary>
        public static readonly string InitialGasDistributionKey = $"{SectionKey}:{nameof(ProtocolSettings.InitialGasDistribution)}";
    }
}
