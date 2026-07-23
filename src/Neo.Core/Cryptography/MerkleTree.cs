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

using Neo.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Neo.Core.Cryptography
{
    /// <summary>
    /// Builds Merkle trees over hash leaves and computes Merkle roots.
    /// </summary>
    public class MerkleTree
    {
        private readonly byte[] _root = [];
        private readonly List<byte[]> _leaves = [];

        private MerkleTree(IEnumerable<byte[]> leaves)
        {
            _leaves.AddRange(leaves);
            if (_leaves.Count == 0) return;

            _root = BuildTree(_leaves);
        }

        /// <summary>
        /// Computes the Merkle root of the specified hash leaves.
        /// </summary>
        /// <param name="hashes">The leaf hashes; an empty array yields <see cref="UInt256.Zero"/>.</param>
        /// <returns>The Merkle root of <paramref name="hashes"/>.</returns>
        public static UInt256 ComputeRoot(UInt256[] hashes)
        {
            if (hashes.Length == 0) return UInt256.Zero;
            if (hashes.Length == 1) return hashes[0];

            var tree = new MerkleTree(hashes.Select(static s => s.ToArray()));
            return new(tree._root);
        }

        private static byte[] BuildTree(List<byte[]> nodes)
        {
            if (nodes.Count == 1)
                return nodes[0];

            List<byte[]> parents = [];

            for (var i = 0; i < nodes.Count; i += 2)
            {
                var left = nodes[i];
                var right = (i + 1 < nodes.Count) ? nodes[i + 1] : left; // Duplicate last if odd

                Span<byte> combined = [.. left, .. right];
                var parentHash = SHA256.HashData(combined);

                parents.Add(parentHash);
            }

            return BuildTree(parents);
        }
    }
}
