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
using Neo.Core.Cryptography.ECC;
using Neo.Wallet.Cryptography;
using System;
using System.Collections.Generic;

namespace Neo.Wallet
{
    /// <summary>
    /// Defines a Neo wallet that manages accounts and optional wallet-level extras.
    /// </summary>
    /// <typeparam name="TExtras">The type of wallet-level extra data.</typeparam>
    /// <typeparam name="TAccountExtras">The type of account-level extra data.</typeparam>
    public interface IWallet<TExtras, TAccountExtras>
        where TExtras : class?, new()
        where TAccountExtras : class?, new()
    {
        /// <summary>
        /// Gets the wallet format version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the optional display name of the wallet.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets the SCrypt parameters used for key derivation.
        /// </summary>
        ScryptParameters SCrypt { get; }

        /// <summary>
        /// Gets optional wallet-level extra data.
        /// </summary>
        TExtras? Extra { get; }

        /// <summary>
        /// Determines whether the wallet contains an account with the specified script hash.
        /// </summary>
        /// <param name="scriptHash">The account script hash to look up.</param>
        /// <returns><see langword="true"/> if the account exists; otherwise, <see langword="false"/>.</returns>
        bool Contains(UInt160 scriptHash);

        /// <summary>
        /// Creates a new account with a randomly generated private key.
        /// </summary>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <returns>The created wallet account.</returns>
        IWalletAccount<TAccountExtras> CreateAccount(ProtocolSettings protocolSettings);

        /// <summary>
        /// Creates a new account from the specified private key.
        /// </summary>
        /// <param name="privateKeyBytes">The private key bytes.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <returns>The created wallet account.</returns>
        IWalletAccount<TAccountExtras> CreateAccount(byte[] privateKeyBytes, ProtocolSettings protocolSettings);

        /// <summary>
        /// Creates a new account from a WIF-encoded private key.
        /// </summary>
        /// <param name="wifString">The WIF string of the private key.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <returns>The created wallet account.</returns>
        IWalletAccount<TAccountExtras> CreateAccount(string wifString, ProtocolSettings protocolSettings);

        /// <summary>
        /// Creates a new watch-only or contract account for the specified script hash.
        /// </summary>
        /// <param name="contractHash">The contract script hash.</param>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <returns>The created wallet account.</returns>
        IWalletAccount<TAccountExtras> CreateAccount(UInt160 contractHash, ProtocolSettings protocolSettings);

        /// <summary>
        /// Removes the account with the specified script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the account to remove.</param>
        /// <returns><see langword="true"/> if the account was removed; otherwise, <see langword="false"/>.</returns>
        bool RemoveAccount(UInt160 scriptHash);

        /// <summary>
        /// Sets the account with the specified script hash as the default account.
        /// </summary>
        /// <param name="scriptHash">The script hash of the account to mark as default.</param>
        void SetDefaultAccount(UInt160 scriptHash);

        /// <summary>
        /// Gets the default account in the wallet.
        /// </summary>
        /// <returns>The default wallet account.</returns>
        IWalletAccount<TAccountExtras> GetDefaultAccount();

        /// <summary>
        /// Gets the account with the specified script hash.
        /// </summary>
        /// <param name="scriptHash">The account script hash.</param>
        /// <returns>The matching wallet account.</returns>
        IWalletAccount<TAccountExtras> GetAccount(UInt160 scriptHash);

        /// <summary>
        /// Gets the account associated with the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <returns>The matching wallet account.</returns>
        IWalletAccount<TAccountExtras> GetAccount(ECPoint publicKey);

        /// <summary>
        /// Gets all accounts in the wallet.
        /// </summary>
        /// <returns>An enumerable of all wallet accounts.</returns>
        IEnumerable<IWalletAccount<TAccountExtras>> GetAccounts();

        /// <summary>
        /// Gets accounts that belong to the specified network.
        /// </summary>
        /// <param name="network">The network magic number.</param>
        /// <returns>An enumerable of accounts for the network.</returns>
        IEnumerable<IWalletAccount<TAccountExtras>> GetNetworkAccounts(uint network);

        /// <summary>
        /// Gets accounts that represent deployed contracts (empty witness scripts).
        /// </summary>
        /// <returns>An enumerable of contract accounts.</returns>
        IEnumerable<IWalletAccount<TAccountExtras>> GetContractAccounts();

        /// <summary>
        /// Gets accounts that use multi-signature contracts.
        /// </summary>
        /// <returns>An enumerable of multi-signature accounts.</returns>
        IEnumerable<IWalletAccount<TAccountExtras>> GetMultiSigAccounts();

        /// <summary>
        /// Creates a multi-signature account using a default <c>m</c> threshold of roughly two-thirds of the public keys.
        /// </summary>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <returns>The created multi-signature account.</returns>
        IWalletAccount<TAccountExtras> CreateMultiSigAccount(ProtocolSettings protocolSettings, params ECPoint[] publicKeys);

        /// <summary>
        /// Creates a multi-signature account that requires <paramref name="m"/> of the given public keys.
        /// </summary>
        /// <param name="protocolSettings">The protocol settings used for the account.</param>
        /// <param name="m">The minimum number of signatures required.</param>
        /// <param name="publicKeys">The public keys that participate in the multi-signature contract.</param>
        /// <returns>The created multi-signature account.</returns>
        IWalletAccount<TAccountExtras> CreateMultiSigAccount(ProtocolSettings protocolSettings, int m, params ECPoint[] publicKeys);
    }
}
