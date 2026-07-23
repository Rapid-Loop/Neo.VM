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
using Neo.Core.Cryptography;
using Neo.Core.Extensions;
using Neo.Core.Text;
using Neo.Wallet.Cryptography;
using Neo.Wallet.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Neo.Wallet
{
    /// <summary>
    /// Provides cryptographic helpers and file open operations for Neo wallets.
    /// </summary>
    public static class ChainWallet
    {
        /// <summary>
        /// The maximum supported private key size in bytes.
        /// </summary>
        public const int MaxPrivateKeySizeInBytes = 32;

        /// <summary>
        /// Decodes a private key from the specified WIF string.
        /// </summary>
        /// <param name="wifString">The WIF string to be decoded.</param>
        /// <returns>The decoded private key.</returns>
        public static byte[] GetKeyFromWifString(string wifString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wifString);

            var decodedWifBytes = Base58.DecodeCheck(wifString);

            if (decodedWifBytes.Length != 34 ||
                decodedWifBytes[0] != 0x80 ||
                decodedWifBytes[33] != 0x01)
                throw new FormatException("Invalid WIF key");

            return decodedWifBytes[1..^1];
        }

        /// <summary>
        /// Encrypts a private key into a NEP-2 string using the specified password and SCrypt parameters.
        /// </summary>
        /// <param name="privateKeyBytes">The private key to encrypt.</param>
        /// <param name="password">The password used for key derivation.</param>
        /// <param name="scryptParameters">The SCrypt parameters; uses <see cref="ScryptParameters.Default"/> when <see langword="null"/>.</param>
        /// <param name="addressVersion">The address version used by the protocol (currently unused by the encryption payload).</param>
        /// <returns>The NEP-2 encoded private key string.</returns>
        public static string ToNep2String(byte[] privateKeyBytes, string password, ScryptParameters? scryptParameters = default, byte addressVersion = 53)
        {
            const int SCryptKeyLengthInBytes = 64;
            const int SaltLengthInBytes = 4;

            scryptParameters ??= ScryptParameters.Default;

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var salt = RandomNumberGenerator.GetBytes(SaltLengthInBytes);

            var sCrypt = SCrypt.Generate(passwordBytes, salt, scryptParameters.N, scryptParameters.R, scryptParameters.P, SCryptKeyLengthInBytes);
            var derivedHalf1 = sCrypt[..32];
            var derivedHalf2 = sCrypt[32..];

            var ciphertextBytes = AesEncryptECB(privateKeyBytes.Xor(derivedHalf1), derivedHalf2);

            byte[] nep2StringBytes = [
                0x01, 0x42, 0xe0,
                .. salt,
                .. ciphertextBytes,
            ];

            return Base58.EncodeCheck(nep2StringBytes);
        }

        /// <summary>
        /// Decrypts a NEP-2 string into a private key using the specified password and SCrypt parameters.
        /// </summary>
        /// <param name="nep2String">The NEP-2 encoded private key string.</param>
        /// <param name="password">The password used for key derivation.</param>
        /// <param name="scryptParameters">The SCrypt parameters used when the key was encrypted.</param>
        /// <param name="addressVersion">The address version used by the protocol (currently unused by the decryption payload).</param>
        /// <returns>The decrypted private key bytes.</returns>
        /// <exception cref="FormatException">Thrown when the NEP-2 payload is invalid.</exception>
        public static byte[] GetKeyFromNep2String(string nep2String, string password, ScryptParameters scryptParameters, byte addressVersion = 53)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nep2String, nameof(nep2String));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(password, nameof(password));

            var decodedBytes = Base58.DecodeCheck(nep2String);
            var decodedSpan = decodedBytes.AsSpan();

            if (decodedSpan.Length != 39 ||
                decodedSpan[0] != 0x01 ||
                decodedSpan[1] != 0x42 ||
                decodedSpan[2] != 0xe0)
                throw new FormatException("Invalid NEP-2 key");

            const int SCryptKeyLengthInBytes = 64;
            const int SaltLengthInBytes = 4;

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var salt = decodedSpan.Slice(3, SaltLengthInBytes);

            var scrypt = SCrypt.Generate(passwordBytes, [.. salt], scryptParameters.N, scryptParameters.R, scryptParameters.P, SCryptKeyLengthInBytes);
            var derivedHalf1 = scrypt[..32];
            var derivedHalf2 = scrypt[32..];

            byte[] encryptedKeyBytes = [.. decodedSpan.Slice(7, 32)];
            var decryptedBytes = AesDecryptECB(encryptedKeyBytes, derivedHalf2);
            var privateKeyBytes = decryptedBytes.Xor(derivedHalf1);

            return privateKeyBytes;
        }

        /// <summary>
        /// Decrypts ciphertext using AES in ECB mode with no padding.
        /// </summary>
        /// <param name="ciphertextBytes">The encrypted data.</param>
        /// <param name="keyBytes">The AES key.</param>
        /// <returns>The decrypted plaintext bytes.</returns>
        public static byte[] AesDecryptECB(byte[] ciphertextBytes, byte[] keyBytes)
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
        }

        /// <summary>
        /// Encrypts plaintext using AES in ECB mode with no padding.
        /// </summary>
        /// <param name="plaintextBytes">The data to encrypt.</param>
        /// <param name="keyBytes">The AES key.</param>
        /// <returns>The encrypted ciphertext bytes.</returns>
        public static byte[] AesEncryptECB(byte[] plaintextBytes, byte[] keyBytes)
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        }

        /// <summary>
        /// Opens a development wallet from a JSON file.
        /// </summary>
        /// <param name="file">The wallet file to open.</param>
        /// <returns>The loaded <see cref="DevWallet"/>.</returns>
        public static DevWallet OpenDevWalletFile(FileInfo file) =>
            JsonModel.FromJson<DevWalletModel>(file)!.ToObject();

        /// <summary>
        /// Opens a NEP-6 wallet from a JSON file.
        /// </summary>
        /// <param name="file">The wallet file to open.</param>
        /// <returns>The loaded <see cref="Nep6Wallet"/>, or <see langword="null"/> if unavailable.</returns>
        public static Nep6Wallet? OpenNep6WalletFie(FileInfo file) =>
            JsonModel.FromJson<Nep6WalletModel>(file)!.ToObject();
    }
}
