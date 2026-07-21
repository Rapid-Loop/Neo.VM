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

using System.IO;

namespace Neo.Platform.Storage.Tests
{
    [TestClass]
    public class UT_BlockchainStoreBackup
    {
        [TestMethod]
        public void TestCreateBackup()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xde, 0xad, 0x12], [0x12]);

            using (var backup = store.CreateBackup())
            {
                Assert.IsNotNull(backup);

                backup.Backup();
            }

            var actualBackupDir = new DirectoryInfo(store.BackupOptions.BackupPath);

            Assert.IsTrue(actualBackupDir.Exists);
            Assert.IsNotEmpty(actualBackupDir.EnumerateDirectories());

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestRestoreBackup()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xde, 0xad, 0x12], [0x12]);

            using (var backup = store.CreateBackup())
            {
                Assert.IsNotNull(backup);

                backup.Backup();

                var actualBackupDir = new DirectoryInfo(store.BackupOptions.BackupPath);

                Assert.IsTrue(actualBackupDir.Exists);
                Assert.IsNotEmpty(actualBackupDir.EnumerateDirectories());

                var actualRestoreDir = new DirectoryInfo(Path.Combine(Path.GetRandomFileName()));

                if (actualRestoreDir.Exists == false)
                    actualRestoreDir.Create();

                backup.Restore(actualRestoreDir.FullName);

                Assert.IsNotEmpty(actualRestoreDir.EnumerateFiles());

                actualRestoreDir.Delete(true);
            }

            TestStorePool.Shared.Return(store);
        }
    }
}
