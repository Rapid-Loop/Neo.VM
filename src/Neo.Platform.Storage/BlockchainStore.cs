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
using Neo.Platform.Storage.Interface;
using Neo.Platform.Storage.Logging;
using RocksDbNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo.Platform.Storage
{
    public sealed class BlockchainStore : IEnumerable<KeyValuePair<byte[], byte[]>>, IStore
    {
        public BlockchainStoreOptions Options => _options;

        private static readonly ColumnFamilyDescriptor[] s_columnFamilies =
        [
            new(ColumnFamilyNames.Default),
            new(ColumnFamilyNames.Checkpoints),
            new(ColumnFamilyNames.Backups),
        ];

        private readonly Cache _blockSharedCache;
        private readonly FilterPolicy _bloomFilter;
        private readonly RocksDb _db;

        private readonly BlockchainStoreOptions _options;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public BlockchainStore(
            IOptions<BlockchainStoreOptions> options,
            ILoggerFactory? loggerFactory = default)
        {
            _options = options.Value;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<BlockchainStore>();

            var dirInfo = new DirectoryInfo(_options.DatabasePath);

            if (dirInfo.Exists == false)
                dirInfo.Create();

            var gcInfo = GC.GetGCMemoryInfo();
            var ram = (ulong)gcInfo.TotalAvailableMemoryBytes / 3ul;  // 33% of all RAM

            _blockSharedCache = Cache.CreateLru(ram);
            _bloomFilter = FilterPolicy.CreateBloom(10);

            var dbOptions = new DbOptions()
            {
                CreateIfMissing = _options.CreateIfMissing,
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

            _db = RocksDb.Open(dbOptions, _options.DatabasePath, s_columnFamilies);
        }

        public void Dispose()
        {
            _db.Dispose();
            _blockSharedCache.Dispose();
            _bloomFilter.Dispose();
        }

        public IStoreSnapshot CreateSnapshot()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogCreateSnapshotMessage(logLevel, "Creating snapshot of the store.");

            return new BlockchainStoreSnapshot(this, _db, _loggerFactory);
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            _db.Put(key, value);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogWriteMessage(logLevel, $"Put key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(value)}");
        }

        public void Delete(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                _db.Delete(key);

                var logLevel = LogLevel.Debug;

                if (_logger.IsEnabled(logLevel))
                    _logger.LogDeleteMessage(logLevel, $"Deleted key: {Convert.ToHexStringLower(key)}");
            }
        }

        public bool ContainsKey(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
                return TryGet(key, out _);
            return false;
        }

        public byte[]? Get(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                var data = _db.Get(key);
                if (data is not null)
                {
                    var logLevel = LogLevel.Debug;

                    if (_logger.IsEnabled(logLevel))
                        _logger.LogReadMessage(logLevel, $"Get key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(data)}");

                    return data;
                }
            }
            return default;
        }

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
                        _logger.LogReadMessage(logLevel, $"TryGet key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(data)}");

                    return true;
                }
            }

            value = default;
            return false;
        }

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
                    _logger.LogReadMessage(logLevel, $"Seek key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(value)}");

                if (seekFromEnd)
                    iter.Prev();
                else
                    iter.Next();
            }
        }

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
