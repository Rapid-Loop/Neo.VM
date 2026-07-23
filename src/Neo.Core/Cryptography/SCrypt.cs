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
using System.Security.Cryptography;

namespace Neo.Core.Cryptography
{
    public static class SCrypt
    {
        /// <summary>
        /// Generates a derived key using SCrypt.
        /// </summary>
        public static byte[] Generate(byte[] password, byte[] salt, int n, int r, int p, int keyLength = 32)
        {
            ArgumentOutOfRangeException.ThrowIfZero(password.Length, nameof(password));
            ArgumentOutOfRangeException.ThrowIfZero(salt.Length, nameof(salt));

            if (n <= 1 || (n & (n - 1)) != 0) throw new ArgumentException("N must be a power of 2 and > 1");
            if (r < 1) throw new ArgumentException("r must be >= 1");
            if (p < 1) throw new ArgumentException("p must be >= 1");
            if (keyLength < 1) throw new ArgumentException("keyLength must be >= 1");

            return ScryptEncoder.CryptoScrypt(password, salt, n, r, p, keyLength);
        }

        /// <summary>
        /// Convenience overload for string password.
        /// </summary>
        public static byte[] Generate(string password, byte[] salt, int n = 16384, int r = 8, int p = 1, int keyLength = 32)
        {
            var passBytes = CoreUtilities.StrictUtf8Encoding.GetBytes(password);
            return Generate(passBytes, salt, n, r, p, keyLength);
        }

        /// <summary>
        /// Generates a cryptographically secure random salt.
        /// </summary>
        public static byte[] GenerateSalt(int length = 16)
        {
            var salt = new byte[length];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }
    }

    internal static class ScryptEncoder
    {
        public unsafe static byte[] CryptoScrypt(byte[] password, byte[] salt, int n, int r, int p, int keyLength = 32)
        {
            var ba = new byte[128 * r * p + 63];
            var xYa = new byte[256 * r + 63];
            var va = new byte[128 * r * n + 63];
            var buf = new byte[keyLength];

            var mac = new HMACSHA256(password);

            /* 1: (B_0 ... B_{p-1}) <-- PBKDF2(P, S, 1, p * 128 * r) */
            PBKDF2_SHA256(mac, password, salt, salt.Length, 1, ba, p * 128 * r);

            fixed (byte* b = ba)
            fixed (void* v = va)
            fixed (void* xy = xYa)
            {
                for (var i = 0; i < p; i++)
                    SMix(&b[i * 128 * r], r, n, (uint*)v, (uint*)xy);
            }

            /* 5: DK <-- PBKDF2(P, B, 1, dkLen) */
            PBKDF2_SHA256(mac, password, ba, p * 128 * r, 1, buf, buf.Length);

            return buf;
        }

        private static void PBKDF2_SHA256(HMACSHA256 mac, byte[] password, byte[] salt, int saltLength, long iterationCount, byte[] derivedKey, int derivedKeyLength)
        {
            if (derivedKeyLength > (Math.Pow(2, 32) - 1) * 32)
                throw new ArgumentException("Requested key length too long");

            var u = new byte[32];
            var t = new byte[32];
            var saltBuffer = new byte[saltLength + 4];

            var blockCount = (int)Math.Ceiling(derivedKeyLength / 32d);
            var r = derivedKeyLength - (blockCount - 1) * 32;

            Buffer.BlockCopy(salt, 0, saltBuffer, 0, saltLength);

            for (var i = 1; i <= blockCount; i++)
            {
                saltBuffer[saltLength + 0] = (byte)(i >> 24);
                saltBuffer[saltLength + 1] = (byte)(i >> 16);
                saltBuffer[saltLength + 2] = (byte)(i >> 8);
                saltBuffer[saltLength + 3] = (byte)i;

                mac.Initialize();
                mac.TransformFinalBlock(saltBuffer, 0, saltBuffer.Length);
                Buffer.BlockCopy(mac.Hash!, 0, u, 0, u.Length);

                Buffer.BlockCopy(u, 0, t, 0, 32);

                for (long j = 1; j < iterationCount; j++)
                {
                    mac.TransformFinalBlock(u, 0, u.Length);
                    Buffer.BlockCopy(mac.Hash!, 0, u, 0, u.Length);

                    for (var k = 0; k < 32; k++)
                        t[k] ^= u[k];
                }

                Buffer.BlockCopy(t, 0, derivedKey, (i - 1) * 32, (i == blockCount ? r : 32));
            }
        }

        private unsafe static void BulkCopy(void* dst, void* src, int len)
        {
            var d = (byte*)dst;
            var s = (byte*)src;
            while (len >= 8) { *(ulong*)d = *(ulong*)s; d += 8; s += 8; len -= 8; }
            if (len >= 4) { *(uint*)d = *(uint*)s; d += 4; s += 4; len -= 4; }
            if (len >= 2) { *(ushort*)d = *(ushort*)s; d += 2; s += 2; len -= 2; }
            if (len >= 1) *d = *s;
        }

