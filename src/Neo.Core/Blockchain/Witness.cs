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
using Neo.Core.Serialization;
using System.IO;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// A cryptographic witness consisting of invocation and verification scripts.
    /// </summary>
    public class Witness : INeoSerializable
    {
        /// <summary>
        /// The maximum allowed length of an invocation script in bytes.
        /// </summary>
        public const int MaxInvocationScript = 1024;

        /// <summary>
        /// The maximum allowed length of a verification script in bytes.
        /// </summary>
        public const int MaxVerificationScript = 1024;

        /// <summary>
        /// Gets an empty witness with no scripts.
        /// </summary>
        public static Witness Empty => new();

        /// <summary>
        /// The invocation script of the witness. Used to pass arguments for <see cref="VerificationScript"/>.
        /// </summary>
        public byte[] InvocationScript { get; set; } = [];

        /// <summary>
        /// The verification script of the witness. It can be empty if the contract is deployed.
        /// </summary>
        public byte[] VerificationScript { get; set; } = [];

        /// <summary>
        /// The hash of the <see cref="VerificationScript"/>.
        /// </summary>
        public UInt160 ScriptHash => VerificationScript.ToScriptHash();

        /// <summary>
        /// Gets the serialized size of this witness in bytes.
        /// </summary>
        public int Size =>
            InvocationScript.GetSerializedSize() +
            VerificationScript.GetSerializedSize();

        /// <summary>
        /// Deserializes this witness from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        public void Deserialize(Stream reader)
        {
            InvocationScript = reader.ReadDynamic<byte>();
            VerificationScript = reader.ReadDynamic<byte>();
        }

        /// <summary>
        /// Serializes this witness to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        public void Serialize(Stream writer)
        {
            writer.Write<byte>(InvocationScript);
            writer.Write<byte>(VerificationScript);
        }
    }
}
