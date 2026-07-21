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

using Microsoft.Extensions.Options;
using Neo.Configuration;
using System.Collections.Concurrent;
using System.IO;

namespace Neo.Platform.Storage.Tests
{
    internal sealed class TestStorePool
    {
        public static TestStorePool Shared => s_storePool;

        private static readonly TestStorePool s_storePool = new();

        private readonly ConcurrentDictionary<string, BlockchainStore> _store = [];

        public BlockchainStore Rent()
        {
            var storeOptions = Options.Create(new BlockchainStoreOptions()
            {
                DatabasePath = Path.Combine(Path.GetRandomFileName()),
            });

            var backupOptions = Options.Create(new BlockchainBackupOptions()
            {
                BackupPath = Path.Combine(Path.GetRandomFileName()),
                MaxBackups = 3,
            });

            var store = new BlockchainStore(storeOptions, backupOptions, TestUtilities.TraceLoggerFactory);

            _store.TryAdd(storeOptions.Value.DatabasePath, store);

            return store;
        }

        public void Return(BlockchainStore store)
        {
            if (_store.TryRemove(store.StoreOptions.DatabasePath, out var value))
            {
                value.Dispose();

                new DirectoryInfo(store.StoreOptions.DatabasePath)
                    .Delete(true);

                var backupDir = new DirectoryInfo(store.BackupOptions.BackupPath);

                if (backupDir.Exists)
                    backupDir.Delete(true);
            }
        }
    }
}
