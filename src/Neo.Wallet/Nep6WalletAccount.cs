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
using Neo.Core.Cryptography;
using Neo.Core.Cryptography.ECC;
using Neo.Core.Extensions;
using Neo.Core.Interfaces;
using Neo.Core.VM.SmartContract;
using Neo.Wallet.Cryptography;
using Neo.Wallet.Json;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Neo.Wallet
{
    /// <summary>
    /// A NEP-6 wallet account that can protect its private key with a password using NEP-2 encryption.
    /// </summary>
    public class Nep6WalletAccount : IWalletAccount<ProtocolSettings>, IMap<Nep6WalletAccountModel>
    {
        /// <inheritdoc/>
        public ProtocolSettings ProtocolConfiguration => _protocolSettings;

        /// <inheritdoc/>
        public UInt160 ScriptHash => Contract.ScriptHash;

        /// <inheritdoc/>
        public string Address => ScriptHash.ToAddress(_protocolSettings.AddressVersion);

        /// <summary>
        /// Gets or sets an optional human-readable label for the account.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default account in the wallet.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <inheritdoc/>
        public bool IsLocked => _isLocked;

        /// <inheritdoc/>
        public bool HasKey => _privateKeyBytes.Length > 0;

        /// <summary>
        /// Gets the protocol settings stored as account extras.
        /// </summary>
        public ProtocolSettings Extra => ProtocolConfiguration;

        /// <inheritdoc/>
        public WitnessContract Contract => _witnessContract;

        private readonly ProtocolSettings _protocolSettings;

        private readonly ScryptParameters _scryptParameters;

        private byte[] _privateKeyBytes = [];
        private bool _isLocked;

        private WitnessContract _witnessContract;
        private ProtectedString _password = string.Empty;
        private ProtectedString _nep2String = string.Empty;

        /// <summary>
        /// Initializes a multi-signature account using a default <c>m</c> threshold of roughly two-thirds of the public keys.
        /// </summary>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 encryption; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(ECPoint[] publicKeys, ProtocolSettings protocolSettings, ScryptParameters? scryptParameters = default) : this((int)Math.Ceiling((2 * publicKeys.Length + 1) / 3m), publicKeys, protocolSettings, scryptParameters) { }

        /// <summary>
        /// Initializes a multi-signature account that requires <paramref name="m"/> of the given public keys.
        /// </summary>
        /// <param name="m">The minimum number of signatures required.</param>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 encryption; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(int m, ECPoint[] publicKeys, ProtocolSettings protocolSettings, ScryptParameters? scryptParameters = default)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(publicKeys.Length, 0, nameof(publicKeys));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(m, publicKeys.Length, nameof(publicKeys));
            ArgumentOutOfRangeException.ThrowIfLessThan(m, 1, nameof(m));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(m, 1024, nameof(m));

            _scryptParameters = scryptParameters ?? ScryptParameters.Default;
            _protocolSettings = protocolSettings;
            _witnessContract = WitnessContract.CreateMultiSigContract(m, publicKeys);
        }

        /// <summary>
        /// Initializes a contract or watch-only account for the specified script hash.
        /// </summary>
        /// <param name="contractHash">The contract script hash.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="contractParameters">Optional witness parameter types.</param>
        /// <param name="privateKeyBytes">Optional private key associated with the account.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 encryption; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(UInt160 contractHash, ProtocolSettings protocolSettings, MethodParameterType[]? contractParameters = default, byte[]? privateKeyBytes = default, ScryptParameters? scryptParameters = default)
        {
            _scryptParameters = scryptParameters ?? ScryptParameters.Default;
            _protocolSettings = protocolSettings;
            _witnessContract = WitnessContract.Create(contractHash, contractParameters ?? []);

            if (privateKeyBytes is not null)
                _privateKeyBytes = privateKeyBytes;
        }

        /// <summary>
        /// Initializes a signature account from the specified private key.
        /// </summary>
        /// <param name="privateKeyBytes">The private key bytes.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 encryption; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(byte[] privateKeyBytes, ProtocolSettings protocolSettings, ScryptParameters? scryptParameters = default)
        {
            _privateKeyBytes = privateKeyBytes;
            _scryptParameters = scryptParameters ?? ScryptParameters.Default;
            _protocolSettings = protocolSettings;

            var privateKeySpan = _privateKeyBytes.AsSpan();
            var publicKeyPoint = ECPoint.FromPrivateKey(privateKeySpan, ECCurve.SecP256r1);

            _witnessContract = WitnessContract.CreateSignatureContract(publicKeyPoint);
        }

        /// <summary>
        /// Initializes a signature account by decrypting a NEP-2 key with the specified password.
        /// </summary>
        /// <param name="nep2String">The NEP-2 encoded private key.</param>
        /// <param name="password">The password used to decrypt the key.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 decryption; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(string nep2String, string password, ProtocolSettings protocolSettings, ScryptParameters? scryptParameters = default)
        {
            _nep2String = nep2String;
            _password = password;
            _protocolSettings = protocolSettings;
            _scryptParameters = scryptParameters ?? ScryptParameters.Default;

            _privateKeyBytes = ChainWallet.GetKeyFromNep2String(nep2String, _password, _scryptParameters, _protocolSettings.AddressVersion);

            var privateKeySpan = _privateKeyBytes.AsSpan();
            var publicKeyPoint = ECPoint.FromPrivateKey(privateKeySpan, ECCurve.SecP256r1);

            _witnessContract = WitnessContract.CreateSignatureContract(publicKeyPoint);
        }

        /// <summary>
        /// Initializes a locked account from a NEP-6 account model.
        /// </summary>
        /// <param name="accountModel">The account model to import.</param>
        /// <param name="scryptParameters">The SCrypt parameters used for NEP-2 operations; defaults to <see cref="ScryptParameters.Default"/>.</param>
        public Nep6WalletAccount(Nep6WalletAccountModel accountModel, ScryptParameters? scryptParameters = default)
        {
            _nep2String = Encoding.UTF8.GetString(accountModel.Key ?? []);
            _protocolSettings = accountModel.Extra?.ToObject() ?? ProtocolSettings.Default;
            _scryptParameters = scryptParameters ?? ScryptParameters.Default;
            _witnessContract = accountModel.Contract?.ToObject() ?? WitnessContract.Create([], []);
            _isLocked = true;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"{ToObject()}";

        /// <summary>
        /// Changes the password used to protect the account private key and re-encrypts it as NEP-2.
        /// </summary>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns><see langword="true"/> if the password was changed; otherwise, <see langword="false"/>.</returns>
        public bool ChangePassword(ProtectedString oldPassword, ProtectedString newPassword)
        {
            if (_isLocked) return false;
            if (_privateKeyBytes.Length == 0) return false;
            if (string.IsNullOrEmpty(newPassword)) return false;
            if (oldPassword == newPassword) return false;
            if (_password != oldPassword) return false;

            _password.Dispose();
            _password = newPassword;

            _nep2String = ChainWallet.ToNep2String(
                _privateKeyBytes,
                _password,
                _scryptParameters,
                _protocolSettings.AddressVersion
            );

            _isLocked = true;

            return true;
        }

        /// <summary>
        /// Verifies the password against the stored NEP-2 key and unlocks the account when successful.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <returns><see langword="true"/> if the password is valid and the account was unlocked; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no NEP-2 key has been set.</exception>
        public bool VerifyPassword(ProtectedString password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (string.IsNullOrEmpty(_nep2String))
                throw new InvalidOperationException($"Password is not set. Call {nameof(ChangePassword)} first.");

            var testKeyBytes = ChainWallet.GetKeyFromNep2String(
                    _nep2String,
                    password,
                    _scryptParameters,
                    _protocolSettings.AddressVersion);

            _password = password;
            _privateKeyBytes = testKeyBytes;
            _isLocked = false;

            var privateKeySpan = _privateKeyBytes.AsSpan();
            var publicKeyPoint = ECPoint.FromPrivateKey(privateKeySpan, ECCurve.SecP256r1);
            _witnessContract = WitnessContract.CreateSignatureContract(publicKeyPoint);

            return true;
        }

        /// <summary>
        /// Locks the account so that the private key cannot be retrieved until unlocked.
        /// </summary>
        public void SetLock() =>
            _isLocked = true;

        /// <summary>
        /// Gets a copy of the account private key.
        /// </summary>
        /// <returns>The private key bytes.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the account is locked or has no private key.</exception>
        public byte[] GetPrivateKey()
        {
            if (_isLocked == true || _privateKeyBytes.Length == 0)
                throw new InvalidOperationException($"Account is locked. Call {nameof(VerifyPassword)} first.");

            return _privateKeyBytes[..];
        }

        /// <summary>
        /// Converts this account to its JSON model representation.
        /// </summary>
        /// <returns>A <see cref="Nep6WalletAccountModel"/> that mirrors the current account state.</returns>
        public Nep6WalletAccountModel ToObject() =>
            new()
            {
                Address = Address,
                Contract = new()
                {
                    Deployed = HasKey,
                    Script = _witnessContract.Script,
                    Parameters = [
                        .. _witnessContract.ParameterList
                            .Select(static (s, i) =>
                                new ContractParameterModel()
                                {
                                    Name = $"Parameter{i}",
                                    Type = s,
                                }
                            ),
                        ],
                },
                Key = Encoding.UTF8.GetBytes(_nep2String),
                Label = Label,
                IsDefault = IsDefault,
                Lock = IsLocked,
                Extra = new()
                {
                    Network = Extra.Network,
                    AddressVersion = Extra.AddressVersion,
                    MillisecondsPerBlock = Extra.MillisecondsPerBlock,
                    MaxTransactionsPerBlock = Extra.MaxTransactionsPerBlock,
                    MemoryPoolMaxTransactions = Extra.MemoryPoolMaxTransactions,
                    MaxTraceableBlocks = Extra.MaxTraceableBlocks,
                    InitialGasDistribution = Extra.InitialGasDistribution,
                    ValidatorsCount = Extra.ValidatorsCount,
                    SeedList = [.. Extra.SeedList],
                    HardForks = Extra.HardForks.ToImmutableDictionary(),
                    StandbyCommittee = [.. Extra.StandbyCommittee],
                },
            };
    }
}
