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

namespace Neo.Platform.Storage.Tests
{
    [TestClass]
    public class UT_StorageKeyBuilder
    {
        private const int ExpectedId = 0x0badc0de;
        private const byte ExpectedPrefix = 0x7f;


        [TestMethod]
        public void TestEquals()
        {
            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            using var actualKeyBuilder1 = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            using var actualKeyBuilder2 = expectedKeyBuilder.Clone();
            using var actualKeyBuilder3 = StorageKeyBuilder.Create(-1, 0xaa);

            Assert.IsTrue(expectedKeyBuilder.Equals(actualKeyBuilder1 as object));
            Assert.IsTrue(expectedKeyBuilder.Equals(actualKeyBuilder2 as object));
            Assert.IsFalse(expectedKeyBuilder.Equals(actualKeyBuilder3 as object));

            Assert.IsTrue(expectedKeyBuilder.Equals(actualKeyBuilder1));
            Assert.IsTrue(expectedKeyBuilder.Equals(actualKeyBuilder2));
            Assert.IsFalse(expectedKeyBuilder.Equals(actualKeyBuilder3));

            Assert.IsTrue(expectedKeyBuilder == actualKeyBuilder1);
            Assert.IsTrue(expectedKeyBuilder == actualKeyBuilder2);
            Assert.IsFalse(expectedKeyBuilder == actualKeyBuilder3);

            Assert.IsFalse(expectedKeyBuilder != actualKeyBuilder1);
            Assert.IsFalse(expectedKeyBuilder != actualKeyBuilder2);
            Assert.IsTrue(expectedKeyBuilder != actualKeyBuilder3);
        }

        [TestMethod]
        public void TestCompareTo()
        {
            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            using var actualKeyBuilder1 = expectedKeyBuilder.Clone();
            using var actualKeyBuilder2 = StorageKeyBuilder.Create(-1, 0xaa);

            Assert.AreEqual(0, expectedKeyBuilder.CompareTo(actualKeyBuilder1));
            Assert.AreEqual(0, expectedKeyBuilder.CompareTo(actualKeyBuilder1 as object));

            Assert.IsLessThan(0, expectedKeyBuilder.CompareTo(actualKeyBuilder2));
            Assert.IsLessThan(0, expectedKeyBuilder.CompareTo(actualKeyBuilder2 as object));
        }

        [TestMethod]
        public void TestHashCode()
        {
            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            using var actualKeyBuilder1 = expectedKeyBuilder.Clone();
            using var actualKeyBuilder2 = StorageKeyBuilder.Create(-1, 0xaa);

            Assert.AreNotEqual(0, expectedKeyBuilder.GetHashCode());
            Assert.AreNotEqual(0, actualKeyBuilder1.GetHashCode());
            Assert.AreNotEqual(0, actualKeyBuilder2.GetHashCode());

            Assert.AreEqual(expectedKeyBuilder.GetHashCode(), actualKeyBuilder1.GetHashCode());
            Assert.AreNotEqual(expectedKeyBuilder.GetHashCode(), actualKeyBuilder2.GetHashCode());
            Assert.AreNotEqual(actualKeyBuilder1.GetHashCode(), actualKeyBuilder2.GetHashCode());
        }

        [TestMethod]
        public void TestConcatenation()
        {
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                ];

            expectedKeyBytes = [.. expectedKeyBytes, .. expectedKeyBytes];

            using var expectedKeyBuilder1 = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            using var expectedKeyBuilder2 = expectedKeyBuilder1.Clone();
            using var actualKeyBuilder1 = expectedKeyBuilder1 + expectedKeyBuilder2;

            var actualKeyBytes = actualKeyBuilder1.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
            Assert.IsFalse(expectedKeyBuilder1.Equals(actualKeyBuilder1));
            Assert.IsFalse(expectedKeyBuilder2.Equals(actualKeyBuilder1));
        }

        [TestMethod]
        public void TestCreate()
        {
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendByte()
        {
            byte expectedAppendedByte = 0x80;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    expectedAppendedByte,   // Appended byte
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedByte);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendSByte()
        {
            var expectedAppendedSByte = sbyte.MinValue;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b,     // ID
                    ExpectedPrefix,             // Prefix
                    (byte)expectedAppendedSByte, // Appended sbyte
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedSByte);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendInt16()
        {
            var expectedAppendedInt16 = (short)0x0bad;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0xad, 0x0b,             // Appended int16
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedInt16);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendUInt16()
        {
            var expectedAppendedUInt16 = (ushort)0xdead;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0xad, 0xde,             // Appended uint16
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedUInt16);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendInt32()
        {
            var expectedAppendedInt32 = 0xc0dea55;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0x55, 0xea, 0x0d, 0x0c, // Appended int32
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedInt32);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendUInt32()
        {
            var expectedAppendedUInt32 = 0xc0dea55u;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0x55, 0xea, 0x0d, 0x0c, // Appended uint32
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedUInt32);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendInt64()
        {
            var expectedAppendedInt64 = 0x000d0dec0debad12;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0x12, 0xad, 0xeb, 0x0d, 0xec, 0x0d, 0x0d, 0x00, // Appended int64
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedInt64);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendUInt64()
        {
            var expectedAppendedUInt64 = 0x000d0dec0debad12u;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0x12, 0xad, 0xeb, 0x0d, 0xec, 0x0d, 0x0d, 0x00, // Appended uint64
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedUInt64);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestAppendBoolean()
        {
            var expectedAppendedBoolean = true;
            byte[] expectedKeyBytes =
                [
                    0xde, 0xc0, 0xad, 0x0b, // ID
                    ExpectedPrefix,         // Prefix
                    0x01,                   // Appended boolean
                ];

            using var expectedKeyBuilder = StorageKeyBuilder
                .Create(ExpectedId, ExpectedPrefix)
                .Append(expectedAppendedBoolean);

            var actualKeyBytes = expectedKeyBuilder.ToArray();

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }
    }
}
