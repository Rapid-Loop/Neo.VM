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
using System.Linq;

namespace Neo.Platform.Storage.Tests
{
    [TestClass]
    public sealed class UT_BlockchainStore
    {
        [TestMethod]
        public void TestDelete()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xff, 0x00, 0x00], [0x00]);
            Assert.IsTrue(store.ContainsKey([0xff, 0x00, 0x00]));

            store.Delete([0xff, 0x00, 0x00]);
            Assert.IsFalse(store.ContainsKey([0xff, 0x00, 0x00]));

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestGet()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xff, 0x00, 0x00], [0x00]);
            Assert.IsTrue(store.TryGet([0xff, 0x00, 0x00], out var value));
            Assert.IsNotNull(value);
            Assert.HasCount(1, value);
            Assert.AreEqual<byte?>(0x00, value[0]);

            value = store.Get([0xff, 0x00, 0x00]);
            Assert.IsNotNull(value);
            Assert.HasCount(1, value);
            Assert.AreEqual<byte?>(0x00, value[0]);

            Assert.IsFalse(store.TryGet([0xff, 0x00, 0x01], out value));
            Assert.IsNull(value);

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestSnapshotDelete()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xff, 0x00, 0x00], [0x00]);
            Assert.IsTrue(store.ContainsKey([0xff, 0x00, 0x00]));

            using (var snapshot = store.CreateSnapshot())
            {
                snapshot.Delete([0xff, 0x00, 0x00]);
                snapshot.Commit();
            }

            Assert.IsFalse(store.ContainsKey([0xff, 0x00, 0x00]));

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestSnapshotGet()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xff, 0x00, 0x00], [0x00]);

            using (var snapshot = store.CreateSnapshot())
            {
                Assert.IsTrue(snapshot.TryGet([0xff, 0x00, 0x00], out var value));
                Assert.IsNotNull(value);
                Assert.HasCount(1, value);
                Assert.AreEqual<byte?>(0x00, value[0]);
            }

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestSnapshotPut()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0xff, 0x00, 0x00], [0x00]);

            using (var snapshot = store.CreateSnapshot())
            {
                var actualBytes = snapshot.Get([0xff, 0x00, 0x00]);

                Assert.IsTrue(snapshot.ContainsKey([0xff, 0x00, 0x00]));
                Assert.IsNotNull(actualBytes);
                Assert.AreEqual(1, actualBytes?.Length);
                Assert.AreEqual<byte?>(0x00, actualBytes?[0]);

                actualBytes = store.Get([0xff, 0x00, 0x00]);

                Assert.IsTrue(store.ContainsKey([0xff, 0x00, 0x00]));
                Assert.IsNotNull(actualBytes);
                Assert.AreEqual(1, actualBytes?.Length);
                Assert.AreEqual<byte?>(0x00, actualBytes?[0]);

                snapshot.Put([0xff, 0x00, 0x00], [0x01]);
                snapshot.Commit();

                actualBytes = store.Get([0xff, 0x00, 0x00]);

                Assert.IsTrue(store.ContainsKey([0xff, 0x00, 0x00]));
                Assert.IsNotNull(actualBytes);
                Assert.AreEqual(1, actualBytes?.Length);
                Assert.AreEqual<byte?>(0x01, actualBytes?[0]);
            }

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestSeek()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0x00, 0x00, 0x00], [0x00]);
            store.Put([0x00, 0x00, 0x01], [0x01]);
            store.Put([0x00, 0x00, 0x02], [0x02]);
            store.Put([0x00, 0x00, 0x03], [0x03]);
            store.Put([0x00, 0x00, 0x04], [0x04]);

            var actualResults = store.Seek((byte[])[0x00, 0x00, 0x01]).ToArray();

            Assert.HasCount(4, actualResults);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x01], actualResults[0].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x02], actualResults[1].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x03], actualResults[2].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x04], actualResults[3].Key);

            actualResults = [.. store.Seek((byte[])[0x00, 0x00, 0x03], seekFromEnd: true)];

            Assert.HasCount(4, actualResults);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x03], actualResults[0].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x02], actualResults[1].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x01], actualResults[2].Key);
            Assert.AreSequenceEqual((byte[])[0x00, 0x00, 0x00], actualResults[3].Key);

            TestStorePool.Shared.Return(store);
        }

        [TestMethod]
        public void TestCheckpoint()
        {
            var store = TestStorePool.Shared.Rent();

            store.Put([0x00, 0x00, 0x00], [0x00]);

            var actualCheckpointDir = new DirectoryInfo(Path.Combine(Path.GetRandomFileName()));
            store.CreateCheckpoint(actualCheckpointDir.FullName);

            Assert.IsNotEmpty(actualCheckpointDir.EnumerateFiles());

            actualCheckpointDir.Delete(true);

            TestStorePool.Shared.Return(store);
        }
    }
}
