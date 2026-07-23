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

using Neo.Core.VM.Attributes;
using Neo.Core.VM.Type;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Neo.Core.VM
{
    /// <summary>
    /// Represents a single decoded instruction within a Neo VM script, including its opcode and operand.
    /// </summary>
    [DebuggerDisplay("OpCode={OpCode}, OperandSize={OperandSize}")]
    public sealed class OpCodeInst : IEnumerable<OpCodeInst>
    {
        private const int OpCodeSize = 1;

        /// <summary>
        /// Gets a prebuilt instruction containing only <see cref="OpCode.RET"/>.
        /// </summary>
        public static OpCodeInst Ret => new((byte[])[(byte)OpCode.RET]);

        /// <summary>
        /// Gets the byte offset of this instruction within the script.
        /// </summary>
        public int Position { get; private init; }

        /// <summary>
        /// Gets the opcode of this instruction.
        /// </summary>
        public OpCode OpCode { get; private init; }

        /// <summary>
        /// Gets the operand bytes associated with this instruction.
        /// </summary>
        public ReadOnlyMemory<byte> Operand { get; private init; }

        /// <summary>
        /// Gets the total operand size in bytes, including any size prefix.
        /// </summary>
        public int OperandSize { get; private init; }

        /// <summary>
        /// Gets the size of the operand length prefix in bytes, or zero when the operand size is fixed.
        /// </summary>
        public int OperandPrefixSize { get; private init; }

        /// <summary>
        /// Gets the total size of this instruction in bytes, including the opcode and operand.
        /// </summary>
        public int Size => OperandPrefixSize > 0 ?
            OpCodeSize + Operand.Length :
            OpCodeSize + OperandSize;

        private static readonly int[] s_operandSizeTable = new int[256];
        private static readonly int[] s_operandSizePrefixTable = new int[256];

        private readonly ReadOnlyMemory<byte> _script;

        /// <summary>
        /// Decodes the instruction at the specified position in a script.
        /// </summary>
        /// <param name="script">The script that contains the instruction.</param>
        /// <param name="start">The byte offset at which the instruction begins.</param>
        /// <exception cref="Exception">Thrown when the script is empty.</exception>
        /// <exception cref="InvalidDataException">Thrown when the opcode or operand prefix is invalid.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown when the operand extends past the end of the script.</exception>
        public OpCodeInst(ReadOnlyMemory<byte> script, int start = 0)
        {
            if (script.IsEmpty)
                throw new Exception("Bad Script.");

            var opcode = (OpCode)script.Span[start];

            if (Enum.IsDefined(opcode) == false)
                throw new InvalidDataException($"Invalid opcode at Position: {start}.");

            OperandPrefixSize = s_operandSizePrefixTable[(int)opcode];
            OperandSize = OperandPrefixSize switch
            {
                0 => s_operandSizeTable[(int)opcode],
                1 => script.Span[start + 1],
                2 => BinaryPrimitives.ReadUInt16LittleEndian(script.Span[(start + 1)..]),
                4 => unchecked((int)BinaryPrimitives.ReadUInt32LittleEndian(script.Span[(start + 1)..])),
                _ => throw new InvalidDataException($"Invalid opcode prefix at Position: {start}."),
            };

            OperandSize += OperandPrefixSize;

            if (start + OperandSize + OpCodeSize > script.Length)
                throw new IndexOutOfRangeException("Operand size exceeds end of script.");

            Operand = script.Slice(start + OpCodeSize, OperandSize);

            _script = script;
            OpCode = opcode;
            Position = start;
        }

        static OpCodeInst()
        {
            foreach (var field in typeof(OpCode).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral == false) continue; // Skip non-enum values

                var attr = field.GetCustomAttribute<OperandSizeAttribute>();
                if (attr == null) continue;

                var index = (uint)(OpCode)field.GetValue(null)!;
                s_operandSizeTable[index] = attr.Size;
                s_operandSizePrefixTable[index] = attr.SizePrefix;
            }
        }

        /// <summary>
        /// Enumerates this instruction and all subsequent instructions in the script.
        /// </summary>
        /// <returns>An enumerator over instructions starting at the current position.</returns>
        public IEnumerator<OpCodeInst> GetEnumerator()
        {
            var nip = Position + OperandSize + OpCodeSize;
            yield return this;

            OpCodeInst? instruct;
            for (var ip = nip; ip < _script.Length; ip += instruct.OperandSize + OpCodeSize)
                yield return instruct = new OpCodeInst(_script, ip);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Returns a human-readable representation of the instruction, including position, opcode, and decoded operand.
        /// </summary>
        /// <returns>A formatted instruction string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0:X04} {1,-10}{2}", Position, OpCode, DecodeOperand());
            return sb.ToString();
        }

        /// <summary>
        /// Interprets a portion of the operand as an unmanaged value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to read from the operand.</typeparam>
        /// <param name="index">The byte offset within the operand at which to begin reading.</param>
        /// <returns>The value read from the operand.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <typeparamref name="T"/> does not fit in the operand at the specified index.
        /// </exception>
        public T AsToken<T>(uint index = 0)
            where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();

            if (size > OperandSize)
                throw new ArgumentOutOfRangeException(nameof(T), $"SizeOf {typeof(T).FullName} is too big for operand. OpCode: {OpCode}.");
            if (size + index > OperandSize)
                throw new ArgumentOutOfRangeException(nameof(index), $"SizeOf {typeof(T).FullName} + {index} is too big for operand. OpCode: {OpCode}.");

            var bytes = Operand[..OperandSize].ToArray();
            return Unsafe.As<byte, T>(ref bytes[index]);
        }

        /// <summary>
        /// Produces a short human-readable decoding of the operand for display and debugging.
        /// </summary>
        /// <returns>A string describing the operand, or an empty string when no decoding is available.</returns>
        public string DecodeOperand()
        {
            var operand = Operand[OperandPrefixSize..].ToArray();
            var asStr = Encoding.ASCII.GetString(operand);
            var readable = asStr.All(char.IsAscii);

            if (string.IsNullOrWhiteSpace(asStr))
                asStr = "[null]";

            return OpCode switch
            {
                OpCode.PUSHT => $"// \"{bool.TrueString}\"",
                OpCode.PUSHF => $"// \"{bool.FalseString}\"",
                OpCode.PUSHM1 => "// \"-1\"",
                OpCode.PUSH0 => "// \"0\"",
                OpCode.PUSH1 => "// \"1\"",
                OpCode.PUSH2 => "// \"2\"",
                OpCode.PUSH3 => "// \"3\"",
                OpCode.PUSH4 => "// \"4\"",
                OpCode.PUSH5 => "// \"5\"",
                OpCode.PUSH6 => "// \"6\"",
                OpCode.PUSH7 => "// \"7\"",
                OpCode.PUSH8 => "// \"8\"",
                OpCode.PUSH9 => "// \"9\"",
                OpCode.PUSH10 => "// \"10\"",
                OpCode.PUSH11 => "// \"11\"",
                OpCode.PUSH12 => "// \"12\"",
                OpCode.PUSH13 => "// \"13\"",
                OpCode.PUSH14 => "// \"14\"",
                OpCode.PUSH15 => "// \"15\"",
                OpCode.PUSH16 => "// \"16\"",
                OpCode.JMP or
                OpCode.JMPIF or
                OpCode.JMPIFNOT or
                OpCode.JMPEQ or
                OpCode.JMPNE or
                OpCode.JMPGT or
                OpCode.JMPLT or
                OpCode.CALL or
                OpCode.ENDTRY => $"[0x{checked(Position + AsToken<byte>()):x8}]",
                OpCode.JMP_L or
                OpCode.JMPIF_L or
                OpCode.PUSHA or
                OpCode.JMPIFNOT_L or
                OpCode.JMPEQ_L or
                OpCode.JMPNE_L or
                OpCode.JMPGT_L or
                OpCode.JMPLT_L or
                OpCode.CALL_L or
                OpCode.ENDTRY_L => $"[0x{checked(Position + AsToken<int>()):x8}]",
                OpCode.TRY => $"[0x{AsToken<byte>():x2}, 0x{AsToken<byte>(1):x2}]",
                OpCode.INITSLOT => $"[{AsToken<byte>()}, {AsToken<byte>(1)}]",
                OpCode.TRY_L => $"[0x{checked(Position + AsToken<int>()):x8}, 0x{checked(Position + AsToken<int>()):x8}]",
                OpCode.CALLT => $"[0x{checked(Position + AsToken<ushort>()):x8}]",
                OpCode.NEWARRAY_T or
                OpCode.ISTYPE or
                OpCode.CONVERT => $"0x{AsToken<byte>():x2} // \"{AsToken<VMObjectType>():G}\"",
                OpCode.STLOC or
                OpCode.LDLOC or
                OpCode.LDSFLD or
                OpCode.STSFLD or
                OpCode.LDARG or
                OpCode.STARG or
                OpCode.INITSSLOT => $"{AsToken<byte>()}",
                OpCode.PUSHINT8 => $"{AsToken<sbyte>()}",
                OpCode.PUSHINT16 => $"{AsToken<short>()}",
                OpCode.PUSHINT32 => $"{AsToken<int>()}",
                OpCode.PUSHINT64 => $"{AsToken<long>()}",
                OpCode.PUSHINT128 or
                OpCode.PUSHINT256 => $"{new BigInteger(operand)}",
                OpCode.SYSCALL => $"[0x{Unsafe.As<byte, uint>(ref operand[0]):x8}]",
                OpCode.PUSHDATA1 or
                OpCode.PUSHDATA2 or
                OpCode.PUSHDATA4 => operand.Length == 0 ? string.Empty : readable ? $"0x{Convert.ToHexStringLower(operand)} // \"{asStr}\"" : $"0x{Convert.ToHexStringLower(operand)}",
                _ => string.Empty,
            };
        }
    }
}
