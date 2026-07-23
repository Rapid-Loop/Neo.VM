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
using Neo.Core.Types.Converter;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Neo.Core.Cryptography.ECC
{
    [TypeConverter(typeof(ECPointTypeConverter))]
    public class ECPoint : IEquatable<ECPoint>, IComparable, IComparable<ECPoint>, INeoSerializable
    {
        public const int UncompressedLength = 65;
        public const int CompressedLength = 33;

        public BigInteger X => _x;

        public BigInteger Y => _y;

        public bool IsInfinity => _x == BigInteger.Zero && _y == BigInteger.Zero;

        public int Length => _uncompressed.Length;

        public int Size =>
            sizeof(ECCurveName) +
            _uncompressed.Length;

        public ECCurve Curve => _curve;

        private ECCurve _curve;

        private byte[] _uncompressed;
        private BigInteger _x, _y;

        private ECPoint() : this(BigInteger.Zero, BigInteger.Zero, ECCurve.SecP256r1) { }

        private ECPoint(BigInteger x, BigInteger y, ECCurve curve)
        {
            _x = x;
            _y = y;
            _curve = curve;
            _uncompressed = [
                0x04,
                .. x.ToByteArray(true, true),
                .. y.ToByteArray(true, true),
            ];
        }

        public static ECPoint FromPrivateKey(ReadOnlySpan<byte> privateKeySpan, ECCurve curve)
        {
            using var ecdsa = ECDsa.Create();

            var ecPrivateKeyParameters = new ECParameters
            {
                Curve = curve.ECCurveType,
                D = [.. privateKeySpan],
            };

            ecdsa.ImportParameters(ecPrivateKeyParameters);

            var publicKeyParameters = ecdsa.ExportParameters(false);
            var x = new BigInteger(publicKeyParameters.Q.X.AsSpan(), true, true);
            var y = new BigInteger(publicKeyParameters.Q.Y.AsSpan(), true, true);

            return new(x, y, curve);
        }

        public static ECPoint Parse(string value, ECCurve curve)
        {
            if ((value.Length != UncompressedLength * 2 || value.Length != CompressedLength * 2) &&
                value.Length % 2 != 0)
                throw new FormatException();

            var bytes = Convert.FromHexString(value);
            var x = new BigInteger(bytes[1..33].AsSpan(), true, true);
            var y = BigInteger.Zero;

            if (bytes.Length == UncompressedLength)
                y = new(bytes[^32..].AsSpan(), true, true);

            if (bytes.Length == CompressedLength)
            {
                var data = ECCurve.SecP256r1.DecompressPoint(bytes);
                y = new(data[^32..].AsSpan(), true, true);
            }

            return new(x, y, curve);
        }

        public static bool TryParse(string value, ECCurve curve, [MaybeNullWhen(false)] out ECPoint? result)
        {
            try
            {
                result = Parse(value, curve);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as ECPoint);
        }

        public override int GetHashCode()
        {
            return _uncompressed.ToHashCode();
        }

        [return: MaybeNull]
        public override string? ToString()
        {
            return Convert.ToHexStringLower(Encode(false));
        }

        public bool Equals(ECPoint? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (_curve != other._curve) return false;
            return _x == other._x && _y == other._y;
        }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            return CompareTo(obj as ECPoint);
        }

        public int CompareTo(ECPoint? other)
        {
            if (other is null) return 1;
            var result = _x.CompareTo(other._x);
            if (result != 0) return result;
            return _y.CompareTo(other._y);
        }

        public void Serialize(Stream writer)
        {
            if (writer.CanWrite == false)
                throw new NotSupportedException("This stream does not support writing.");

            writer.Write(_curve.Name);
            writer.Write(Encode(false));
        }

        public void Deserialize(Stream reader)
        {
            if (reader.CanRead == false)
                throw new NotSupportedException("This stream does not support reading.");

            _curve = reader.Read<ECCurveName>() switch
            {
                ECCurveName.SecP256r1 => ECCurve.SecP256r1,
                _ => throw new FormatException()
            };

            _uncompressed = GC.AllocateUninitializedArray<byte>(UncompressedLength, false);
            reader.ReadExactly(_uncompressed, 0, UncompressedLength);

            _x = new(_uncompressed.AsSpan()[1..33], true, true);
            _y = new(_uncompressed.AsSpan()[33..UncompressedLength], true, true);
        }

        public byte[] Encode(bool shouldCompress = true)
        {
            return shouldCompress ?
                ECCurve.CompressPoint(_uncompressed.AsSpan()) :
                _uncompressed;
        }

        public static bool operator ==(ECPoint? left, ECPoint? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(ECPoint? left, ECPoint? right)
        {
            return !(left == right);
        }

        public static bool operator <(ECPoint? left, ECPoint? right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ECPoint? left, ECPoint? right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ECPoint? left, ECPoint? right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ECPoint? left, ECPoint? right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
