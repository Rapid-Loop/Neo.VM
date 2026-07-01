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
using System.IO;

namespace Neo.Platform.Storage.Tests
{
    [TestClass]
    public sealed class UT_BlockchainStore
    {
        [TestMethod]
        public void TestColumnFamilyNames()
        {
            var options = Options.Create(new BlockchainStoreOptions());
            var store = new BlockchainStore(options);

            store.Put([0xff, 0x00, 0x00], [0x00]);
            store.Put([0xff, 0x00, 0x01], [0x01], ColumnFamilyNames.Blocks);

            var actualBytes = store.Get([0xff, 0x00, 0x01]);

            Assert.IsTrue(actualBytes.IsEmpty);
            Assert.AreEqual(0, actualBytes.Length);

            actualBytes = store.Get([0xff, 0x00, 0x01], ColumnFamilyNames.Blocks);

            Assert.IsFalse(actualBytes.IsEmpty);
            Assert.AreEqual(1, actualBytes.Length);
            Assert.AreEqual(1, actualBytes[0]);

            store.Dispose();

            var dirInfo = new DirectoryInfo(options.Value.DatabasePath);

            dirInfo.Delete(true);
        }
    }
}
