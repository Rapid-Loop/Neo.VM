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
using Neo.Core.Extensions;
using Neo.Core.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// A blockchain block header without transaction bodies.
    /// </summary>
    public class BlockHeader : IEquatable<BlockHeader>, INeoSerializable, IVerifiable
    {
        /// <summary>
        /// The version of the block.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets the hash of this block header.
        /// </summary>
        public virtual UInt256 Hash => this.ToArray().ToTxHash();

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public UInt256 PrevHash { get; set; } = UInt256.Zero;

        /// <summary>
        /// The Merkle root of the transactions.
        /// </summary>
        public UInt256 MerkleRoot { get; set; } = UInt256.Zero;

        /// <summary>
        /// The timestamp of the block.
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// The first eight bytes of random number generated.
        /// </summary>
        public ulong Nonce { get; set; }

        /// <summary>
        /// The index of the block.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// The primary index of the consensus node that generated this block.
        /// </summary>
        public byte PrimaryIndex { get; set; }

        /// <summary>
        /// The multi-signature address of the consensus nodes that generates the next block.
        /// </summary>
        public UInt160 NextConsensus { get; set; } = UInt160.Zero;

        /// <summary>
        /// The witness of the block.
        /// </summary>
        public Witness Witness { get; set; } = Witness.Empty;

        /// <summary>
        /// The witnesses of the block.
        /// </summary>
        public Witness[] Witnesses => [Witness];

        /// <summary>
        /// Gets the serialized size of this block header in bytes.
        /// </summary>
        public virtual int Size =>
            sizeof(uint) +      // Version
            UInt256.Length +    // PrevHash
            UInt256.Length +    // MerkleRoot
            sizeof(ulong) +     // Timestamp
            sizeof(ulong) +     // Nonce
            sizeof(uint) +      // Index
            sizeof(byte) +      // PrimaryIndex
            UInt160.Length +    // NextConsensus
            Witnesses.GetSerializedSize();

        /// <summary>
        /// Returns a hash code for this header based on its hash.
        /// </summary>
        /// <returns>A hash code for the current header.</returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BlockHeader"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The header to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the headers have the same hash; otherwise, <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] BlockHeader? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            return Hash == other.Hash;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current header.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as BlockHeader);
        }

        /// <summary>
        /// Deserializes this header from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="FormatException">Version is not 0, or the witness count is not 1.</exception>
        public virtual void Deserialize(Stream reader)
        {
            Version = reader.Read<uint>();
            if (Version > 0)
                throw new FormatException($"[{nameof(BlockHeader)}::{nameof(Version)}] Value \'{Version}\' does not match \'0\'.");

            PrevHash.Deserialize(reader);
            MerkleRoot.Deserialize(reader);
            Timestamp = reader.Read<ulong>();
            Nonce = reader.Read<ulong>();
            Index = reader.Read<uint>();
            PrimaryIndex = reader.Read<byte>();
            NextConsensus.Deserialize(reader);

            var witnesses = reader.ReadObjects<Witness>();
            if (witnesses.Length != 1)
                throw new FormatException($"[{nameof(BlockHeader)}::{nameof(Witnesses)}->Length] Value \'{witnesses.Length}\' does not match \'1\'.");
            Witness = witnesses[0];
        }

        /// <summary>
        /// Serializes this header to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public virtual void Serialize(Stream writer)
        {
            writer.Write(Version);
            PrevHash.Serialize(writer);
            MerkleRoot.Serialize(writer);
            writer.Write(Timestamp);
            writer.Write(Nonce);
            writer.Write(Index);
            writer.Write(PrimaryIndex);
            NextConsensus.Serialize(writer);
            writer.WriteObjects(Witnesses);
        }
    }
}
