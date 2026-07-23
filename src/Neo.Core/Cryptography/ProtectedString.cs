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
    public class ProtectedString : IEquatable<ProtectedString>, IComparable, IComparable<ProtectedString>, IDisposable
    {
        // Sizes required for standard AES-GCM compliance
        private const int NonceSize = 12; // 12 bytes / 96 bits (Standard for GCM)
        private const int TagSize = 16;   // 16 bytes / 128 bits (Maximum security tag)

        private static readonly byte[] s_entropy = SHA256.HashData(RandomNumberGenerator.GetBytes(32));

        private readonly Memory<byte> _protectedMemory;

        private int _cachedHashCode = 0;

        public ProtectedString() { }

        public ProtectedString(string plaintext)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            _protectedMemory = ProtectData(plaintextBytes);
        }

        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(_protectedMemory.Span);
            GC.SuppressFinalize(this);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as ProtectedString);
        }

        public override int GetHashCode()
        {
            if (_cachedHashCode != 0) return _cachedHashCode;
            return _cachedHashCode = _protectedMemory.ToArray()
                .Aggregate(_protectedMemory.Length,
                (hash, b) =>
                        (hash * 31) ^ b);
        }

        public override string ToString() =>
            UnprotectData(_protectedMemory.Span);

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            return CompareTo(obj as ProtectedString);
        }

        public int CompareTo(ProtectedString? other)
        {
            if (other is null) return 1;
            return string.Compare(this, other);
        }

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

        public static implicit operator ProtectedString(string value) =>
            new(value);

        public static implicit operator string(ProtectedString value) =>
            value.ToString();

        public static bool operator ==(ProtectedString a, ProtectedString b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProtectedString a, ProtectedString b) =>
            !(a == b);
    }
}
