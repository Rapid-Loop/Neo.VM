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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Neo.Core.VM
{
    /// <summary>
    /// A helper class for building scripts.
    /// </summary>
    public class ScriptBuilder : IDisposable
    {
        private readonly MemoryStream _stream = new();

        /// <summary>
        /// The length of the script.
        /// </summary>
        public int Length => (int)_stream.Length;

        /// <summary>
        /// Releases the underlying script buffer.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Converts the value of this instance to a byte array.
        /// </summary>
        /// <returns>A byte array contains the script.</returns>
        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        /// <summary>
        /// Emits an <see cref="OpCodeInst"/> with the specified <see cref="OpCode"/> and operand.
        /// </summary>
        /// <param name="opcode">The <see cref="OpCode"/> to be emitted.</param>
        /// <param name="operand">The operand to be emitted.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder Emit(OpCode opcode, ReadOnlySpan<byte> operand = default)
        {
            _stream.Write((byte)opcode);
            _stream.Write(operand);
            return this;
        }

        /// <summary>
        /// Emits a call <see cref="OpCodeInst"/> with the specified offset.
        /// </summary>
        /// <param name="offset">The offset to be called.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitCall(int offset)
        {
            if (offset < sbyte.MinValue || offset > sbyte.MaxValue)
                return Emit(OpCode.CALL_L, BitConverter.GetBytes(offset));
            else
                return Emit(OpCode.CALL, [(byte)offset]);
        }

        /// <summary>
        /// Emits a jump <see cref="OpCodeInst"/> with the specified offset.
        /// </summary>
        /// <param name="opcode">The <see cref="OpCode"/> to be emitted. It must be a jump <see cref="OpCode"/></param>
        /// <param name="offset">The offset to jump.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitJump(OpCode opcode, int offset)
        {
            if (opcode < OpCode.JMP || opcode > OpCode.JMPLE_L)
                throw new ArgumentOutOfRangeException(nameof(opcode));

            if ((int)opcode % 2 == 0 && (offset < sbyte.MinValue || offset > sbyte.MaxValue))
                opcode += 1;

            if ((int)opcode % 2 == 0)
                return Emit(opcode, [(byte)offset]);
            else
                return Emit(opcode, BitConverter.GetBytes(offset));
        }

        /// <summary>
        /// Emits a push <see cref="OpCodeInst"/> with the specified number.
        /// </summary>
        /// <param name="value">The number to be pushed.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitPush(BigInteger value)
        {
            if (value >= -1 && value <= 16)
                return Emit(OpCode.PUSH0 + (byte)(int)value);

            Span<byte> buffer = stackalloc byte[32];
            if (!value.TryWriteBytes(buffer, out var bytesWritten, isUnsigned: false, isBigEndian: false))
                throw new ArgumentOutOfRangeException(nameof(value));

            return bytesWritten switch
            {
                1 => Emit(OpCode.PUSHINT8, PadRight(buffer, bytesWritten, 1, value.Sign < 0)),
                2 => Emit(OpCode.PUSHINT16, PadRight(buffer, bytesWritten, 2, value.Sign < 0)),
                <= 4 => Emit(OpCode.PUSHINT32, PadRight(buffer, bytesWritten, 4, value.Sign < 0)),
                <= 8 => Emit(OpCode.PUSHINT64, PadRight(buffer, bytesWritten, 8, value.Sign < 0)),
                <= 16 => Emit(OpCode.PUSHINT128, PadRight(buffer, bytesWritten, 16, value.Sign < 0)),
                <= 32 => Emit(OpCode.PUSHINT256, PadRight(buffer, bytesWritten, 32, value.Sign < 0)),
                _ => throw new ArgumentOutOfRangeException(nameof(value), "Invalid value: BigInteger is too large"),
            };
        }

        /// <summary>
        /// Emits a push <see cref="OpCodeInst"/> with the specified boolean value.
        /// </summary>
        /// <param name="value">The value to be pushed.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitPush(bool value)
        {
            return Emit(value ? OpCode.PUSHT : OpCode.PUSHF);
        }

        /// <summary>
        /// Emits a push <see cref="OpCodeInst"/> with the specified data.
        /// </summary>
        /// <param name="data">The data to be pushed.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitPush(ReadOnlySpan<byte> data)
        {
            if (data.Length < 0x100)
            {
                Emit(OpCode.PUSHDATA1);
                _stream.Write((byte)data.Length);
                _stream.Write(data);
            }
            else if (data.Length < 0x10000)
            {
                Emit(OpCode.PUSHDATA2);
                _stream.Write((ushort)data.Length);
                _stream.Write(data);
            }
            else// if (data.Length < 0x100000000L)
            {
                Emit(OpCode.PUSHDATA4);
                _stream.Write(data.Length);
                _stream.Write(data);
            }

            return this;
        }

        /// <summary>
        /// Emits a push <see cref="OpCodeInst"/> with the specified <see cref="string"/>.
        /// </summary>
        /// <param name="data">The <see cref="string"/> to be pushed.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitPush(string data)
        {
            return EmitPush(CoreUtilities.StrictUtf8Encoding.GetBytes(data));
        }

        /// <summary>
        /// Emits raw script.
        /// </summary>
        /// <param name="script">The raw script to be emitted.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitRaw(ReadOnlySpan<byte> script = default)
        {
            _stream.Write(script);
            return this;
        }

        /// <summary>
        /// Emits an <see cref="OpCodeInst"/> with <see cref="OpCode.SYSCALL"/>.
        /// </summary>
        /// <param name="api">The operand of <see cref="OpCode.SYSCALL"/>.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitSysCall(uint api)
        {
            return Emit(OpCode.SYSCALL, BitConverter.GetBytes(api));
        }

        /// <summary>
        /// Emits instructions that create an array from the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="list">The items to place into the array; when null or empty, emits <see cref="OpCode.NEWARRAY0"/>.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitCreateArray<T>(IReadOnlyList<T>? list = null)
        {
            if (list is null || list.Count == 0)
                return Emit(OpCode.NEWARRAY0);

            for (var i = list.Count - 1; i >= 0; i--)
                EmitPush(list[i]);

            EmitPush(list.Count);

            return Emit(OpCode.PACK);
        }

        /// <summary>
        /// Emits instructions that create a map from the specified key/value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the map keys.</typeparam>
        /// <typeparam name="TValue">The type of the map values.</typeparam>
        /// <param name="map">The key/value pairs to place into the map.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitCreateMap<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> map)
            where TKey : notnull
            where TValue : notnull
        {
            var count = map.Count();

            if (count == 0)
                return Emit(OpCode.NEWMAP);

            foreach (var (key, value) in map.Reverse())
            {
                EmitPush(value);
                EmitPush(key);
            }

            EmitPush(count);

            return Emit(OpCode.PACKMAP);
        }

        /// <summary>
        /// Emits instructions that create a map from the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the map keys.</typeparam>
        /// <typeparam name="TValue">The type of the map values.</typeparam>
        /// <param name="map">The dictionary to place into the map.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitCreateMap<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> map)
            where TKey : notnull
            where TValue : notnull
        {
            if (map.Count == 0)
                return Emit(OpCode.NEWMAP);

            foreach (var (key, value) in map.Reverse())
            {
                EmitPush(value);
                EmitPush(key);
            }

            EmitPush(map.Count);

            return Emit(OpCode.PACKMAP);
        }

        /// <summary>
        /// Emits instructions that create a struct from the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the struct elements.</typeparam>
        /// <param name="array">The items to place into the struct.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitCreateStruct<T>(IReadOnlyList<T> array)
            where T : notnull
        {
            if (array.Count == 0)
                return Emit(OpCode.NEWSTRUCT0);

            for (var i = array.Count - 1; i >= 0; i--)
                EmitPush(array[i]);

            EmitPush(array.Count);

            return Emit(OpCode.PACKSTRUCT);
        }

        /// <summary>
        /// Emits one or more opcodes without operands.
        /// </summary>
        /// <param name="ops">The opcodes to emit.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder Emit(params OpCode[] ops)
        {
            foreach (var op in ops)
                Emit(op);

            return this;
        }

        /// <summary>
        /// Emits a push instruction for a supported object value.
        /// </summary>
        /// <param name="obj">
        /// The value to push. Supported types include booleans, numeric types, strings, byte arrays,
        /// enums, <see cref="INeoSerializable"/> instances, and <see langword="null"/>.
        /// </param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> has an unsupported type.</exception>
        public ScriptBuilder EmitPush(object? obj)
        {
            switch (obj)
            {
                case bool data:
                    EmitPush(data);
                    break;
                case byte[] data:
                    EmitPush(data);
                    break;
                case string data:
                    EmitPush(data);
                    break;
                case BigInteger data:
                    EmitPush(data);
                    break;
                case sbyte data:
                    EmitPush(data);
                    break;
                case byte data:
                    EmitPush(data);
                    break;
                case short data:
                    EmitPush(data);
                    break;
                case char data:
                    EmitPush(data);
                    break;
                case ushort data:
                    EmitPush(data);
                    break;
                case int data:
                    EmitPush(data);
                    break;
                case uint data:
                    EmitPush(data);
                    break;
                case long data:
                    EmitPush(data);
                    break;
                case ulong data:
                    EmitPush(data);
                    break;
                case Enum data:
                    EmitPush(BigInteger.Parse(data.ToString("d")));
                    break;
                case INeoSerializable data:
                    EmitPush(data.ToArray());
                    break;
                case null:
                    Emit(OpCode.PUSHNULL);
                    break;
                default:
                    throw new ArgumentException($"Unsupported object type: {obj.GetType()}. This object type cannot be converted to a stack item for script execution.", nameof(obj));
            }

            return this;
        }

        /// <summary>
        /// Emits argument pushes followed by an <see cref="OpCode.SYSCALL"/> for the specified API.
        /// </summary>
        /// <param name="method">The interoperable service hash to call.</param>
        /// <param name="args">The arguments to push onto the stack before the call.</param>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitSysCall(uint method, params object[] args)
        {
            for (var i = args.Length - 1; i >= 0; i--)
                EmitPush(args[i]);

            return EmitSysCall(method);
        }

        /// <summary>
        /// Emits an <see cref="OpCode.RET"/> instruction.
        /// </summary>
        /// <returns>A reference to this instance after the emit operation has completed.</returns>
        public ScriptBuilder EmitReturn() =>
            Emit(OpCode.RET);

        private static ReadOnlySpan<byte> PadRight(Span<byte> buffer, int dataLength, int padLength, bool negative)
        {
            var pad = negative ?
                (byte)0xff :
                (byte)0;

            for (var x = dataLength; x < padLength; x++)
                buffer[x] = pad;

            return buffer[..padLength];
        }
    }
}
