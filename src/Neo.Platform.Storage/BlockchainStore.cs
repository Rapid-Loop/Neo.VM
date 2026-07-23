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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo.Platform.Storage
{
    /// <summary>
    /// RocksDB-backed blockchain key/value store implementing <see cref="IStore"/>.
    /// </summary>
    public sealed class BlockchainStore : IEnumerable<KeyValuePair<byte[], byte[]>>, IStore
    {
        /// <summary>
        /// Gets the store options used to open the database.
        /// </summary>
        public BlockchainStoreOptions StoreOptions => _storeOptions.Value;

        /// <summary>
        /// Gets the backup options used when creating backups.
        /// </summary>
        public BlockchainBackupOptions BackupOptions => _backupOptions.Value;

        internal RocksDb Database => _db;

        private static readonly ColumnFamilyDescriptor[] s_columnFamilies =
        [
            new(ColumnFamilyNames.Default),
        ];

        private readonly Cache _blockSharedCache;
        private readonly FilterPolicy _bloomFilter;
        private readonly RocksDb _db;

        private readonly IOptions<BlockchainStoreOptions> _storeOptions;
        private readonly IOptions<BlockchainBackupOptions> _backupOptions;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Opens or creates a RocksDB database at the path specified by <paramref name="options"/>.
        /// </summary>
        /// <param name="options">Store options, including the database path.</param>
        /// <param name="backupOptions">Optional backup options. When omitted, default backup options are used.</param>
        /// <param name="loggerFactory">Optional logger factory. When omitted, a null logger is used.</param>
        public BlockchainStore(
            IOptions<BlockchainStoreOptions> options,
            IOptions<BlockchainBackupOptions>? backupOptions = default,
            ILoggerFactory? loggerFactory = default)
        {
            _storeOptions = options;
            _backupOptions = backupOptions ?? Options.Create(new BlockchainBackupOptions());
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<BlockchainStore>();

            var dirInfo = new DirectoryInfo(_storeOptions.Value.DatabasePath);

            if (dirInfo.Exists == false)
                dirInfo.Create();

            var gcInfo = GC.GetGCMemoryInfo();
            var ram = (ulong)gcInfo.TotalAvailableMemoryBytes / 3ul;  // 33% of all RAM

            _blockSharedCache = Cache.CreateLru(ram);
            _bloomFilter = FilterPolicy.CreateBloom(10);

            var dbOptions = new DbOptions()
            {
                CreateIfMissing = _storeOptions.Value.CreateIfMissing,
                CreateMissingColumnFamilies = true,

                // Compression
                Compression = Compression.Lz4,
                BottommostCompression = Compression.Zstd,

                // Parallelism
                MaxBackgroundJobs = Math.Max(2, Environment.ProcessorCount),
                MaxSubcompactions = (uint)Environment.ProcessorCount / 2,

                // Write buffers
                WriteBufferSize = 256 * 1024 * 1024, // 256MB
                MaxWriteBufferNumber = 5,
                MinWriteBufferNumberToMerge = 2,

                // Compaction
                NumLevels = 7,
                Level0FileNumCompactionTrigger = 4,
                Level0SlowdownWritesTrigger = 20,
                Level0StopWritesTrigger = 36,
                TargetFileSizeBase = 128 * 1024 * 1024, // 128MB
                MaxBytesForLevelBase = 256 * 1024 * 1024,  // 256MB
                LevelCompactionDynamicLevelBytes = true,

                // I/O
                BytesPerSync = 0,
                UseDirectReads = false,
                AllowConcurrentMemtableWrite = true,
                AtomicFlush = false,

                // Files
                MaxOpenFiles = -1, // Keep all files open

                // WAL
                WalRecoveryMode = WalRecoveryMode.PointInTime,
                ManualWalFlush = false,

                // Cache
                BlockBasedTableFactory = CreateBlockTableOptions(),
            };

            dbOptions.OptimizeForPointLookup(64); // 64MB

            _db = RocksDb.Open(dbOptions, _storeOptions.Value.DatabasePath, s_columnFamilies);
        }

        /// <summary>
        /// Releases the underlying RocksDB database and related native resources.
        /// </summary>
        public void Dispose()
        {
            _db.Dispose();
            _blockSharedCache.Dispose();
            _bloomFilter.Dispose();
        }

        /// <summary>
        /// Creates a RocksDB checkpoint in the specified directory.
        /// </summary>
        /// <param name="checkpointDirectory">The directory that will receive the checkpoint.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="checkpointDirectory"/> is <see langword="null"/> or empty.
        /// </exception>
        public void CreateCheckpoint(string checkpointDirectory)
        {
            if (string.IsNullOrEmpty(checkpointDirectory))
                throw new ArgumentException("Checkpoint name cannot be null or empty.", nameof(checkpointDirectory));

            using var checkpoint = Checkpoint.Create(_db);
            checkpoint.CreateCheckpoint(checkpointDirectory);

            var logLevel = LogLevel.Information;

            if (_logger.IsEnabled(logLevel))
                _logger.LogCheckpointMessage(logLevel, $"Created checkpoint: \'{checkpointDirectory}\'");
        }

        /// <summary>
        /// Creates a backup handle for this store.
        /// </summary>
        /// <returns>An <see cref="IStoreBackup"/> bound to this database.</returns>
        public IStoreBackup CreateBackup() =>
            new BlockchainStoreBackup(_db, _backupOptions, _loggerFactory);

        /// <summary>
        /// Creates a consistent snapshot of the current store state.
        /// </summary>
        /// <returns>An <see cref="IStoreSnapshot"/> for transactional reads and writes.</returns>
        public IStoreSnapshot CreateSnapshot()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogSnapshotMessage(logLevel, "Creating snapshot of the store.");

            return new BlockchainStoreSnapshot(this, _db, _loggerFactory);
        }

        /// <summary>
        /// Writes <paramref name="value"/> under <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to store.</param>
        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            _db.Put(key, value);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogWriteMessage(logLevel, $"Put key: 0x{Convert.ToHexStringLower(key)} value: 0x{Convert.ToHexStringLower(value)}");
        }

        /// <summary>
        /// Deletes the value associated with <paramref name="key"/> if it may exist.
        /// </summary>
        /// <param name="key">The storage key to delete.</param>
        public void Delete(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                _db.Delete(key);

                var logLevel = LogLevel.Debug;

                if (_logger.IsEnabled(logLevel))
                    _logger.LogDeleteMessage(logLevel, $"Deleted key: 0x{Convert.ToHexStringLower(key)}");
            }
        }

        /// <summary>
        /// Determines whether <paramref name="key"/> is present in the store.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <returns><see langword="true"/> if the key exists; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
                return TryGet(key, out _);
            return false;
        }

        /// <summary>
        /// Gets the value for <paramref name="key"/>, or <see langword="null"/> if it does not exist.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <returns>The stored value, or <see langword="null"/> when missing.</returns>
        public byte[]? Get(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                var data = _db.Get(key);
                if (data is not null)
                {
                    var logLevel = LogLevel.Debug;

                    if (_logger.IsEnabled(logLevel))
                        _logger.LogReadMessage(logLevel, $"Get key: 0x{Convert.ToHexStringLower(key)} value: 0x{Convert.ToHexStringLower(data)}");

                    return data;
                }
            }
            return default;
        }

        /// <summary>
        /// Attempts to get the value for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="value">
        /// When this method returns <see langword="true"/>, contains the stored value;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGet(ReadOnlySpan<byte> key, [NotNullWhen(true)] out byte[]? value)
        {
            if (_db.KeyMayExist(key))
            {
                var data = _db.Get(key);
                if (data is not null)
                {
                    value = data;

                    var logLevel = LogLevel.Debug;

                    if (_logger.IsEnabled(logLevel))
                        _logger.LogReadMessage(logLevel, $"TryGet key: 0x{Convert.ToHexStringLower(key)} value: 0x{Convert.ToHexStringLower(data)}");

                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Iterates key/value pairs starting at or before <paramref name="keyOrPrefix"/>.
        /// </summary>
        /// <param name="keyOrPrefix">The seek key or prefix.</param>
        /// <param name="seekFromEnd">
        /// When <see langword="true"/>, iterates backward; otherwise, forward.
        /// </param>
        /// <returns>An enumerable of key/value pairs in seek order.</returns>
        public IEnumerable<KeyValuePair<byte[], byte[]>> Seek(ReadOnlyMemory<byte> keyOrPrefix, bool seekFromEnd = false)
        {
            using var iter = _db.NewIterator();

            for (iter.SeekForPrev(keyOrPrefix.Span); iter.IsValid();)
            {
                var key = iter.KeyToArray();
                var value = iter.ValueToArray();

                yield return new(key, value);

                var logLevel = LogLevel.Debug;

                if (_logger.IsEnabled(logLevel))
                    _logger.LogReadMessage(logLevel, $"Seek key: 0x{Convert.ToHexStringLower(key)} value: 0x{Convert.ToHexStringLower(value)}");

                if (seekFromEnd)
                    iter.Prev();
                else
                    iter.Next();
            }
        }

        /// <summary>
        /// Returns a non-generic enumerator over all key/value pairs.
        /// </summary>
        /// <returns>An enumerator for the store contents.</returns>
        public IEnumerator GetEnumerator() =>
            GetEnumerator();

        IEnumerator<KeyValuePair<byte[], byte[]>> IEnumerable<KeyValuePair<byte[], byte[]>>.GetEnumerator()
        {
            using var iter = _db.NewIterator();
            for (iter.SeekToFirst(); iter.IsValid(); iter.Next())
                yield return new(iter.KeyToArray(), iter.ValueToArray());
        }

        private BlockBasedTableOptions CreateBlockTableOptions()
        {
            var blockOptions = new BlockBasedTableOptions()
            {
                FormatVersion = 5,
                BlockSize = 64 * 1024, // 64KB
                CacheIndexAndFilterBlocks = true,
                PinL0FilterAndIndexBlocksInCache = true,
                WholeKeyFiltering = true,
            };

            blockOptions.SetBlockCache(_blockSharedCache);
            blockOptions.SetFilterPolicy(_bloomFilter);

            return blockOptions;
        }
    }
}
