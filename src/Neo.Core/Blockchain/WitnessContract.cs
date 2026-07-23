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

using Neo.Core.Cryptography.ECC;
using Neo.Core.Extensions;
using Neo.Core.VM;
using Neo.Core.VM.SmartContract;
using System;
using System.Linq;

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// Represents a contract that can be invoked.
    /// </summary>
    public sealed class WitnessContract
    {
        /// <summary>
        /// The hash of the contract.
        /// </summary>
        public UInt160 ScriptHash => _scriptHash;

        /// <summary>
        /// The script of the contract.
        /// </summary>
        public byte[] Script => _redeemScriptBytes;

        /// <summary>
        /// The parameters of the contract.
        /// </summary>
        public MethodParameterType[] ParameterList => _contractParameters;

        private readonly MethodParameterType[] _contractParameters;
        private readonly byte[] _redeemScriptBytes;
        private readonly UInt160 _scriptHash;

        private WitnessContract(byte[] redeemScriptBytes, MethodParameterType[] contractParameters)
        {
            _redeemScriptBytes = redeemScriptBytes;
            _contractParameters = contractParameters;
            _scriptHash = _redeemScriptBytes.ToScriptHash();
        }

        private WitnessContract(UInt160 scriptHash, MethodParameterType[] contractParameters)
        {
            _scriptHash = scriptHash;
            _redeemScriptBytes = [];
            _contractParameters = contractParameters;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WitnessContract"/> class.
        /// </summary>
        /// <param name="redeemScriptBytes">The script of the contract.</param>
        /// <param name="parameterList">The parameters of the contract.</param>
        /// <returns>The created contract.</returns>
        public static WitnessContract Create(byte[] redeemScriptBytes, params MethodParameterType[] parameterList) =>
            new(redeemScriptBytes, parameterList);

        /// <summary>
        /// Constructs a special contract with empty script, will get the script with scriptHash from blockchain when doing the verification.
        /// </summary>
        /// <param name="scriptHash">The hash of the contract.</param>
        /// <param name="parameterList">The parameters of the contract.</param>
        /// <returns>The created contract.</returns>
        public static WitnessContract Create(UInt160 scriptHash, params MethodParameterType[] parameterList) =>
            new(scriptHash, parameterList);

        /// <summary>
        /// Creates a multi-sig contract.
        /// </summary>
        /// <param name="m">The number of correct signatures that need to be provided in order for the verification to pass.</param>
        /// <param name="publicKeys">The public keys of the contract.</param>
        /// <returns>The created contract.</returns>
        public static WitnessContract CreateMultiSigContract(int m, params ECPoint[] publicKeys) =>
            new(CreateMultiSigRedeemScript(m, publicKeys), [.. Enumerable.Repeat(MethodParameterType.Signature, m)]);

        /// <summary>
        /// Creates the script of multi-sig contract.
        /// </summary>
        /// <param name="m">The number of correct signatures that need to be provided in order for the verification to pass.</param>
        /// <param name="publicKeys">The public keys of the contract.</param>
        /// <returns>The created script.</returns>
        public static byte[] CreateMultiSigRedeemScript(int m, params ECPoint[] publicKeys)
        {
            if (!(1 <= m && m <= publicKeys.Length && publicKeys.Length <= 1024))
                throw new ArgumentException($"Invalid multi-signature parameters: m={m}, publicKeys.Count={publicKeys.Length}");

            using var sb = new ScriptBuilder();
            sb.EmitPush(m);

            foreach (var publicKey in publicKeys.OrderBy(p => p))
                sb.EmitPush(publicKey.Encode(true));

            sb.EmitPush(publicKeys.Length);
            sb.EmitSysCall(0xdeadc0de); // TODO: Add real VM SystemCall Address

            return sb.ToArray();
        }

        /// <summary>
        /// Determines whether the specified redeem script is a multi-signature contract script.
        /// </summary>
        /// <param name="redeemScriptBytes">The redeem script to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if the script ends with a multi-signature system call; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsMultiSigContract(byte[] redeemScriptBytes)
        {
            using var sb = new ScriptBuilder();
            sb.EmitSysCall(0xdeadc0de); // TODO: Add real VM SystemCall Address

            var contractScriptSpan = sb.ToArray().AsSpan();
            var redeemScriptSpan = redeemScriptBytes.AsSpan();

            return contractScriptSpan.SequenceEqual(redeemScriptSpan[^contractScriptSpan.Length..]);
        }

        /// <summary>
        /// Creates a signature contract.
        /// </summary>
        /// <param name="publicKey">The public key of the contract.</param>
        /// <returns>The created contract.</returns>
        public static WitnessContract CreateSignatureContract(ECPoint publicKey) =>
            new(CreateSignatureRedeemScript(publicKey), [MethodParameterType.Signature]);

        /// <summary>
        /// Creates the script of signature contract.
        /// </summary>
        /// <param name="publicKey">The public key of the contract.</param>
        /// <returns>The created script.</returns>
        public static byte[] CreateSignatureRedeemScript(ECPoint publicKey)
        {
            using var sb = new ScriptBuilder();

            sb.EmitPush(publicKey.Encode(true));
            sb.EmitSysCall(0xbadc0de); // TODO: Add real VM SystemCall Address

            return sb.ToArray();
        }
    }
}
