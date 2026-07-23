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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Neo.Configuration;
using Neo.Core.Storage;
using Neo.Platform.Storage.Logging;
using RocksDbNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Platform.Storage
{
    /// <summary>
    /// RocksDB backup engine wrapper that implements <see cref="IStoreBackup"/>.
    /// </summary>
    public class BlockchainStoreBackup : IStoreBackup
    {
        /// <summary>
        /// Gets the backup options used by this instance.
        /// </summary>
        public BlockchainBackupOptions BackupOptions => _backupOptions;

        private readonly BlockchainBackupOptions _backupOptions;

        private readonly RocksDb _db;
        private readonly BackupEngine _backupEngine;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<BlockchainStoreBackup> _logger;

        /// <summary>
        /// Initializes a new backup engine for the specified database.
        /// </summary>
        /// <param name="db">The open RocksDB instance to back up.</param>
        /// <param name="options">Backup options, including the backup directory path.</param>
        /// <param name="loggerFactory">Optional logger factory. When omitted, a null logger is used.</param>
        public BlockchainStoreBackup(
            RocksDb db,
            IOptions<BlockchainBackupOptions> options,
            ILoggerFactory? loggerFactory = default)
        {
            _db = db;
            _backupOptions = options.Value;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<BlockchainStoreBackup>();

            var dirInfo = new DirectoryInfo(_backupOptions.BackupPath);

            if (dirInfo.Exists == false)
                dirInfo.Create();

            _backupEngine = BackupEngine.Open(new(), _backupOptions.BackupPath);
        }

        /// <summary>
        /// Releases the underlying RocksDB backup engine.
        /// </summary>
        public void Dispose()
        {
            _backupEngine.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new backup of the database.
        /// </summary>
        public void Backup()
        {
            _backupEngine.CreateNewBackup(_db);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogBackupMessage(logLevel, $"Created new backup of the store.");
        }

        /// <summary>
        /// Restores the latest backup to the specified path.
        /// </summary>
        /// <param name="restorePath">The directory that will receive the restored database.</param>
        /// <param name="walPath">
        /// Optional WAL directory. When omitted, <paramref name="restorePath"/> is used.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="restorePath"/> is <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <paramref name="restorePath"/> or <paramref name="walPath"/> does not exist.
        /// </exception>
        public void Restore(string restorePath, string? walPath = default)
        {
            if (string.IsNullOrEmpty(restorePath))
                throw new ArgumentException("Restore path cannot be null or empty.", nameof(restorePath));

            if (Directory.Exists(restorePath) == false)
                throw new DirectoryNotFoundException($"Restore path \'{restorePath}\' does not exist.");

            if (string.IsNullOrEmpty(walPath) == false &&
                Directory.Exists(walPath) == false)
                throw new DirectoryNotFoundException($"WAL path \'{walPath}\' does not exist.");

            _backupEngine.RestoreDbFromLatestBackup(restorePath, walPath ?? restorePath);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogRestoreMessage(logLevel, $"Restored latest backup to path \'{restorePath}\'.");
        }

        /// <summary>
        /// Lists available backups managed by the backup engine.
        /// </summary>
        /// <returns>An enumerable of backup metadata.</returns>
        public IEnumerable<BackupInfo> ListBackups() =>
            _backupEngine.AsEnumerable();

        /// <summary>
        /// Deletes older backups, keeping only the newest backups up to the specified count.
        /// </summary>
        /// <param name="numberOfBackupsToKeep">The number of most recent backups to retain.</param>
        public void Purge(uint numberOfBackupsToKeep)
        {
            _backupEngine.PurgeOldBackups(numberOfBackupsToKeep);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogRestoreMessage(logLevel, $"Purged old backups, keeping the latest {numberOfBackupsToKeep} backups.");
        }
    }
}
