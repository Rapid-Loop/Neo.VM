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

using Neo.Platform.Storage.Interface;
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

        public BlockchainStoreSnapshot(IStore store, RocksDb db)
        {
            _store = store;
            _db = db;
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

        public void Commit() =>
            _db.Write(_writeBatch);

        public bool ContainsKey(ReadOnlySpan<byte> key, string? columnFamilyName = default)
        {
            //if (_db.KeyMayExist(key, GetColumnFamilyHandle(columnFamilyName)))
            return TryGet(key, out _, columnFamilyName);
            //return false;
        }

        public IStoreSnapshot CreateSnapshot() =>
            new BlockchainStoreSnapshot(this, _db);

        public void Delete(ReadOnlySpan<byte> key, string? columnFamilyName = default)
        {
            //if (_db.KeyMayExist(key, GetColumnFamilyHandle(columnFamilyName)))
            _writeBatch.Delete(key, GetColumnFamilyHandle(columnFamilyName));
        }

        public byte[]? Get(ReadOnlySpan<byte> key, string? columnFamilyName = default)
        {
            //if (_db.KeyMayExist(key, GetColumnFamilyHandle(columnFamilyName)))
            return _db.Get(key, GetColumnFamilyHandle(columnFamilyName), _readOptions);
            //return default;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            using var iter = _db.NewIterator(_readOptions);
            for (iter.SeekToFirst(); iter.IsValid(); iter.Next())
                yield return new(iter.KeyToArray(), iter.ValueToArray());
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, string? columnFamilyName = default) =>
            _writeBatch.Put(key, value, GetColumnFamilyHandle(columnFamilyName));

        public IEnumerable<KeyValuePair<byte[], byte[]>> Seek(ReadOnlyMemory<byte> keyOrPrefix, bool seekFromEnd = false, string? columnFamilyName = default)
        {
            using var iter = _db.NewIterator(GetColumnFamilyHandle(columnFamilyName), _readOptions);

            for (iter.SeekForPrev(keyOrPrefix.Span); iter.IsValid();)
            {
                yield return new(iter.KeyToArray(), iter.ValueToArray());
                if (seekFromEnd)
                    iter.Prev();
                else
                    iter.Next();
            }
        }

        public bool TryGet(ReadOnlySpan<byte> key, [NotNullWhen(true)] out byte[]? value, string? columnFamilyName = default)
        {
            //if (_db.KeyMayExist(key, GetColumnFamilyHandle(columnFamilyName)))
            //{
            var data = _db.Get(key, GetColumnFamilyHandle(columnFamilyName), _readOptions);
            if (data is not null)
            {
                value = data;
                return true;
            }
            //}

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private ColumnFamilyHandle GetColumnFamilyHandle(string? columnFamilyName = default) =>
            string.IsNullOrWhiteSpace(columnFamilyName) ?
                _db.GetDefaultColumnFamily() :
                _db.GetColumnFamily(columnFamilyName);
    }
}
