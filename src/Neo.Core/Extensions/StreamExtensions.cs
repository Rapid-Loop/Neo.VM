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

using Neo.Core.Serialization;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Neo.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for reading and writing Neo serialization formats on <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Writes an integer value using Bitcoin-style compact size encoding.
        /// </summary>
        /// <typeparam name="T">The integer type to write.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The value to encode.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void WriteCompact<T>(this Stream stream, T value)
            where T : unmanaged, IBinaryInteger<T>
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var v = Convert.ToUInt64(value);

            switch (v)
            {
                case < 0xfd:
                    stream.Write((byte)v);
                    break;
                case <= ushort.MaxValue:
                    stream.Write((byte)0xfd);
                    stream.Write((ushort)v);
                    break;
                case <= uint.MaxValue:
                    stream.Write((byte)0xfe);
                    stream.Write((uint)v);
                    break;
                default:
                    stream.Write((byte)0xff);
                    stream.Write(v);
                    break;
            }
        }

        /// <summary>
        /// Writes the raw bytes of an unmanaged value to the stream.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to write.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write<T>(this Stream stream, T value)
            where T : unmanaged
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var tSpan = MemoryMarshal.CreateSpan(ref value, 1);
            var span = MemoryMarshal.AsBytes(tSpan);

            stream.Write(span);
        }

        /// <summary>
        /// Writes the raw bytes of an unmanaged value to the stream by reference.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to write.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write<T>(this Stream stream, ref T value)
            where T : unmanaged
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var tSpan = MemoryMarshal.CreateSpan(ref value, 1);
            var span = MemoryMarshal.AsBytes(tSpan);

            stream.Write(span);
        }

        /// <summary>
        /// Writes an unmanaged array with a compact-size length prefix followed by the raw element bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="values">The array to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write<T>(this Stream stream, T[] values)
            where T : unmanaged
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var tSpan = values.AsSpan();
            var span = MemoryMarshal.AsBytes(tSpan);

            stream.WriteCompact(values.Length);
            stream.Write(span);
        }

        /// <summary>
        /// Writes an unmanaged span with a compact-size length prefix followed by the raw element bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="values">The span to write.</param>
        public static void Write<T>(this Stream stream, Span<T> values)
            where T : unmanaged =>
            stream.Write((ReadOnlySpan<T>)values);

        /// <summary>
        /// Writes an unmanaged read-only span with a compact-size length prefix followed by the raw element bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="values">The span to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write<T>(this Stream stream, ReadOnlySpan<T> values)
            where T : unmanaged
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var span = MemoryMarshal.AsBytes(values);

            stream.WriteCompact(values.Length);
            stream.Write(span);
        }

        /// <summary>
        /// Writes a UTF-8 string with a compact-size byte-length prefix.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The string to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write(this Stream stream, string value)
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var encoding = CoreUtilities.StrictUtf8Encoding;
            var valueSpan = value.AsSpan();
            var encodedLength = encoding.GetByteCount(valueSpan);

            Span<byte> byteSpan = stackalloc byte[encodedLength];
            var byteLength = encoding.GetBytes(valueSpan, byteSpan);

            stream.WriteCompact(byteLength);
            stream.Write(byteSpan);
        }

        /// <summary>
        /// Writes a character array as UTF-8 with compact-size prefixes for both byte length and character count.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The character array to write.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void Write(this Stream stream, char[] value)
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            var encoding = CoreUtilities.StrictUtf8Encoding;
            var valueSpan = value.AsSpan();
            var encodedLength = encoding.GetByteCount(valueSpan);

            Span<byte> byteSpan = stackalloc byte[encodedLength];
            var byteLength = encoding.GetBytes(valueSpan, byteSpan);

            stream.WriteCompact(byteLength);
            stream.WriteCompact(value.Length);
            stream.Write(byteSpan);
        }

        /// <summary>
        /// Writes an array of <see cref="INeoSerializable"/> objects with a compact-size length prefix.
        /// </summary>
        /// <typeparam name="T">The serializable element type.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="values">The objects to serialize.</param>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support writing.</exception>
        public static void WriteObjects<T>(this Stream stream, T[] values)
            where T : INeoSerializable
        {
            if (stream.CanWrite == false)
                throw new NotSupportedException("Stream does not support writing.");

            stream.WriteCompact(values.Length);

            foreach (var t in values)
                t.Serialize(stream);
        }

        /// <summary>
        /// Reads an integer value encoded with Bitcoin-style compact size encoding.
        /// </summary>
        /// <typeparam name="T">The integer type to read.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The decoded integer value.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the stream ends before the value can be read.</exception>
        public static T ReadCompact<T>(this Stream stream)
            where T : unmanaged, IBinaryInteger<T>
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            var prefixByte = stream.ReadByte();

            if (prefixByte == -1)
                throw new EndOfStreamException("Unexpected end of stream while reading CompactSize.");

            var flag = (byte)prefixByte;

            var value = flag switch
            {
                < 0xfd => flag,
                0xfd => stream.Read<ushort>(),
                0xfe => stream.Read<uint>(),
                0xff => stream.Read<ulong>(),
            };

            return T.CreateChecked(value); // Throws on overflow for the target type
        }

        /// <summary>
        /// Reads the raw bytes of an unmanaged value into the provided reference.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to read.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="result">A reference that receives the read value.</param>
        /// <returns>A reference to <paramref name="result"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        public static ref T Read<T>(this Stream stream, ref T result)
            where T : unmanaged
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            var tSpan = MemoryMarshal.CreateSpan(ref result, 1);
            var span = MemoryMarshal.AsBytes(tSpan);

            stream.ReadExactly(span);

            return ref result;
        }

        /// <summary>
        /// Reads the raw bytes of an unmanaged value from the stream.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to read.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The value read from the stream.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        public static T Read<T>(this Stream stream)
            where T : unmanaged
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            var result = default(T);
            var tSpan = MemoryMarshal.CreateSpan(ref result, 1);
            var span = MemoryMarshal.AsBytes(tSpan);

            stream.ReadExactly(span);

            return result;
        }

        /// <summary>
        /// Reads a character array previously written by <see cref="Write(Stream, char[])"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The decoded character array.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        public static char[] ReadCharArray(this Stream stream)
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            var byteLength = stream.ReadCompact<int>();
            var charLength = stream.ReadCompact<int>();

            Span<byte> span = stackalloc byte[byteLength];
            stream.ReadExactly(span);

            var results = new char[charLength];
            var charSpan = results.AsSpan();
            var encoding = CoreUtilities.StrictUtf8Encoding;
            _ = encoding.GetChars(span, charSpan);

            return results;
        }

        /// <summary>
        /// Reads a UTF-8 string previously written by <see cref="Write(Stream, string)"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The decoded string.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        public static string ReadString(this Stream stream)
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            var encoding = CoreUtilities.StrictUtf8Encoding;
            var byteLength = stream.ReadCompact<int>();

            Span<byte> bytes = stackalloc byte[byteLength];
            stream.ReadExactly(bytes);

            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Reads an unmanaged array previously written with a compact-size length prefix.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The decoded array.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        /// <exception cref="NotImplementedException">Thrown when <typeparamref name="T"/> is <see cref="char"/>.</exception>
        public static T[] ReadDynamic<T>(this Stream stream)
            where T : unmanaged
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            if (typeof(T) == typeof(char))
                throw new NotImplementedException($"\'{typeof(T).FullName}\' type is not supported.");

            var length = stream.ReadCompact<int>();
            var results = new T[length];

            var tSpan = results.AsSpan();
            var span = MemoryMarshal.AsBytes(tSpan);
            stream.ReadExactly(span);

            return results;
        }

        /// <summary>
        /// Reads an array of <see cref="INeoSerializable"/> objects previously written by <see cref="WriteObjects{T}"/>.
        /// </summary>
        /// <typeparam name="T">The serializable element type.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The deserialized object array.</returns>
        /// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
        /// <exception cref="NotImplementedException">Thrown when <typeparamref name="T"/> is <see cref="char"/>.</exception>
        public static T[] ReadObjects<T>(this Stream stream)
            where T : INeoSerializable
        {
            if (stream.CanRead == false)
                throw new NotSupportedException("Stream does not support reading.");

            if (typeof(T) == typeof(char))
                throw new NotImplementedException($"\'{typeof(T).FullName}\' type is not supported.");

            var length = stream.ReadCompact<int>();
            var results = new T[length];

            for (var i = 0; i < length; i++)
            {
                results[i] = (T)typeof(T).CreateInitializedObject();
                results[i].Deserialize(stream);
            }

            return results;
        }
    }
}
