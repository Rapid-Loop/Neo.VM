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

namespace Neo.Wallet
{
    /// <summary>
    /// Defines a single account within a Neo wallet.
    /// </summary>
    /// <typeparam name="TExtras">The type of account-level extra data.</typeparam>
    public interface IWalletAccount<TExtras>
        where TExtras : class?, new()
    {
        /// <summary>
        /// Gets the protocol settings associated with this account.
        /// </summary>
        ProtocolSettings ProtocolConfiguration { get; }

        /// <summary>
        /// Gets the script hash of the account.
        /// </summary>
        UInt160 ScriptHash { get; }

        /// <summary>
        /// Gets the base58-check address of the account.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets an optional human-readable label for the account.
        /// </summary>
        string? Label { get; }

        /// <summary>
        /// Gets a value indicating whether this is the default account in the wallet.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// Gets a value indicating whether the account is locked.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Gets a value indicating whether a private key is available for this account.
        /// </summary>
        bool HasKey { get; }

        /// <summary>
        /// Gets optional account-level extra data.
        /// </summary>
        TExtras Extra { get; }

        /// <summary>
        /// Gets the witness contract used to authorize the account.
        /// </summary>
        WitnessContract Contract { get; }

        /// <summary>
        /// Changes the password used to protect the account private key.
        /// </summary>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns><see langword="true"/> if the password was changed; otherwise, <see langword="false"/>.</returns>
        bool ChangePassword(ProtectedString oldPassword, ProtectedString newPassword);

        /// <summary>
        /// Verifies the password and unlocks the account when successful.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <returns><see langword="true"/> if the password is valid; otherwise, <see langword="false"/>.</returns>
        bool VerifyPassword(ProtectedString password);

        /// <summary>
        /// Gets a copy of the account private key.
        /// </summary>
        /// <returns>The private key bytes.</returns>
        byte[] GetPrivateKey();

        /// <summary>
        /// Locks the account so that the private key cannot be retrieved until unlocked.
        /// </summary>
        void SetLock();
    }
}
