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

using Neo.Core.Cryptography.ECC;
using System;

namespace Neo.Core.Tests.Cryptography.ECC
{
    [TestClass]
    public class UT_ECCurve
    {
        [TestMethod]
        public void TestSecP256r1CompressionPoint()
        {
            var expectedUncompressedPublicKeyString =
                "04"
                + "6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"
                + "4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5";
            var expectedUncompressedPublicKeyBytes = Convert.FromHexString(expectedUncompressedPublicKeyString);

            var expectedCompressedPublicKeyString = "036b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296";
            var expectedCompressedPublicKeyBytes = Convert.FromHexString(expectedCompressedPublicKeyString);

            var actualCompressedPublicKeyBytes = ECCurve.CompressPoint(expectedUncompressedPublicKeyBytes);
            var actualCompressedPublicKeyString = Convert.ToHexStringLower(actualCompressedPublicKeyBytes);

            var actualUncompressedPublicKeyBytes = ECCurve.SecP256r1.DecompressPoint(expectedCompressedPublicKeyBytes);
            var actualUncompressedPublicKeyString = Convert.ToHexStringLower(actualUncompressedPublicKeyBytes);

            Assert.AreEqual(expectedCompressedPublicKeyString, actualCompressedPublicKeyString);
            Assert.AreEqual(expectedUncompressedPublicKeyString, actualUncompressedPublicKeyString);
        }
    }
}
