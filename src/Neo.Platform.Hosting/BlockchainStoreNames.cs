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

namespace Neo.Platform.Hosting
{
    /// <summary>
    /// Configuration key names for blockchain store and backup options.
    /// </summary>
    public static class BlockchainStoreNames
    {
        /// <summary>
        /// The root configuration section for store settings.
        /// </summary>
        public static readonly string SectionKey = "Store";

        /// <summary>
        /// Configuration key for <see cref="BlockchainStoreOptions.DatabasePath"/>.
        /// </summary>
        public static readonly string DatabasePathKey = $"{SectionKey}:{nameof(BlockchainStoreOptions.DatabasePath)}";

        /// <summary>
        /// Configuration key for <see cref="BlockchainStoreOptions.CreateIfMissing"/>.
        /// </summary>
        public static readonly string CreateIfMissingKey = $"{SectionKey}:{nameof(BlockchainStoreOptions.CreateIfMissing)}";

        /// <summary>
        /// Configuration section for store backup settings.
        /// </summary>
        public static readonly string BackupSectionKey = $"{SectionKey}:Backup";

        /// <summary>
        /// Configuration key for <see cref="BlockchainBackupOptions.BackupPath"/>.
        /// </summary>
        public static readonly string BackupPathKey = $"{BackupSectionKey}:{nameof(BlockchainBackupOptions.BackupPath)}";

        /// <summary>
        /// Configuration key for <see cref="BlockchainBackupOptions.MaxBackups"/>.
        /// </summary>
        public static readonly string MaxBackupsKey = $"{BackupSectionKey}:{nameof(BlockchainBackupOptions.MaxBackups)}";
    }
}
