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

using Neo.VM.Types;
using System;
using System.Numerics;

namespace Neo.VM.Tests.Types
{
    [TestClass]
    public sealed class UT_VMObject
    {
        [TestMethod]
        public void TestCircularReference()
        {
            var itemA = new VMStruct() { true, false };
            var itemB = new VMStruct() { true, false };
            var itemC = new VMStruct() { false, false };

            itemA[1] = itemA;
            itemB[1] = itemB;
            itemC[1] = itemC;

            Assert.IsTrue(itemA.HasCircularReference());
            Assert.IsTrue(itemB.HasCircularReference());
            Assert.IsTrue(itemC.HasCircularReference());
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            VMObject itemA = "NEO";
            VMObject itemB = "NEO";
            VMObject itemC = "SmartEconomy";

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new VMBuffer([1]);
            itemB = new VMBuffer([1]);
            itemC = new VMBuffer([2]);

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new byte[] { 1, 2, 3 };
            itemB = new byte[] { 1, 2, 3 };
            itemC = new byte[] { 5, 6 };

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = true;
            itemB = true;
            itemC = false;

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = 1;
            itemB = 1;
            itemC = 123;

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new VMNull();
            itemB = new VMNull();

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());

            itemA = new VMArray() { true, false, 0 };
            itemB = new VMArray() { true, false, 0 };
            itemC = new VMArray() { true, false, 1 };

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new VMStruct() { true, false, 0 };
            itemB = new VMStruct() { true, false, 0 };
            itemC = new VMStruct() { true, false, 1 };

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new VMMap() { [true] = false, [0] = 1 };
            itemB = new VMMap() { [true] = false, [0] = 1 };
            itemC = new VMMap() { [true] = false, [0] = 2 };

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            var junk = new VMArray() { true, false, 0 };
            itemA = new VMMap() { [true] = junk, [0] = junk };
            itemB = new VMMap() { [true] = junk, [0] = junk };
            itemC = new VMMap() { [true] = junk, [0] = 2 };

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            itemA = new VMInteropInterface(123);
            itemB = new VMInteropInterface(123);
            itemC = new VMInteropInterface(124);

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());

            byte[] script = [];
            itemA = new VMPointer(script, 123);
            itemB = new VMPointer(script, 123);
            itemC = new VMPointer(script, 1234);

            Assert.AreNotEqual(0, itemA.GetHashCode());
            Assert.AreNotEqual(0, itemB.GetHashCode());
            Assert.AreNotEqual(0, itemC.GetHashCode());

            Assert.AreEqual(itemB.GetHashCode(), itemA.GetHashCode());
            Assert.AreNotEqual(itemC.GetHashCode(), itemA.GetHashCode());
        }

        [TestMethod]
        public void TestNull()
        {
            VMObject nullItem = Array.Empty<byte>();

            Assert.AreNotEqual(VMNull.Instance, nullItem);
            Assert.AreEqual(VMNull.Instance, new VMNull());
        }

        [TestMethod]
        public void TestEqual()
        {
            VMObject itemA = "NEO";
            VMObject itemB = "NEO";
            VMObject itemC = "SmartEconomy";
            VMObject itemD = "Smarteconomy";
            VMObject itemE = "smarteconomy";

            Assert.IsTrue(itemA.Equals(itemB));
            Assert.IsFalse(itemA.Equals(itemC));
            Assert.IsFalse(itemC.Equals(itemD));
            Assert.IsFalse(itemD.Equals(itemE));
            Assert.IsFalse(itemA.Equals(new object()));
        }

        [TestMethod]
        public void TestCast()
        {
            // Signed byte

            VMObject item = sbyte.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(sbyte.MaxValue), item.GetInteger());

            // Unsigned byte

            item = byte.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(byte.MaxValue), item.GetInteger());

            // Signed short

            item = short.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(short.MaxValue), item.GetInteger());

            // Unsigned short

            item = ushort.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(ushort.MaxValue), item.GetInteger());

            // Signed integer

            item = int.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(int.MaxValue), item.GetInteger());

            // Unsigned integer

            item = uint.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(uint.MaxValue), item.GetInteger());

            // Signed long

            item = long.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(long.MaxValue), item.GetInteger());

            // Unsigned long

            item = ulong.MaxValue;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(ulong.MaxValue), item.GetInteger());

            // BigInteger

            item = BigInteger.MinusOne;

            Assert.IsInstanceOfType<VMInteger>(item);
            Assert.AreEqual(new BigInteger(-1), item.GetInteger());

            // Boolean

            item = true;

            Assert.IsInstanceOfType<VMBoolean>(item);
            Assert.IsTrue(item.GetBoolean());

            // Byte Array

            item = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };

            Assert.IsInstanceOfType<VMByteArray>(item);
            Assert.AreSequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, item.AsSpan().ToArray());
        }

        [TestMethod]
        public void TestCloneSelfReference()
        {
            var a = new VMArray()
            {
                true,
                1,
                new byte[] { 1 },
                VMNull.Instance,
                new VMBuffer([1]),
                new VMMap() { [0] = 1, [2] = 3 },
                new VMStruct() { 1, 2, 3 }
            };

            a.Add(a);

            var aa = (VMArray)a.Clone();

            Assert.AreSequenceEqual(a, aa);
            Assert.AreNotSame(a, aa);
            Assert.IsTrue(a[^2].Equals(aa[^2]));
            Assert.IsTrue(a.HasCircularReference());
            Assert.AreNotSame(a[^2], aa[^2]);
        }
    }
}
