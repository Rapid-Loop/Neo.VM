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

namespace Neo.Core.VM
{
    /// <summary>
    /// Identifies protocol hard forks that may change VM behavior, gas costs, or features.
    /// </summary>
    public enum HardFork : byte
    {
        /// <summary>
        /// The initial protocol state before any named hard fork.
        /// </summary>
        Genesis = 0,

        /// <summary>
        /// The Aspidochelone hard fork.
        /// </summary>
        Aspidochelone,

        /// <summary>
        /// The Basilisk hard fork.
        /// </summary>
        Basilisk,

        /// <summary>
        /// The Cockatrice hard fork.
        /// </summary>
        Cockatrice,

        /// <summary>
        /// The Domovoi hard fork.
        /// </summary>
        Domovoi,

        /// <summary>
        /// The Echidna hard fork.
        /// </summary>
        Echidna,

        /// <summary>
        /// The Faun hard fork.
        /// </summary>
        Faun,

        /// <summary>
        /// The Gorgon hard fork.
        /// </summary>
        Gorgon,
    }
}