        private unsafe static void BulkXor(void* dst, void* src, int len)
        {
            var d = (byte*)dst;
            var s = (byte*)src;
            while (len >= 8) { *(ulong*)d ^= *(ulong*)s; d += 8; s += 8; len -= 8; }
            if (len >= 4) { *(uint*)d ^= *(uint*)s; d += 4; s += 4; len -= 4; }
            if (len >= 2) { *(ushort*)d ^= *(ushort*)s; d += 2; s += 2; len -= 2; }
            if (len >= 1) *d ^= *s;
        }

        private unsafe static void Encode32(byte* p, uint x)
        {
            p[0] = (byte)(x & 0xff);
            p[1] = (byte)((x >> 8) & 0xff);
            p[2] = (byte)((x >> 16) & 0xff);
            p[3] = (byte)((x >> 24) & 0xff);
        }

        private unsafe static uint Decode32(byte* p)
        {
            return p[0] | ((uint)p[1] << 8) | ((uint)p[2] << 16) | ((uint)p[3] << 24);
        }

        private unsafe static void Salsa208(uint* b)
        {
            uint x0 = b[0], x1 = b[1], x2 = b[2], x3 = b[3],
                 x4 = b[4], x5 = b[5], x6 = b[6], x7 = b[7],
                 x8 = b[8], x9 = b[9], x10 = b[10], x11 = b[11],
                 x12 = b[12], x13 = b[13], x14 = b[14], x15 = b[15];

            for (var i = 0; i < 8; i += 2)
            {
                x4 ^= R(x0 + x12, 7); x8 ^= R(x4 + x0, 9); x12 ^= R(x8 + x4, 13); x0 ^= R(x12 + x8, 18);
                x9 ^= R(x5 + x1, 7); x13 ^= R(x9 + x5, 9); x1 ^= R(x13 + x9, 13); x5 ^= R(x1 + x13, 18);
                x14 ^= R(x10 + x6, 7); x2 ^= R(x14 + x10, 9); x6 ^= R(x2 + x14, 13); x10 ^= R(x6 + x2, 18);
                x3 ^= R(x15 + x11, 7); x7 ^= R(x3 + x15, 9); x11 ^= R(x7 + x3, 13); x15 ^= R(x11 + x7, 18);

                x1 ^= R(x0 + x3, 7); x2 ^= R(x1 + x0, 9); x3 ^= R(x2 + x1, 13); x0 ^= R(x3 + x2, 18);
                x6 ^= R(x5 + x4, 7); x7 ^= R(x6 + x5, 9); x4 ^= R(x7 + x6, 13); x5 ^= R(x4 + x7, 18);
                x11 ^= R(x10 + x9, 7); x8 ^= R(x11 + x10, 9); x9 ^= R(x8 + x11, 13); x10 ^= R(x9 + x8, 18);
                x12 ^= R(x15 + x14, 7); x13 ^= R(x12 + x15, 9); x14 ^= R(x13 + x12, 13); x15 ^= R(x14 + x13, 18);
            }

            b[0] += x0; b[1] += x1; b[2] += x2; b[3] += x3;
            b[4] += x4; b[5] += x5; b[6] += x6; b[7] += x7;
            b[8] += x8; b[9] += x9; b[10] += x10; b[11] += x11;
            b[12] += x12; b[13] += x13; b[14] += x14; b[15] += x15;
        }

        private static uint R(uint a, int b) =>
            (a << b) | (a >> (32 - b));

        private unsafe static void BlockMix(uint* bin, uint* bOut, uint* x, int r)
        {
            BulkCopy(x, &bin[(2 * r - 1) * 16], 64);

            for (var i = 0; i < 2 * r; i += 2)
            {
                BulkXor(x, &bin[i * 16], 64);
                Salsa208(x);
                BulkCopy(&bOut[i * 8], x, 64);

                BulkXor(x, &bin[i * 16 + 16], 64);
                Salsa208(x);
                BulkCopy(&bOut[i * 8 + r * 16], x, 64);
            }
        }

        private unsafe static long Integerify(uint* b, int r)
        {
            var x = (uint*)(((byte*)b) + (2 * r - 1) * 64);
            return (((long)x[1] << 32) + x[0]);
        }

        private unsafe static void SMix(byte* b, int r, int n, uint* v, uint* xY)
        {
            var x = xY;
            var y = &xY[32 * r];
            var z = &xY[64 * r];

            for (var k = 0; k < 32 * r; k++)
                x[k] = Decode32(&b[4 * k]);

            for (var i = 0L; i < n; i += 2)
            {
                BulkCopy(&v[i * (32 * r)], x, 128 * r);
                BlockMix(x, y, z, r);
                BulkCopy(&v[(i + 1) * (32 * r)], y, 128 * r);
                BlockMix(y, x, z, r);
            }

            for (var i = 0; i < n; i += 2)
            {
                var j = Integerify(x, r) & (n - 1);
                BulkXor(x, &v[j * (32 * r)], 128 * r);
                BlockMix(x, y, z, r);

                j = Integerify(y, r) & (n - 1);
                BulkXor(y, &v[j * (32 * r)], 128 * r);
                BlockMix(y, x, z, r);
            }

            for (var k = 0; k < 32 * r; k++)
                Encode32(&b[4 * k], x[k]);
        }
    }
}
