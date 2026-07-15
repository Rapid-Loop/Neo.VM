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

using Neo.Core.Extensions;
using System;
using System.IO;

namespace Neo.Core.Tests.Extensions
{
    [TestClass]
    public class UT_ByteExtensions
    {
        [TestMethod]
        public void TestXor()
        {
            byte[] expectedBytes1 = [0xff, 0xff, 0xff];
            byte[] expectedBytes2 = [0xff, 0x00, 0xff];

            var expectedBytes3 = new byte[ushort.MaxValue];
            Array.Fill(expectedBytes3, (byte)0xff);

            // Data to small for Vector matrix
            var actualXorByte1 = expectedBytes1.Xor([0x00, 0x00, 0x00]);
            var actualXorByte2 = expectedBytes1.Xor([0x00, 0xff, 0x00]);

            // use Vector matrix to process XOR
            var actualXorByte3 = expectedBytes3.Xor(new byte[ushort.MaxValue]);

            Assert.HasCount(3, actualXorByte1);
            Assert.AreSequenceEqual(expectedBytes1, actualXorByte1);

            Assert.HasCount(3, actualXorByte2);
            Assert.AreSequenceEqual(expectedBytes2, actualXorByte2);

            Assert.HasCount(ushort.MaxValue, actualXorByte3);
            Assert.AreSequenceEqual(expectedBytes3, actualXorByte3);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new byte[0].Xor(new byte[ushort.MaxValue]));
        }

        [TestMethod]
        public void TestSerializedSize()
        {
            byte[] expectedBytes = [0xad, 0xde];

            var actualByteCount = expectedBytes.GetSerializedSize();

            using var ms = new MemoryStream();
            ms.Write<byte>(expectedBytes);

            var actualBytes = ms.ToArray();

            Assert.AreEqual(3, actualByteCount);
            Assert.HasCount(3, actualBytes);
        }
    }
}
