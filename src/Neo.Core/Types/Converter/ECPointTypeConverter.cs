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
using System;
using System.ComponentModel;
using System.Globalization;

namespace Neo.Core.Types.Converter
{
    /// <summary>
    /// Converts between <see cref="ECPoint"/> values and strings for design-time and configuration scenarios.
    /// </summary>
    internal class ECPointTypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to an <see cref="ECPoint"/>.
        /// </summary>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) && base.CanConvertFrom(context, sourceType);

        /// <summary>
        /// Converts a string (or other supported value) to an <see cref="ECPoint"/> on secp256r1.
        /// </summary>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string pointString)
            {
                if (ECPoint.TryParse(pointString, ECCurve.SecP256r1, out var point))
                    return point;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts an <see cref="ECPoint"/> to the requested destination type (typically a compressed hex string).
        /// </summary>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
            CanConvertTo(context, destinationType) ?
            value switch
            {
                ECPoint e => e.Encode(shouldCompress: true),
                _ => base.ConvertTo(context, culture, value, destinationType),
            } : base.ConvertTo(context, culture, value, destinationType);
    }
}
