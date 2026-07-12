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

using Neo.IO.Data;
using System;

namespace Neo.IO.Tests.Data
{
    [TestClass]
    public class UT_BloomFilter
    {
        [TestMethod]
        public void TestAddContains()
        {
            var expectedElements = 7;
            var nTweak = 123456u;
            byte[] elements = [0, 1, 2, 3, 4];

            var filter = new BloomFilter(expectedElements, tweak: nTweak);
            filter.Add(elements);

            Assert.IsTrue(filter.Contains(elements));

            byte[] anotherElements = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

            Assert.IsFalse(filter.Contains(anotherElements));
        }

        [TestMethod]
        public void TestBloomFilterConstructorGetMTweak()
        {
            var expectedElements = 7;
            var nTweak = 123456u;

            var filter = new BloomFilter(expectedElements, tweak: nTweak);

            Assert.AreEqual(68, filter.BitSize);
            Assert.AreEqual(nTweak, filter.Tweak);
        }

        [TestMethod]
        public void TestGetBits()
        {
            var expectedElements = 7;
            var nTweak = 123456u;

            var filter = new BloomFilter(expectedElements, tweak: nTweak);
            var result = new byte[68];

            filter.GetBits(result);

            foreach (var value in result)
                Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TestInvalidArguments()
        {
            var expectedElements = -7;
            var nTweak = 123456u;

            var action = () => new BloomFilter(expectedElements, tweak: nTweak);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

            action = () => new BloomFilter(expectedElements, tweak: nTweak, elementBytes: [0, 1, 2, 3, 4]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);
        }
    }
}
