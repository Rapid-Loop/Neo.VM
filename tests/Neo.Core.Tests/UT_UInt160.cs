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

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using System;

namespace Neo.Core.Tests
{
    [TestClass]
    public class UT_UInt160
    {
        [TestMethod]
        public void TestFail()
        {
            Assert.ThrowsExactly<FormatException>(() => _ = new UInt160(new byte[UInt160.Length + 1]));
        }

        [TestMethod]
        public void TestGenerator1()
        {
            var uInt160 = new UInt160();
            Assert.IsNotNull(uInt160);
        }

        [TestMethod]
        public void TestGenerator2()
        {
            UInt160 uInt160 = new byte[20];
            Assert.IsNotNull(uInt160);
        }

        [TestMethod]
        public void TestGenerator3()
        {
            UInt160 uInt160 = "0xff00000000000000000000000000000000000001";
            Assert.IsNotNull(uInt160);
            Assert.AreEqual("0xff00000000000000000000000000000000000001", uInt160.ToString());

            UInt160 value = "0x0102030405060708090a0b0c0d0e0f1011121314";
            Assert.IsNotNull(value);
            Assert.AreEqual("0x0102030405060708090a0b0c0d0e0f1011121314", value.ToString());
        }

        [TestMethod]
        public void TestCompareTo()
        {
            var temp = new byte[20];
            temp[19] = 0x01;
            var result = new UInt160(temp);
            Assert.AreEqual(0, UInt160.Zero.CompareTo(UInt160.Zero));
            Assert.AreEqual(-1, UInt160.Zero.CompareTo(result));
            Assert.AreEqual(1, result.CompareTo(UInt160.Zero));
            Assert.AreEqual(0, result.CompareTo(temp));
        }

        [TestMethod]
        public void TestEquals()
        {
            var temp = new byte[20];
            temp[19] = 0x01;
            var result = new UInt160(temp);
            Assert.IsTrue(UInt160.Zero.Equals(UInt160.Zero));
            Assert.IsFalse(UInt160.Zero.Equals(result));
            Assert.IsFalse(result.Equals(null));
            Assert.IsTrue(UInt160.Zero == UInt160.Zero);
            Assert.IsFalse(UInt160.Zero != UInt160.Zero);
            Assert.IsTrue(UInt160.Zero == "0x0000000000000000000000000000000000000000");
            Assert.IsFalse(UInt160.Zero == "0x0000000000000000000000000000000000000001");
        }

        [TestMethod]
        public void TestParse()
        {
            Assert.ThrowsExactly<FormatException>((Action)(() => UInt160.Parse(null)));
            var result = UInt160.Parse("0x0000000000000000000000000000000000000000");
            Assert.AreEqual(UInt160.Zero, result);
            Assert.ThrowsExactly<FormatException>((Action)(() => UInt160.Parse("000000000000000000000000000000000000000")));
            var result1 = UInt160.Parse("0000000000000000000000000000000000000000");
            Assert.AreEqual(UInt160.Zero, result1);
        }

        [TestMethod]
        public void TestTryParse()
        {
            Assert.IsFalse(UInt160.TryParse(null, out _));
            Assert.IsTrue(UInt160.TryParse("0x0000000000000000000000000000000000000000", out var temp));
            Assert.AreEqual("0x0000000000000000000000000000000000000000", temp.ToString());
            Assert.AreEqual(UInt160.Zero, temp);
            Assert.IsTrue(UInt160.TryParse("0x1230000000000000000000000000000000000000", out temp));
            Assert.AreEqual("0x1230000000000000000000000000000000000000", temp.ToString());
            Assert.IsFalse(UInt160.TryParse("000000000000000000000000000000000000000", out _));
            Assert.IsFalse(UInt160.TryParse("0xKK00000000000000000000000000000000000000", out _));
            Assert.IsFalse(UInt160.TryParse(" 1 2 3 45 000000000000000000000000000000", out _));
        }

        [TestMethod]
        public void TestOperatorLarger()
        {
            Assert.IsFalse(UInt160.Zero > UInt160.Zero);
            Assert.IsFalse(UInt160.Zero > "0x0000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorLargerAndEqual()
        {
            Assert.IsTrue(UInt160.Zero >= UInt160.Zero);
            Assert.IsTrue(UInt160.Zero >= "0x0000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorSmaller()
        {
            Assert.IsFalse(UInt160.Zero < UInt160.Zero);
            Assert.IsFalse(UInt160.Zero < "0x0000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorSmallerAndEqual()
        {
            Assert.IsTrue(UInt160.Zero <= UInt160.Zero);
            Assert.IsTrue(UInt160.Zero <= "0x0000000000000000000000000000000000000000");
        }
    }
}
