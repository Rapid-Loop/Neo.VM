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
using Neo.Core.Storage;
using Neo.Platform.Storage.Logging;
using RocksDbNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Storage
{
    /// <summary>
    /// Point-in-time RocksDB snapshot that stages writes in a batch until <see cref="Commit"/>.
    /// </summary>
    public class BlockchainStoreSnapshot : IEnumerable<KeyValuePair<byte[], byte[]>>, IStoreSnapshot
    {
        /// <summary>
        /// Gets the store that produced this snapshot.
        /// </summary>
        public IStore Store => _store;

        private readonly IStore _store;
        private readonly RocksDb _db;
        private readonly Snapshot _snapshot;
        private readonly ReadOptions _readOptions;
        private readonly WriteBatch _writeBatch;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new snapshot over the given store and RocksDB instance.
        /// </summary>
        /// <param name="store">The parent store associated with this snapshot.</param>
        /// <param name="db">The open RocksDB database.</param>
        /// <param name="loggerFactory">Optional logger factory. When omitted, a null logger is used.</param>
        public BlockchainStoreSnapshot(
            IStore store,
            RocksDb db,
            ILoggerFactory? loggerFactory = default)
        {
            _store = store;
            _db = db;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerFactory.CreateLogger<BlockchainStoreSnapshot>();

            _snapshot = _db.NewSnapshot();
            _readOptions = new();
            _readOptions.SetSnapshot(_snapshot);
            _writeBatch = new();
        }

        /// <summary>
        /// Releases the RocksDB snapshot, read options, and write batch.
        /// </summary>
        public void Dispose()
        {
            _snapshot.Dispose();
            _readOptions.Dispose();
            _writeBatch.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Applies all staged write-batch changes to the database.
        /// </summary>
        public void Commit()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogCommitMessage(logLevel, $"Committing {_writeBatch.Count} change(s) to the database");

            _db.Write(_writeBatch);
        }

        /// <summary>
        /// Determines whether <paramref name="key"/> is present as of this snapshot.
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
        /// Creates a backup handle for the underlying store.
        /// </summary>
        /// <returns>An <see cref="IStoreBackup"/> from the parent store.</returns>
        public IStoreBackup CreateBackup() =>
            _store.CreateBackup();

        /// <summary>
        /// Creates a checkpoint of the underlying store.
        /// </summary>
        /// <param name="checkpointDirectory">The directory that will receive the checkpoint.</param>
        public void CreateCheckpoint(string checkpointDirectory) =>
            _store.CreateCheckpoint(checkpointDirectory);

        /// <summary>
        /// Creates a nested snapshot over the same database.
        /// </summary>
        /// <returns>A new <see cref="IStoreSnapshot"/>.</returns>
        public IStoreSnapshot CreateSnapshot()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogSnapshotMessage(logLevel, "Creating snapshot of the snapshot.");

            return new BlockchainStoreSnapshot(this, _db, _loggerFactory);
        }

        /// <summary>
        /// Stages deletion of <paramref name="key"/> in the write batch when the key may exist.
        /// </summary>
        /// <param name="key">The storage key to delete.</param>
        public void Delete(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                _writeBatch.Delete(key);

                var logLevel = LogLevel.Debug;

                if (_logger.IsEnabled(logLevel))
                    _logger.LogDeleteMessage(logLevel, $"Deleted key: {Convert.ToHexStringLower(key)}");
            }
        }

        /// <summary>
        /// Gets the value for <paramref name="key"/> as of this snapshot, or <see langword="null"/> if missing.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <returns>The stored value, or <see langword="null"/> when missing.</returns>
        public byte[]? Get(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
            {
                var data = _db.Get(key, _readOptions);
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

        /// <summary>
        /// Enumerates all key/value pairs visible in this snapshot.
        /// </summary>
        /// <returns>An enumerator over snapshot contents.</returns>
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            using var iter = _db.NewIterator(_readOptions);
            for (iter.SeekToFirst(); iter.IsValid(); iter.Next())
                yield return new(iter.KeyToArray(), iter.ValueToArray());
        }

        /// <summary>
        /// Stages a put of <paramref name="value"/> under <paramref name="key"/> in the write batch.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to store.</param>
        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            _writeBatch.Put(key, value);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogWriteMessage(logLevel, $"Put key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(value)}");
        }

        /// <summary>
        /// Iterates key/value pairs from this snapshot starting at or before <paramref name="keyOrPrefix"/>.
        /// </summary>
        /// <param name="keyOrPrefix">The seek key or prefix.</param>
        /// <param name="seekFromEnd">
        /// When <see langword="true"/>, iterates backward; otherwise, forward.
        /// </param>
        /// <returns>An enumerable of key/value pairs in seek order.</returns>
        public IEnumerable<KeyValuePair<byte[], byte[]>> Seek(ReadOnlyMemory<byte> keyOrPrefix, bool seekFromEnd = false)
        {
            using var iter = _db.NewIterator(_readOptions);

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

        /// <summary>
        /// Attempts to get the value for <paramref name="key"/> as of this snapshot.
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
                var data = _db.Get(key, _readOptions);
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

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
