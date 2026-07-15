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

using Neo.Wallet.Cryptography;
using System;

namespace Neo.Wallet.Tests
{
    [TestClass]
    public class UT_ChainWallet
    {
        [TestMethod]
        public void TestGetKeyFromWifString()
        {
            var expectedKeyBytes = Convert.FromHexString("c7134d6fd8e73d819e82755c64c93788d8db0961929e025a53363c4cc02a6962");

            var action = () => ChainWallet.GetKeyFromWifString(string.Empty);
            Assert.ThrowsExactly<ArgumentException>(action);

            action = () => ChainWallet.GetKeyFromWifString("3vQB7B6MrGQZaxCuFg4oh");
            Assert.ThrowsExactly<FormatException>(action);

            var actualKeyBytes = ChainWallet.GetKeyFromWifString("L3tgppXLgdaeqSGSFw1Go3skBiy8vQAM7YMXvTHsKQtE16PBncSU");

            Assert.AreSequenceEqual(expectedKeyBytes, actualKeyBytes);
        }

        [TestMethod]
        public void TestNep2String()
        {
            var defaultSCryptParameters = new ScryptParameters(2, 1, 1);
            var expectedPrivateKeyBytes = ChainWallet.GetKeyFromWifString("Ky7cYncUA92kWnh7xymshpfgz7QiX46qPWCQBQPVUSv5vndE2VTR");

            var actualNep2String = ChainWallet.ToNep2String(expectedPrivateKeyBytes, "pwd", defaultSCryptParameters);
            var actualPrivateKeyBytes = ChainWallet.GetKeyFromNep2String(actualNep2String, "pwd", defaultSCryptParameters);

            Assert.AreSequenceEqual(expectedPrivateKeyBytes, actualPrivateKeyBytes);
        }
    }
}
