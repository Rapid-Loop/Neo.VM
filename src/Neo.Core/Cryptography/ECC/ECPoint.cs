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
    /// <summary>
    /// An elliptic curve public key point on a supported <see cref="ECCurve"/>.
    /// </summary>
    [TypeConverter(typeof(ECPointTypeConverter))]
    public class ECPoint : IEquatable<ECPoint>, IComparable, IComparable<ECPoint>, INeoSerializable
    {
        /// <summary>
        /// The length in bytes of an uncompressed public key encoding (including prefix).
        /// </summary>
        public const int UncompressedLength = 65;

        /// <summary>
        /// The length in bytes of a compressed public key encoding (including prefix).
        /// </summary>
        public const int CompressedLength = 33;

        /// <summary>
        /// Gets the X coordinate of this point.
        /// </summary>
        public BigInteger X => _x;

        /// <summary>
        /// Gets the Y coordinate of this point.
        /// </summary>
        public BigInteger Y => _y;

        /// <summary>
        /// Gets a value indicating whether this point is the point at infinity.
        /// </summary>
        public bool IsInfinity => _x == BigInteger.Zero && _y == BigInteger.Zero;

        /// <summary>
        /// Gets the length of the uncompressed encoding in bytes.
        /// </summary>
        public int Length => _uncompressed.Length;

        /// <summary>
        /// Gets the serialized size of this point in bytes.
        /// </summary>
        public int Size =>
            sizeof(ECCurveName) +
            _uncompressed.Length;

        /// <summary>
        /// Gets the curve this point belongs to.
        /// </summary>
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

        /// <summary>
        /// Derives the public key point from a private key on the specified curve.
        /// </summary>
        /// <param name="privateKeySpan">The private key bytes.</param>
        /// <param name="curve">The elliptic curve.</param>
        /// <returns>The corresponding public key point.</returns>
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

        /// <summary>
        /// Parses a hex-encoded public key on the specified curve.
        /// </summary>
        /// <param name="value">A compressed or uncompressed hex public key.</param>
        /// <param name="curve">The elliptic curve.</param>
        /// <returns>The parsed point.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> is not a valid public key encoding.</exception>
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

        /// <summary>
        /// Attempts to parse a hex-encoded public key on the specified curve.
        /// </summary>
        /// <param name="value">A compressed or uncompressed hex public key.</param>
        /// <param name="curve">The elliptic curve.</param>
        /// <param name="result">When this method returns, the parsed point if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Determines whether the specified object is equal to the current point.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as ECPoint);
        }

        /// <summary>
        /// Returns a hash code for this point.
        /// </summary>
        /// <returns>A hash code for the current point.</returns>
        public override int GetHashCode()
        {
            return _uncompressed.ToHashCode();
        }

        /// <summary>
        /// Returns the uncompressed hex encoding of this point.
        /// </summary>
        /// <returns>A lower-case hex string of the uncompressed public key.</returns>
        [return: MaybeNull]
        public override string? ToString()
        {
            return Convert.ToHexStringLower(Encode(false));
        }

        /// <summary>
        /// Determines whether the specified <see cref="ECPoint"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The point to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the points are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ECPoint? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (_curve != other._curve) return false;
            return _x == other._x && _y == other._y;
        }

        /// <summary>
        /// Compares this instance to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or <see langword="null"/>.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="obj"/>.
        /// </returns>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(obj, this)) return 0;
            return CompareTo(obj as ECPoint);
        }

        /// <summary>
        /// Compares this instance to another <see cref="ECPoint"/>.
        /// </summary>
        /// <param name="other">An <see cref="ECPoint"/> to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of this instance and <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ECPoint? other)
        {
            if (other is null) return 1;
            var result = _x.CompareTo(other._x);
            if (result != 0) return result;
            return _y.CompareTo(other._y);
        }

        /// <summary>
        /// Serializes this point to the specified stream.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        public void Serialize(Stream writer)
        {
            if (writer.CanWrite == false)
                throw new NotSupportedException("This stream does not support writing.");

            writer.Write(_curve.Name);
            writer.Write(Encode(false));
        }

        /// <summary>
        /// Deserializes this point from the specified stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="FormatException">The curve name is not supported.</exception>
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

        /// <summary>
        /// Encodes this point in compressed or uncompressed form.
        /// </summary>
        /// <param name="shouldCompress"><see langword="true"/> for compressed encoding; otherwise uncompressed.</param>
        /// <returns>The encoded public key bytes.</returns>
        public byte[] Encode(bool shouldCompress = true)
        {
            return shouldCompress ?
                ECCurve.CompressPoint(_uncompressed.AsSpan()) :
                _uncompressed;
        }

        /// <summary>
        /// Determines whether two <see cref="ECPoint"/> values are equal.
        /// </summary>
        public static bool operator ==(ECPoint? left, ECPoint? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="ECPoint"/> values are not equal.
        /// </summary>
        public static bool operator !=(ECPoint? left, ECPoint? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether one <see cref="ECPoint"/> is less than another.
        /// </summary>
        public static bool operator <(ECPoint? left, ECPoint? right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether one <see cref="ECPoint"/> is less than or equal to another.
        /// </summary>
        public static bool operator <=(ECPoint? left, ECPoint? right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether one <see cref="ECPoint"/> is greater than another.
        /// </summary>
        public static bool operator >(ECPoint? left, ECPoint? right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether one <see cref="ECPoint"/> is greater than or equal to another.
        /// </summary>
        public static bool operator >=(ECPoint? left, ECPoint? right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
