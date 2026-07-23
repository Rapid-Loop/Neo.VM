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

namespace Neo.Core.Blockchain
{
    /// <summary>
    /// Identifies the type of a <see cref="TransactionAttribute"/>.
    /// </summary>
    public enum TransactionAttributeType : byte
    {
        /// <summary>
        /// Indicates that the transaction is of high priority.
        /// </summary>
        HighPriority = 0x01,

        /// <summary>
        /// Indicates that the transaction is an oracle response.
        /// </summary>
        OracleResponse = 0x11,

        /// <summary>
        /// Indicates that the transaction is not valid before height
        /// </summary>
        NotValidBefore = 0x20,

        /// <summary>
        /// Indicates that the transaction conflicts with hash
        /// </summary>
        Conflicts = 0x21,

        /// <summary>
        /// Indicates that the transaction uses notary request service with "NKeys" number of keys.
        /// </summary>
        NotaryAssisted = 0x22
    }
}
