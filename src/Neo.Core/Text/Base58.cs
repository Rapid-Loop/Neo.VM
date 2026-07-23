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
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Neo.Core.Text
{
    /// <summary>
    /// Provides Base58 and Base58Check encoding and decoding.
    /// </summary>
    public static class Base58
    {
        private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        private static readonly Dictionary<char, int> s_alphabetMap = [];

        static Base58()
        {
            for (var i = 0; i < Alphabet.Length; i++)
                s_alphabetMap[Alphabet[i]] = i;
        }

        /// <summary>
        /// Encodes a byte array to Base58 string
        /// </summary>
        public static string Encode(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            // Count leading zeros
            var zeros = 0;
            while (zeros < input.Length && input[zeros] == 0)
                zeros++;

            // Convert to BigInteger
            var number = new BigInteger(input, isUnsigned: true, isBigEndian: true);

            // Encode
            var result = new StringBuilder();
            while (number > 0)
            {
                number = BigInteger.DivRem(number, Alphabet.Length, out var remainder);
                result.Insert(0, Alphabet[(int)remainder]);
            }

            // Add leading '1's for each leading zero byte
            return new string('1', zeros) + result.ToString();
        }

        /// <summary>
        /// Decodes a Base58 string to byte array
        /// </summary>
        public static byte[] Decode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return [];

            // Count leading '1's
            var leadingOnesCount = 0;
            while (leadingOnesCount < input.Length && input[leadingOnesCount] == '1')
                leadingOnesCount++;

            BigInteger number = 0;
            foreach (var c in input)
            {
                if (!s_alphabetMap.TryGetValue(c, out var index))
                    throw new FormatException($"Invalid Base58 character: {c}");

                number = number * Alphabet.Length + index;
            }

            if (number.IsZero)
                return new byte[leadingOnesCount];

            // Convert BigInteger to byte array
            var bytes = number.ToByteArray(isUnsigned: true, isBigEndian: true);

            // Add leading zeros
            var result = new byte[leadingOnesCount + bytes.Length];
            Array.Copy(bytes, 0, result, leadingOnesCount, bytes.Length);

            return result;
        }

        /// <summary>
        /// Encode with checksum (Base58Check)
        /// </summary>
        public static string EncodeCheck(byte[] input)
        {
            var dataWithChecksum = new byte[input.Length + 4];
            Array.Copy(input, 0, dataWithChecksum, 0, input.Length);

            var hash = SHA256.HashData(SHA256.HashData(input));
            Array.Copy(hash, 0, dataWithChecksum, input.Length, 4);

            return Encode(dataWithChecksum);
        }

        /// <summary>
        /// Decode with checksum verification (Base58Check)
        /// </summary>
        public static byte[] DecodeCheck(string input)
        {
            var decoded = Decode(input);
            if (decoded.Length < 4)
                throw new FormatException("Invalid Base58Check data");

            var data = new byte[decoded.Length - 4];
            Array.Copy(decoded, 0, data, 0, data.Length);

            var hash = SHA256.HashData(SHA256.HashData(data));

            for (var i = 0; i < 4; i++)
            {
                if (decoded[data.Length + i] != hash[i])
                    throw new FormatException("Base58Check checksum failed");
            }

            return data;
        }
    }
}
