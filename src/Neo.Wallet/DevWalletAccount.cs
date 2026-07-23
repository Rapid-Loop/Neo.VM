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
using Neo.Wallet.Json;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Neo.Wallet
{
    /// <summary>
    /// A development wallet account that keeps the private key in memory without password encryption.
    /// </summary>
    public class DevWalletAccount : IWalletAccount<ProtocolSettings>, IMap<DevWalletAccountModel>
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

        /// <summary>
        /// Gets or sets a value indicating whether the account is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <inheritdoc/>
        public bool HasKey => _privateKeyBytes.Length > 0;

        /// <summary>
        /// Gets the protocol settings stored as account extras.
        /// </summary>
        public ProtocolSettings Extra => ProtocolConfiguration;

        /// <inheritdoc/>
        public WitnessContract Contract => _witnessContract;

        private readonly ProtocolSettings _protocolSettings;
        private readonly WitnessContract _witnessContract;

        private readonly byte[] _privateKeyBytes = [];

        /// <summary>
        /// Initializes a multi-signature account using a default <c>m</c> threshold of roughly two-thirds of the public keys.
        /// </summary>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        public DevWalletAccount(ECPoint[] publicKeys, ProtocolSettings protocolSettings) : this((int)Math.Ceiling((2 * publicKeys.Length + 1) / 3m), publicKeys, protocolSettings) { }

        /// <summary>
        /// Initializes a multi-signature account that requires <paramref name="m"/> of the given public keys.
        /// </summary>
        /// <param name="m">The minimum number of signatures required.</param>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        public DevWalletAccount(int m, ECPoint[] publicKeys, ProtocolSettings protocolSettings)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(publicKeys.Length, 0, nameof(publicKeys));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(m, publicKeys.Length, nameof(publicKeys));
            ArgumentOutOfRangeException.ThrowIfLessThan(m, 1, nameof(m));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(m, 1024, nameof(m));

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
        public DevWalletAccount(UInt160 contractHash, ProtocolSettings protocolSettings, MethodParameterType[]? contractParameters = default, byte[]? privateKeyBytes = default)
        {
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
        public DevWalletAccount(byte[] privateKeyBytes, ProtocolSettings protocolSettings)
        {
            _privateKeyBytes = privateKeyBytes;
            _protocolSettings = protocolSettings;

            var privateKeySpan = privateKeyBytes.AsSpan();
            var publicKeyPoint = ECPoint.FromPrivateKey(privateKeySpan, ECCurve.SecP256r1);

            _witnessContract = WitnessContract.CreateSignatureContract(publicKeyPoint);
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"{ToObject()}";

        /// <summary>
        /// Not supported for development accounts.
        /// </summary>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        [DoesNotReturn]
        public bool ChangePassword(ProtectedString oldPassword, ProtectedString newPassword)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported for development accounts.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        [DoesNotReturn]
        public bool VerifyPassword(ProtectedString password)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported for development accounts.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        [DoesNotReturn]
        public void SetLock()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a copy of the account private key.
        /// </summary>
        /// <returns>The private key bytes, or an empty array when no key is stored.</returns>
        public byte[] GetPrivateKey() =>
            _privateKeyBytes[..];

        /// <summary>
        /// Converts this account to its JSON model representation.
        /// </summary>
        /// <returns>A <see cref="DevWalletAccountModel"/> that mirrors the current account state.</returns>
        public DevWalletAccountModel ToObject() =>
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
                Key = _privateKeyBytes[..],
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
