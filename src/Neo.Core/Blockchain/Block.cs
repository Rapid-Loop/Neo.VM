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

using Neo.Core.Blockchain.Interface;
using Neo.Core.Cryptography;
using Neo.Core.Extensions;
using Neo.Core.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// A blockchain block that includes a header and its transactions.
    /// </summary>
    public class Block : BlockHeader, IEquatable<Block>, INeoSerializable, IInventory, IVerifiable
    {
        /// <summary>
        /// Gets the hash of this block.
        /// </summary>
        public override UInt256 Hash => this.ToArray().ToTxHash();

        /// <summary>
        /// The transaction list of the block.
        /// </summary>
        public Transaction[] Transactions { get; set; } = [];

        /// <summary>
        /// Gets the inventory type for this block.
        /// </summary>
        public InventoryType InventoryType => InventoryType.Block;

        /// <summary>
        /// Gets the serialized size of this block in bytes.
        /// </summary>
        public override int Size =>
            base.Size +
            Transactions.GetSerializedSize();

        /// <summary>
        /// Returns a hash code for this block based on its hash.
        /// </summary>
        /// <returns>A hash code for the current block.</returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Block"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The block to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the blocks have the same hash; otherwise, <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] Block? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            return Hash == other.Hash;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current block.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as Block);
        }

        /// <summary>
        /// Deserializes this block from the specified stream and validates the Merkle root.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="FormatException">The computed Merkle root does not match the header.</exception>
        public override void Deserialize(Stream reader)
        {
            base.Deserialize(reader);
            Transactions = reader.ReadObjects<Transaction>();

            var txHashes = Transactions
                .Select(static s => s.Hash)
                .ToHashSet();

            if (MerkleTree.ComputeRoot([.. txHashes]) != MerkleRoot)
                throw new FormatException("The computed Merkle root does not match the expected value.");
        }

        /// <summary>
        /// Serializes this block to the specified stream, recomputing the Merkle root first.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public override void Serialize(Stream writer)
        {
            var txHashes = Transactions
                .Select(static s => s.Hash)
                .ToHashSet();

            MerkleRoot = MerkleTree.ComputeRoot([.. txHashes]);

            base.Serialize(writer);
            writer.WriteObjects(Transactions);
        }
    }
}
