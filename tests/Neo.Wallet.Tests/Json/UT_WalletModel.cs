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

using Neo.Configuration.Json;
using Neo.Wallet.Json;
using System.Linq;

namespace Neo.Wallet.Tests.Json
{
    [TestClass]
    public class UT_WalletModel
    {
        [TestMethod]
        public void TestPropertyValues()
        {
            var jsonTestString = "{\"name\":\"Unit Test Wallet\",\"version\":\"1.0\",\"scrypt\":{\"n\":16384,\"r\":8,\"p\":8},\"accounts\":[{\"address\":\"NhpxKrsHrFCizcupug2pTNkM7TnhyMzwEa\",\"label\":\"Main Test Account\",\"isDefault\":true,\"lock\":false,\"key\":\"3889c6201680d433ffdccb94a6b01c09863f5dc83aa85a003ae4dfe9460cf33f\",\"contract\":{\"script\":\"0c21028cd8520a4379f8bf84734fdc8063cc810932ae5f15d9d76362d7af35ca8371a84156e7b327\",\"parameters\":[{\"name\":\"Signature\",\"type\":\"Signature\"}],\"deployed\":false},\"extra\":null}],\"extra\":null}";

            var expectedTestWalletModel = TestDefaults.TestDevWalletModel;

            var actualTestWalletModel = JsonModel.FromJson<DevWalletModel>(jsonTestString, TestDefaults.JsonDefaultSerializerOptions);

            Assert.IsNotNull(actualTestWalletModel);

            Assert.IsNotNull(actualTestWalletModel.Name);
            Assert.AreEqual(expectedTestWalletModel.Name, actualTestWalletModel.Name);

            Assert.IsNotNull(actualTestWalletModel.Version);
            Assert.AreEqual(expectedTestWalletModel.Version, actualTestWalletModel.Version);

            Assert.IsNotNull(actualTestWalletModel.SCrypt);
            Assert.AreEqual(expectedTestWalletModel.SCrypt!.N, actualTestWalletModel.SCrypt.N);
            Assert.AreEqual(expectedTestWalletModel.SCrypt!.R, actualTestWalletModel.SCrypt.R);
            Assert.AreEqual(expectedTestWalletModel.SCrypt!.P, actualTestWalletModel.SCrypt.P);

            Assert.IsNotNull(actualTestWalletModel.Accounts);
            Assert.HasCount(1, actualTestWalletModel.Accounts);

            var expectedTestWalletAccountModel = expectedTestWalletModel.Accounts!.Single();
            var actualTestWalletAccountModel = actualTestWalletModel.Accounts.SingleOrDefault();

            Assert.IsNotNull(actualTestWalletAccountModel);

            Assert.IsNotNull(actualTestWalletAccountModel.Address);
            Assert.AreEqual(expectedTestWalletAccountModel.Address, actualTestWalletAccountModel.Address);

            Assert.IsNotNull(actualTestWalletAccountModel.Label);
            Assert.AreEqual(expectedTestWalletAccountModel.Label, actualTestWalletAccountModel.Label);
            Assert.AreEqual(expectedTestWalletAccountModel.IsDefault, actualTestWalletAccountModel.IsDefault);
            Assert.AreEqual(expectedTestWalletAccountModel.Lock, actualTestWalletAccountModel.Lock);

            Assert.IsNotNull(actualTestWalletAccountModel.Key);
            Assert.AreSequenceEqual(expectedTestWalletAccountModel.Key!, actualTestWalletAccountModel.Key);

            Assert.IsNotNull(actualTestWalletAccountModel.Contract);
            Assert.AreSequenceEqual(expectedTestWalletAccountModel.Contract!.Script, actualTestWalletAccountModel.Contract.Script);

            Assert.IsNotNull(actualTestWalletAccountModel.Contract.Parameters);
            Assert.HasCount(1, actualTestWalletAccountModel.Contract.Parameters);

            var expectedContractParametersModel = expectedTestWalletAccountModel.Contract.Parameters!.Single();
            var actualContractParametersModel = actualTestWalletAccountModel.Contract.Parameters.SingleOrDefault();

            Assert.IsNotNull(actualContractParametersModel);

            Assert.IsNotNull(actualContractParametersModel.Name);
            Assert.AreEqual(expectedContractParametersModel.Name, actualContractParametersModel.Name);
            Assert.AreEqual(expectedContractParametersModel.Type, actualContractParametersModel.Type);
        }
    }
}
