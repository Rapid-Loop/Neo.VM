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
using System.Linq;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// A NEO transaction that can be verified, inventoried, and serialized.
    /// </summary>
    public class Transaction : IEquatable<Transaction>, IInventory, INeoSerializable, IVerifiable
    {
        /// <summary>
        /// The maximum allowed size of a serialized transaction in bytes.
        /// </summary>
        public const int MaxTransactionSize = 102400;

        /// <summary>
        /// The maximum number of attributes allowed on a transaction.
        /// </summary>
        public const int MaxTransactionAttributes = 16;

        /// <summary>
        /// The version of the transaction.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets the hash of this transaction.
        /// </summary>
        public UInt256 Hash => this.ToArray().ToTxHash();

        /// <summary>
        /// The script of the transaction.
        /// </summary>
        public ReadOnlyMemory<byte> Script { get; set; }

        /// <summary>
        /// The nonce of the transaction.
        /// </summary>
        public uint Nonce { get; set; }

        /// <summary>
        /// The system fee of the transaction.
        /// </summary>
        public long SystemFee { get; set; }

        /// <summary>
        /// The network fee of the transaction.
        /// </summary>
        public long NetworkFee { get; set; }

        /// <summary>
        /// Indicates that the transaction is only valid before this block height.
        /// </summary>
        public uint ValidUnitBlock { get; set; }

        /// <summary>
        /// The <see cref="NetworkFee"/> for the transaction divided by its <see cref="Size"/>.
        /// </summary>
        public long FeePerByte => NetworkFee / Size;

        /// <summary>
        /// The attributes of the transaction.
        /// </summary>
        public TransactionAttribute[] Attributes { get; set; } = [];

        /// <summary>
        /// The signers of the transaction.
        /// </summary>
        public Signer[] Signers { get; set; } = [];

        /// <summary>
        /// The witnesses of the transaction.
        /// </summary>
        public Witness[] Witnesses { get; set; } = [];

        /// <summary>
        /// The sender is the first signer of the transaction, regardless of its <see cref="WitnessScope"/>.
        /// </summary>
        /// <remarks>Note: The sender will pay the fees of the transaction.</remarks>
        public UInt160 Sender => Signers.FirstOrDefault()?.Account ?? UInt160.Zero;

        /// <summary>
        /// Gets the inventory type for this transaction.
        /// </summary>
        public InventoryType InventoryType => InventoryType.TX;

        /// <summary>
        /// Gets the serialized size of this transaction in bytes.
        /// </summary>
        public int Size =>
            sizeof(byte) +  //Version
            sizeof(uint) +  //Nonce
            sizeof(long) +  //SystemFee
            sizeof(long) +  //NetworkFee
            sizeof(uint) +  //ValidUntilBlock
            Signers.GetSerializedSize() +
            Attributes.GetSerializedSize() +
            Script.GetSerializedSize() +
            Witnesses.GetSerializedSize();

        /// <summary>
        /// Returns a hash code for this transaction based on its hash.
        /// </summary>
        /// <returns>A hash code for the current transaction.</returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current transaction.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as Transaction);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Transaction"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The transaction to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the transactions have the same hash; otherwise, <see langword="false"/>.</returns>
        public bool Equals([NotNullWhen(true)] Transaction? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            return Hash == other.Hash;
        }

        /// <summary>
        /// Deserializes this transaction from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        public void Deserialize(Stream reader)
        {
            Version = reader.Read<byte>();
            Nonce = reader.Read<uint>();
            SystemFee = reader.Read<long>();
            NetworkFee = reader.Read<long>();
            ValidUnitBlock = reader.Read<uint>();
            Signers = reader.ReadObjects<Signer>();
            Attributes = reader.ReadObjects<TransactionAttribute>();
            Script = reader.ReadDynamic<byte>();
            Witnesses = reader.ReadObjects<Witness>();
        }

        /// <summary>
        /// Serializes this transaction to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write(Version);
            writer.Write(Nonce);
            writer.Write(SystemFee);
            writer.Write(NetworkFee);
            writer.Write(ValidUnitBlock);
            writer.WriteObjects(Signers);
            writer.WriteObjects(Attributes);
            writer.Write<byte>(Script.Span);
            writer.WriteObjects(Witnesses);
        }
    }
}
