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

namespace Neo.Core.Net
{
    /// <summary>
    /// Identifies a node capability advertised in the P2P version handshake.
    /// </summary>
    public enum NodeCapabilityType : byte
    {
        #region Servers

        /// <summary>
        /// Indicates that the node is listening on a Tcp port.
        /// </summary>
        TcpServer = 0x01,

        /// <summary>
        /// Disable p2p compression
        /// </summary>
        DisableCompression = 0x03,

        #endregion

        #region Data availability

        /// <summary>
        /// Indicates that the node has complete current state.
        /// </summary>
        FullNode = 0x10,

        /// <summary>
        /// Indicates that the node stores full block history. These nodes can be used
        /// for P2P synchronization from genesis (other ones can cut the tail and
        /// won't respond to requests for old (wrt MaxTraceableBlocks) blocks).
        /// </summary>
        ArchivalNode = 0x11,

        #endregion

        #region Private extensions

        /// <summary>
        /// The first extension ID. Any subsequent can be used in an
        /// implementation-specific way.
        /// </summary>
        Extension0 = 0xf0

        #endregion
    }
}
