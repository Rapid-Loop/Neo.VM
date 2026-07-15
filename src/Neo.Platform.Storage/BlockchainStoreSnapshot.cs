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
using Neo.Platform.Storage.Interface;
using Neo.Platform.Storage.Logging;
using RocksDbNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Neo.Platform.Storage
{
    public class BlockchainStoreSnapshot : IEnumerable<KeyValuePair<byte[], byte[]>>, IStoreSnapshot
    {
        public IStore Store => _store;

        private readonly IStore _store;
        private readonly RocksDb _db;
        private readonly Snapshot _snapshot;
        private readonly ReadOptions _readOptions;
        private readonly WriteBatch _writeBatch;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

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

        public void Dispose()
        {
            _snapshot.Dispose();
            _readOptions.Dispose();
            _writeBatch.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogCommitMessage(logLevel, $"Committing {_writeBatch.Count} change(s) to the database");

            _db.Write(_writeBatch);
        }

        public bool ContainsKey(ReadOnlySpan<byte> key)
        {
            if (_db.KeyMayExist(key))
                return TryGet(key, out _);
            return false;
        }

        public IStoreSnapshot CreateSnapshot()
        {
            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogCreateSnapshotMessage(logLevel, "Creating snapshot of the snapshot.");

            return new BlockchainStoreSnapshot(this, _db, _loggerFactory);
        }

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

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            using var iter = _db.NewIterator(_readOptions);
            for (iter.SeekToFirst(); iter.IsValid(); iter.Next())
                yield return new(iter.KeyToArray(), iter.ValueToArray());
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            _writeBatch.Put(key, value);

            var logLevel = LogLevel.Debug;

            if (_logger.IsEnabled(logLevel))
                _logger.LogWriteMessage(logLevel, $"Put key: {Convert.ToHexStringLower(key)} value: {Convert.ToHexStringLower(value)}");
        }

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
