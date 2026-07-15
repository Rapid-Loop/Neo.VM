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

using Neo.Core.Extensions;
using System;

namespace Neo.Core.Tests
{
    [TestClass]
    public class UT_UInt256
    {
        [TestMethod]
        public void TestFail()
        {
            Assert.ThrowsExactly<FormatException>(() => _ = new UInt256(new byte[UInt256.Length + 1]));
        }

        [TestMethod]
        public void TestGenerator1()
        {
            UInt256 uInt256 = new();
            Assert.IsNotNull(uInt256);
        }

        [TestMethod]
        public void TestGenerator2()
        {
            UInt256 uInt256 = new byte[32];
            Assert.IsNotNull(uInt256);
            Assert.AreEqual(UInt256.Zero, uInt256);
        }

        [TestMethod]
        public void TestGenerator3()
        {
            UInt256 uInt256 = "0xff00000000000000000000000000000000000000000000000000000000000001";
            Assert.IsNotNull(uInt256);
            Assert.AreEqual("0xff00000000000000000000000000000000000000000000000000000000000001", uInt256.ToString());

            UInt256 value = "0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";
            Assert.IsNotNull(value);
            Assert.AreEqual("0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20", value.ToString());
        }

        [TestMethod]
        public void TestCompareTo()
        {
            var temp = new byte[32];
            temp[31] = 0x01;
            UInt256 result = new(temp);
            Assert.AreEqual(0, UInt256.Zero.CompareTo(UInt256.Zero));
            Assert.AreEqual(-1, UInt256.Zero.CompareTo(result));
            Assert.AreEqual(1, result.CompareTo(UInt256.Zero));
            Assert.AreEqual(0, result.CompareTo(temp));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            UInt256 expectedValue = "0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";
            var expectedValueBytes = expectedValue.ToArray();

            var actualValue = expectedValueBytes.AsSerializable<UInt256>();

            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestEquals()
        {
            var temp = new byte[32];
            temp[31] = 0x01;

            var result = new UInt256(temp);
            Assert.IsTrue(UInt256.Zero.Equals(UInt256.Zero));
            Assert.IsFalse(UInt256.Zero.Equals(result));
            Assert.IsFalse(result.Equals(null));
        }

        [TestMethod]
        public void TestEquals1()
        {
            var temp1 = new UInt256();
            var temp2 = new UInt256();
            var temp3 = new UInt160();
            Assert.IsFalse(temp1.Equals(null));
            Assert.IsTrue(temp1.Equals(temp1));
            Assert.IsTrue(temp1.Equals(temp2));
            Assert.IsFalse(temp1.Equals(temp3));
        }

        [TestMethod]
        public void TestEquals2()
        {
            UInt256 temp1 = new();
            object? temp2 = null;
            object temp3 = new();
            Assert.IsFalse(temp1.Equals(temp2));
            Assert.IsFalse(temp1.Equals(temp3));
        }

        [TestMethod]
        public void TestParse()
        {
            static void Action() => UInt256.Parse(null);
            Assert.ThrowsExactly<FormatException>(() => Action());
            var result = UInt256.Parse("0x0000000000000000000000000000000000000000000000000000000000000000");
            Assert.AreEqual(UInt256.Zero, result);
            static void Action1() => UInt256.Parse("000000000000000000000000000000000000000000000000000000000000000");
            Assert.ThrowsExactly<FormatException>(() => Action1());
            var result1 = UInt256.Parse("0000000000000000000000000000000000000000000000000000000000000000");
            Assert.AreEqual(UInt256.Zero, result1);
        }

        [TestMethod]
        public void TestTryParse()
        {
            Assert.IsFalse(UInt256.TryParse(null, out _));
            Assert.IsTrue(UInt256.TryParse("0x0000000000000000000000000000000000000000000000000000000000000000", out var temp));
            Assert.AreEqual(UInt256.Zero, temp);
            Assert.IsTrue(UInt256.TryParse("0x1230000000000000000000000000000000000000000000000000000000000000", out temp));
            Assert.AreEqual("0x1230000000000000000000000000000000000000000000000000000000000000", temp.ToString());
            Assert.IsFalse(UInt256.TryParse("000000000000000000000000000000000000000000000000000000000000000", out _));
            Assert.IsFalse(UInt256.TryParse("0xKK00000000000000000000000000000000000000000000000000000000000000", out _));
        }

        [TestMethod]
        public void TestOperatorEqual()
        {
            Assert.IsFalse(new UInt256() == null);
            Assert.IsFalse(null == new UInt256());
        }

        [TestMethod]
        public void TestOperatorLarger()
        {
            Assert.IsFalse(UInt256.Zero > UInt256.Zero);
            Assert.IsFalse(UInt256.Zero > "0x0000000000000000000000000000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorLargerAndEqual()
        {
            Assert.IsTrue(UInt256.Zero >= UInt256.Zero);
            Assert.IsTrue(UInt256.Zero >= "0x0000000000000000000000000000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorSmaller()
        {
            Assert.IsFalse(UInt256.Zero < UInt256.Zero);
            Assert.IsFalse(UInt256.Zero < "0x0000000000000000000000000000000000000000000000000000000000000000");
        }

        [TestMethod]
        public void TestOperatorSmallerAndEqual()
        {
            Assert.IsTrue(UInt256.Zero <= UInt256.Zero);
            Assert.IsTrue(UInt256.Zero <= "0x0000000000000000000000000000000000000000000000000000000000000000");
        }
    }
}
