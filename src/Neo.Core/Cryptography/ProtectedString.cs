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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Neo.Core.Cryptography
{
    /// <summary>
    /// Stores a string encrypted in memory using AES-GCM with process-local entropy.
    /// </summary>
    public class ProtectedString : IEquatable<ProtectedString>, IComparable, IComparable<ProtectedString>, IDisposable
    {
        // Sizes required for standard AES-GCM compliance
        private const int NonceSize = 12; // 12 bytes / 96 bits (Standard for GCM)
        private const int TagSize = 16;   // 16 bytes / 128 bits (Maximum security tag)

        private static readonly byte[] s_entropy = SHA256.HashData(RandomNumberGenerator.GetBytes(32));

        private readonly Memory<byte> _protectedMemory;

        private int _cachedHashCode = 0;

        /// <summary>
        /// Initializes an empty protected string.
        /// </summary>
        public ProtectedString() { }

        /// <summary>
        /// Initializes a protected string from the specified plaintext.
        /// </summary>
        /// <param name="plaintext">The plaintext string to protect.</param>
        public ProtectedString(string plaintext)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            _protectedMemory = ProtectData(plaintextBytes);
        }

        /// <summary>
        /// Clears the protected memory and releases resources.
        /// </summary>
        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(_protectedMemory.Span);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as ProtectedString);
        }

        /// <summary>
        /// Returns a hash code for this instance based on the protected payload.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            if (_cachedHashCode != 0) return _cachedHashCode;
            return _cachedHashCode = _protectedMemory.ToArray()
                .Aggregate(_protectedMemory.Length,
                (hash, b) =>
                        (hash * 31) ^ b);
        }

        /// <summary>
        /// Decrypts and returns the plaintext string.
        /// </summary>
        /// <returns>The unprotected plaintext.</returns>
        public override string ToString() =>
            UnprotectData(_protectedMemory.Span);

        /// <summary>
        /// Compares this instance to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or <see langword="null"/>.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="obj"/>.
        /// </returns>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            return CompareTo(obj as ProtectedString);
        }

        /// <summary>
        /// Compares this instance to another <see cref="ProtectedString"/>.
        /// </summary>
        /// <param name="other">A <see cref="ProtectedString"/> to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ProtectedString? other)
        {
            if (other is null) return 1;
            return string.Compare(this, other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ProtectedString"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The instance to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the plaintext values are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] ProtectedString? other)
        {
            if (other is null) return false;
            return string.Equals(this, other);
        }

        private static Memory<byte> ProtectData(byte[] data)
        {
            if (data.Length == 0) return Memory<byte>.Empty;

            using var aes = new AesGcm(s_entropy, TagSize);
            var nonceBytes = RandomNumberGenerator.GetBytes(NonceSize);
            var tagBytes = new byte[TagSize];
            var ciphertext = new byte[data.Length];

            aes.Encrypt(nonceBytes, data, ciphertext, tagBytes);

            // Combine: nonce + ciphertext + tag
            byte[] ciphertextMessageBytes = [
                .. nonceBytes,
                .. ciphertext,
                .. tagBytes,
            ];

            var protectedStringBytes = GC.AllocateUninitializedArray<byte>(ciphertextMessageBytes.Length, true);
            var result = MemoryMarshal.CreateFromPinnedArray(protectedStringBytes, 0, ciphertextMessageBytes.Length);

            ciphertextMessageBytes.CopyTo(protectedStringBytes);

            CryptographicOperations.ZeroMemory(data);
            CryptographicOperations.ZeroMemory(nonceBytes);
            CryptographicOperations.ZeroMemory(ciphertext);
            CryptographicOperations.ZeroMemory(tagBytes);
            CryptographicOperations.ZeroMemory(ciphertextMessageBytes);

            return result;
        }

        /// <summary>
        /// Decrypts a protected binary payload produced by this type.
        /// </summary>
        /// <param name="protectedData">The encrypted payload (nonce, ciphertext, and tag).</param>
        /// <returns>The decrypted UTF-8 string.</returns>
        public static string UnprotectData(ReadOnlySpan<byte> protectedData)
        {
            if (protectedData.Length == 0) return string.Empty;

            using var aes = new AesGcm(s_entropy, TagSize);

            var nonceBytes = protectedData[..NonceSize];
            var tagBytes = protectedData[^TagSize..];
            var ciphertext = protectedData.Slice(NonceSize, protectedData.Length - NonceSize - TagSize);

            Span<byte> plaintext = stackalloc byte[ciphertext.Length];
            aes.Decrypt(nonceBytes, ciphertext, tagBytes, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Implicitly converts a plaintext string to a <see cref="ProtectedString"/>.
        /// </summary>
        /// <param name="value">The plaintext string.</param>
        public static implicit operator ProtectedString(string value) =>
            new(value);

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedString"/> to its plaintext string.
        /// </summary>
        /// <param name="value">The protected string.</param>
        public static implicit operator string(ProtectedString value) =>
            value.ToString();

        /// <summary>
        /// Determines whether two <see cref="ProtectedString"/> values are equal.
        /// </summary>
        public static bool operator ==(ProtectedString a, ProtectedString b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two <see cref="ProtectedString"/> values are not equal.
        /// </summary>
        public static bool operator !=(ProtectedString a, ProtectedString b) =>
            !(a == b);
    }
}
