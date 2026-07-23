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
using Neo.Core.Blockchain;
using Neo.Core.Cryptography.ECC;
using Neo.Core.Extensions;
using Neo.Core.Interfaces;
using Neo.Wallet.Cryptography;
using Neo.Wallet.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace Neo.Wallet
{
    /// <summary>
    /// An in-memory development wallet that stores private keys without NEP-2 encryption.
    /// </summary>
    public class DevWallet : IWallet<object, ProtocolSettings>, IMap<DevWalletModel>
    {
        private static readonly Version s_walletVersion = new(1, 0);

        /// <inheritdoc/>
        public Version Version => s_walletVersion;

        /// <inheritdoc/>
        public ScryptParameters SCrypt => _scryptParameters;

        /// <summary>
        /// Gets or sets the optional display name of the wallet.
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public object? Extra => null;

        private readonly Dictionary<UInt160, DevWalletAccount> _walletAccounts = [];
        private readonly ScryptParameters _scryptParameters;

        /// <summary>
        /// Initializes a new empty development wallet with default SCrypt parameters.
        /// </summary>
        public DevWallet()
        {
            _scryptParameters = ScryptParameters.Default;
        }

        /// <summary>
        /// Initializes a development wallet from a JSON model.
        /// </summary>
        /// <param name="devWalletModel">The model containing wallet data and accounts.</param>
        public DevWallet(DevWalletModel devWalletModel) : this()
        {
            _scryptParameters = devWalletModel.SCrypt?.ToObject() ?? ScryptParameters.Default;

            if (devWalletModel.Accounts is not null)
            {
                foreach (var account in devWalletModel.Accounts
                    .Select(s => ((DevWalletAccountModel)s).ToObject()))
                    _walletAccounts[account.ScriptHash] = account;
            }
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"{ToObject()}";

        /// <inheritdoc/>
        public bool Contains(UInt160 scriptHash) =>
            _walletAccounts.ContainsKey(scriptHash);

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateAccount(ProtocolSettings protocolSettings) =>
            CreateAccount(RandomNumberGenerator.GetBytes(ChainWallet.MaxPrivateKeySizeInBytes), protocolSettings);

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateAccount(byte[] privateKeyBytes, ProtocolSettings protocolSettings)
        {
            var newWalletAccount = new DevWalletAccount(privateKeyBytes, protocolSettings);

            if (Contains(newWalletAccount.ScriptHash))
                throw new InvalidOperationException();

            return _walletAccounts[newWalletAccount.ScriptHash] = newWalletAccount;
        }

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateAccount(string wifString, ProtocolSettings protocolSettings) =>
            CreateAccount(ChainWallet.GetKeyFromWifString(wifString), protocolSettings);

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateAccount(UInt160 contractHash, ProtocolSettings protocolSettings)
        {
            var newWalletAccount = new DevWalletAccount(contractHash, protocolSettings);

            if (Contains(newWalletAccount.ScriptHash))
                throw new InvalidOperationException();

            return _walletAccounts[newWalletAccount.ScriptHash] = newWalletAccount;
        }

        /// <summary>
        /// Creates an account from a development wallet account model and adds it to the wallet.
        /// </summary>
        /// <param name="walletAccountModel">The account model to import.</param>
        /// <returns>The created wallet account.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the model address is <see langword="null"/>.</exception>
        public IWalletAccount<ProtocolSettings> CreateAccount(DevWalletAccountModel walletAccountModel)
        {
            if (walletAccountModel.Address is null)
                throw new InvalidOperationException();

            return _walletAccounts[walletAccountModel.Address] = walletAccountModel.ToObject();
        }

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateMultiSigAccount(ProtocolSettings protocolSettings, params ECPoint[] publicKeys) =>
            CreateMultiSigAccount(protocolSettings, (int)Math.Ceiling((2 * publicKeys.Length + 1) / 3m), publicKeys);

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> CreateMultiSigAccount(ProtocolSettings protocolSettings, int m, params ECPoint[] publicKeys)
        {
            var newWalletAccount = new DevWalletAccount(m, publicKeys, protocolSettings);

            if (Contains(newWalletAccount.ScriptHash))
                throw new InvalidOperationException();

            return _walletAccounts[newWalletAccount.ScriptHash] = newWalletAccount;
        }

        /// <inheritdoc/>
        public bool RemoveAccount(UInt160 scriptHash) =>
            _walletAccounts.Remove(scriptHash);

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> GetAccount(UInt160 scriptHash) =>
            _walletAccounts[scriptHash];

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> GetAccount(ECPoint publicKey) =>
            _walletAccounts[publicKey.Encode(true).ToScriptHash()];

        /// <inheritdoc/>
        public IEnumerable<IWalletAccount<ProtocolSettings>> GetAccounts() =>
            _walletAccounts.Values;

        /// <inheritdoc/>
        public IEnumerable<IWalletAccount<ProtocolSettings>> GetNetworkAccounts(uint network) =>
            _walletAccounts.Values.Where(s => s.ProtocolConfiguration.Network == network);

        /// <inheritdoc/>
        public IEnumerable<IWalletAccount<ProtocolSettings>> GetContractAccounts() =>
            _walletAccounts.Values.Where(
                static s =>
                    s.Contract.Script.Length == 0
            );

        /// <inheritdoc/>
        public IWalletAccount<ProtocolSettings> GetDefaultAccount() =>
            _walletAccounts.Values.Single(static s => s.IsDefault);

        /// <inheritdoc/>
        public IEnumerable<IWalletAccount<ProtocolSettings>> GetMultiSigAccounts() =>
            _walletAccounts.Values.Where(
                static w =>
                    WitnessContract.IsMultiSigContract(w.Contract.Script)
            );

        /// <inheritdoc/>
        public void SetDefaultAccount(UInt160 scriptHash)
        {
            if (Contains(scriptHash))
                throw new KeyNotFoundException(nameof(scriptHash));

            var defaultWalletAccounts = _walletAccounts.Values.Where(s => s.IsDefault);

            foreach (var account in defaultWalletAccounts)
                account.IsDefault = false;

            _walletAccounts[scriptHash].IsDefault = true;
        }

        /// <summary>
        /// Converts this wallet to its JSON model representation.
        /// </summary>
        /// <returns>A <see cref="DevWalletModel"/> that mirrors the current wallet state.</returns>
        public DevWalletModel ToObject() =>
            new()
            {
                Version = Version,
                Name = Name,
                SCrypt = SCryptModel.Default,
                Extra = Extra,
                Accounts = [.. _walletAccounts.Values.Select(static s => s.ToObject())],
            };
    }
}
