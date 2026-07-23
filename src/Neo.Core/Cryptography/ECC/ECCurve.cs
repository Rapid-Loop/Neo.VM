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

using System;
using System.Numerics;
using CECCurve = System.Security.Cryptography.ECCurve;

namespace Neo.Core.Cryptography.ECC
{
    public class ECCurve
    {
        public static readonly ECCurve SecP256r1 = new(
            BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951"),
            BigInteger.Parse("41058363725152142129326129780047268409114441015993725554835256314039467401291"),
            ECCurveName.SecP256r1
        );

        public ECCurveName Name => _curveName;

        public CECCurve ECCurveType => _curve;

        public BigInteger P => _p;

        public BigInteger B => _b;

        private readonly BigInteger _p;
        private readonly BigInteger _b;
        private readonly ECCurveName _curveName;
        private readonly CECCurve _curve;

        private ECCurve(BigInteger p, BigInteger b, ECCurveName curve)
        {
            _p = p;
            _b = b;
            _curveName = curve;
            _curve = curve switch
            {
                ECCurveName.SecP256r1 => CECCurve.NamedCurves.nistP256,
                _ => throw new NotSupportedException(nameof(curve)),
            };
        }

        internal static byte[] CompressPoint(ReadOnlySpan<byte> span)
        {
            var prefix = span[^1] % 2 == 0 ?
                (byte)0x02 : (byte)0x03;

            return [
                prefix,
                .. span[1..33]
            ];
        }

        internal byte[] DecompressPoint(ReadOnlySpan<byte> compressedPublicKeyBytes)
        {
            if (compressedPublicKeyBytes.Length != 33 || (compressedPublicKeyBytes[0] != 0x02 && compressedPublicKeyBytes[0] != 0x03))
                throw new FormatException("Invalid compressed key format.");

            var xBytes = compressedPublicKeyBytes[1..33];

            var x = new BigInteger(xBytes, isUnsigned: true, isBigEndian: true);
            var xCubed = BigInteger.ModPow(x, 3, _p);
            var z = (xCubed - (3 * x) + _b) % _p;

            if (z < 0) z += _p;

            var y = BigInteger.ModPow(z, (_p + 1) / 4, _p);

            var isYEven = (y % 2 == 0);
            var shouldBeEven = compressedPublicKeyBytes[0] == 0x02;

            if (isYEven != shouldBeEven) y = _p - y;

            var yBytes = y.ToByteArray(isUnsigned: true, isBigEndian: true);

            return [
                0x04,
                .. compressedPublicKeyBytes[1..33],
                .. yBytes,
            ];
        }
    }
}
