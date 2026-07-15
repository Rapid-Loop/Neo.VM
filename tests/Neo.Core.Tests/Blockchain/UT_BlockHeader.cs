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

using Neo.Core.Blockchain;
using Neo.Core.Extensions;
using System;

namespace Neo.Core.Tests.Blockchain
{
    [TestClass]
    public class UT_BlockHeader
    {
        private static readonly string s_blockHeaderHexString =
            "0000000000000000000000000000000000000000000000000000000000000000000000007227ba7b747f1a9" +
            "8f68679d4a98b68927646ab195a6f56b542ca5a0e6a412662493ed0e58f0100000000000000000000000000" +
            "0000000000000000000000000000000000000000000001000111";

        [TestMethod]
        public void TestSize()
        {
            var expectedPrevBlockHash = UInt256.Zero;

            var actualBlockHeader = TestUtilities.CreateBlockHeader(0, expectedPrevBlockHash);

            Assert.AreEqual(113, actualBlockHeader.Size);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var expectedPrevBlockHash = UInt256.Zero;
            var actualBlockHeader = TestUtilities.CreateBlockHeader(0, expectedPrevBlockHash);

            Assert.AreEqual(actualBlockHeader.Hash.GetHashCode(), actualBlockHeader.GetHashCode());
        }

        [TestMethod]
        public void TestDeserialize()
        {
            var expectedPrevBlockHash = UInt256.Zero;
            var expectedBlockHeader = TestUtilities.CreateBlockHeader(0, expectedPrevBlockHash);

            var actualBlockHeaderBytes = expectedBlockHeader.ToArray();

            Assert.HasCount(expectedBlockHeader.Size, actualBlockHeaderBytes);

            var actualBlockHeader = actualBlockHeaderBytes.AsSerializable<BlockHeader>();

            Assert.IsNotNull(actualBlockHeader);
            Assert.AreEqual(expectedBlockHeader.Version, actualBlockHeader.Version);
            Assert.AreEqual(expectedBlockHeader.PrevHash, actualBlockHeader.PrevHash);
            Assert.AreEqual(expectedBlockHeader.MerkleRoot, actualBlockHeader.MerkleRoot);
            Assert.AreEqual(expectedBlockHeader.Timestamp, actualBlockHeader.Timestamp);
            Assert.AreEqual(expectedBlockHeader.Index, actualBlockHeader.Index);
            Assert.AreEqual(expectedBlockHeader.NextConsensus, actualBlockHeader.NextConsensus);

            Assert.AreSequenceEqual(expectedBlockHeader.Witness.InvocationScript, actualBlockHeader.Witness.InvocationScript);
            Assert.AreSequenceEqual(expectedBlockHeader.Witness.VerificationScript, actualBlockHeader.Witness.VerificationScript);
            Assert.AreSequenceEqual(Convert.FromHexString(s_blockHeaderHexString), actualBlockHeaderBytes);
        }

        [TestMethod]
        public void TestIEquatable()
        {
            var expectedPrevBlockHash = UInt256.Zero;
            var expectedBlockHeader = TestUtilities.CreateBlockHeader(0, expectedPrevBlockHash);

            var actualBlockHeader = TestUtilities.CreateBlockHeader(0, expectedPrevBlockHash);

            Assert.AreEqual(expectedBlockHeader, actualBlockHeader);
            Assert.IsTrue(expectedBlockHeader.Equals(actualBlockHeader));
            Assert.IsTrue(expectedBlockHeader.Equals((object)actualBlockHeader));
        }
    }
}
