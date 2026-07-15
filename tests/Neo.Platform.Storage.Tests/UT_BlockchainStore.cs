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

using System.Linq;

namespace Neo.Platform.Storage.Tests
{
    [TestClass]
    public sealed class UT_BlockchainStore
    {
        [TestMethod]
        public void TestSnapshot()
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
    }
}
