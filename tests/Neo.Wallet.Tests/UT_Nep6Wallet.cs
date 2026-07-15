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

using Neo.Core;
using System;

namespace Neo.Wallet.Tests
{
    [TestClass]
    public class UT_Nep6Wallet
    {
        [TestMethod]
        public void TestCreateAccount()
        {
            var expectedWallet = TestDefaults.TestNep6WalletModel;
            var actualWallet = new Nep6Wallet();

            Assert.IsNotNull(expectedWallet.Accounts);

            foreach (var expectedAccountModel in expectedWallet.Accounts)
            {
                if (expectedAccountModel is null) continue;

                var expectedAccount = expectedAccountModel.ToObject();
                var actualAccount = actualWallet.CreateAccount(expectedAccountModel);

                // TODO: Add check for 'expectedAccountModel.Address == actualAccount.ScriptHash'
                //       currently this will not match until we change the SysCall method to use the
                //       right method address
                Assert.IsTrue(expectedAccount.VerifyPassword("abc123"));
                Assert.IsTrue(actualAccount.VerifyPassword("abc123"));

                Assert.AreEqual(expectedAccount.Label, actualAccount.Label);
                Assert.AreEqual(expectedAccount.ScriptHash, actualAccount.ScriptHash);
                Assert.AreEqual(expectedAccount.HasKey, actualAccount.HasKey);
                Assert.AreEqual(expectedAccount.Address, actualAccount.Address);
                Assert.AreEqual(expectedAccount.IsDefault, actualAccount.IsDefault);
                Assert.AreEqual(expectedAccount.IsLocked, actualAccount.IsLocked);
                Assert.AreEqual(expectedAccount.Contract.ScriptHash, actualAccount.Contract.ScriptHash);

                Assert.AreSequenceEqual(expectedAccount.Contract.Script, actualAccount.Contract.Script);
                Assert.AreSequenceEqual(expectedAccount.Contract.ParameterList, actualAccount.Contract.ParameterList);
                Assert.AreSequenceEqual(expectedAccount.GetPrivateKey(), actualAccount.GetPrivateKey());

                Assert.IsTrue(expectedAccount.ChangePassword("abc123", "pwd"));
                Assert.IsTrue(actualAccount.ChangePassword("abc123", "pwd"));
                Assert.IsFalse(actualAccount.ChangePassword("pwd", "pwd"));

                actualAccount.SetLock();

                Assert.IsTrue(actualAccount.IsLocked);
                Assert.ThrowsExactly<InvalidOperationException>(() => actualAccount.GetPrivateKey());
            }

            var actualNewAccount = actualWallet.CreateAccount(ProtocolSettings.Default);

            Assert.ThrowsExactly<InvalidOperationException>(() => actualNewAccount.VerifyPassword("12345"));
            Assert.IsTrue(actualNewAccount.ChangePassword(string.Empty, "12345"));
            Assert.IsTrue(actualNewAccount.VerifyPassword("12345"));
        }
    }
}
