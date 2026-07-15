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
using System.IO;

namespace Neo.Core.Tests.Extensions
{
    [TestClass]
    public class UT_StreamExtensions
    {
        [TestMethod]
        public void TestWrite()
        {
            using var ms = new MemoryStream();
            ms.Write((byte)0xff);
            ms.Write((ushort)0xf0f1u);
            ms.Write(0x12345678u);
            ms.Write(0xdeadc0debad0c0deul);
            ms.Write("Hello");
            ms.Write([0xd0d1d2d3]);

            byte[] expectedData = [
                0xff, // byte
                0xf1, 0xf0, // ushort
                0x78, 0x56, 0x34, 0x12, // uint
                0xde, 0xc0, 0xd0, 0xba, 0xde, 0xc0, 0xad, 0xde, // ulong
                0x05, 0x48, 0x65, 0x6c, 0x6c, 0x6f, // string "Hello" -- Compacted
                0x01, 0xd3, 0xd2, 0xd1, 0xd0, // uint[] -- Compacted
            ];

            var actualData = ms.ToArray();

            Assert.AreSequenceEqual(expectedData, actualData);
        }

        [TestMethod]
        public void TestRead()
        {
            using var ms = new MemoryStream();
            ms.Write((byte)0xff);
            ms.Write((ushort)0xf0f1u);
            ms.Write(0x12345678u);
            ms.Write(0xdeadc0debad0c0deul);
            ms.Write("Hello");
            ms.Write([0xd0d1d2d3]);

            // Set stream to begin
            ms.Seek(0, SeekOrigin.Begin);

            Assert.AreEqual(0xff, ms.Read<byte>());
            Assert.AreEqual(0xf0f1u, ms.Read<ushort>());
            Assert.AreEqual(0x12345678u, ms.Read<uint>());
            Assert.AreEqual(0xdeadc0debad0c0deul, ms.Read<ulong>());
            Assert.AreEqual("Hello", ms.ReadString());

            uint[] expectedArrayData = [0xd0d1d2d3];
            var actualArrayData = ms.ReadDynamic<uint>();

            Assert.HasCount(1, actualArrayData);
            Assert.AreSequenceEqual(expectedArrayData, actualArrayData);
        }
    }
}
