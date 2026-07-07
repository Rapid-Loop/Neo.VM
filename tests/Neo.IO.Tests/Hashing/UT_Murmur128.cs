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

using Neo.IO.Hashing;
using System;
using System.Text;

namespace Neo.IO.Tests.Hashing
{
    [TestClass]
    public class UT_Murmur128
    {
        private const uint DefaultTestSeed = 123u;

        [TestMethod]
        public void TestHash()
        {
            var arrayData = Encoding.ASCII.GetBytes("hello");
            var actualHash = Murmur128.Hash(arrayData, DefaultTestSeed);
            var actualHashString = Convert.ToHexStringLower(actualHash);

            Assert.AreEqual("0bc59d0ad25fde2982ed65af61227a0e", actualHashString);

            arrayData = Encoding.ASCII.GetBytes("world");
            actualHash = Murmur128.Hash(arrayData, DefaultTestSeed);
            actualHashString = Convert.ToHexStringLower(actualHash);

            Assert.AreEqual("3d3810fed480472bd214a14023bb407f", actualHashString);

            arrayData = Encoding.ASCII.GetBytes("hello world");
            actualHash = Murmur128.Hash(arrayData, DefaultTestSeed);
            actualHashString = Convert.ToHexStringLower(actualHash);

            Assert.AreEqual("e0a0632d4f51302c55e3b3e48d28795d", actualHashString);
        }

        [TestMethod]
        public void TestMoreThan16Characters()
        {
            var arrayData = Encoding.ASCII.GetBytes("Testing more than 16 characters.");
            var actualHash = Murmur128.Hash(arrayData, DefaultTestSeed);
            var actualHashString = Convert.ToHexStringLower(actualHash);

            Assert.AreEqual("6903e6d3d2fe781787819c89b2c11aa9", actualHashString);

            arrayData = Encoding.ASCII.GetBytes
            (
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Sed do eiusmod tempor incididunt ut labore et dolore magna " +
                "aliqua. Ut enim ad minim veniam, quis nostrud exercitation " +
                "ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                "Duis aute irure dolor in reprehenderit in voluptate velit " +
                "esse cillum dolore eu fugiat nulla pariatur. Excepteur sint " +
                "occaecat cupidatat non proident, sunt in culpa qui officia " +
                "deserunt mollit anim id est laborum."
            );
            actualHash = Murmur128.Hash(arrayData, DefaultTestSeed);
            actualHashString = Convert.ToHexStringLower(actualHash);

            Assert.AreEqual("dacfff46c267a7be8dbb73489c1d2678", actualHashString);
        }
    }
}
