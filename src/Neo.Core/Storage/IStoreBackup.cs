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

using System;

namespace Neo.Core.Storage
{
    /// <summary>
    /// Creates, purges, and restores store backups.
    /// </summary>
    public interface IStoreBackup : IDisposable
    {
        /// <summary>
        /// Creates a backup of the store.
        /// </summary>
        void Backup();

        /// <summary>
        /// Removes older backups, retaining the specified number of most recent ones.
        /// </summary>
        /// <param name="numberOfBackupsToKeep">The number of backups to keep.</param>
        void Purge(uint numberOfBackupsToKeep);

        /// <summary>
        /// Restores the store from a backup path.
        /// </summary>
        /// <param name="restorePath">The path of the backup to restore.</param>
        /// <param name="walPath">Optional write-ahead log path used during restore.</param>
        void Restore(string restorePath, string? walPath = null);
    }
}
