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

namespace Neo.VM.Logging
{
    /// <summary>
    /// Well-known event IDs used by <c>LoggerMessage</c> sources in Neo.VM.
    /// </summary>
    public static class VirtualMachineEventId
    {
        /// <summary>Event ID for unrecoverable VM faults.</summary>
        public const int Fault = 100;

        /// <summary>Event ID for engine or context creation.</summary>
        public const int Create = 200;
        /// <summary>Event ID for script or context load operations.</summary>
        public const int Load = 201;
        /// <summary>Event ID for paired pre/post lifecycle messages.</summary>
        public const int PrePost = 202;
        /// <summary>Event ID for post-lifecycle messages.</summary>
        public const int Post = 203;
        /// <summary>Event ID for debugger breakpoint stops.</summary>
        public const int Break = 204;
        /// <summary>Event ID for general execution progress messages.</summary>
        public const int Execute = 205;

        /// <summary>Event ID for gas burn operations.</summary>
        public const int Burn = 300;
        /// <summary>Event ID for interop or contract calls.</summary>
        public const int Call = 301;
        /// <summary>Event ID for runtime notify events.</summary>
        public const int Notify = 302;
        /// <summary>Event ID for runtime log events.</summary>
        public const int Log = 303;

        /// <summary>Event ID for block persist start.</summary>
        public const int Persist = 400;
        /// <summary>Event ID for block persist completion.</summary>
        public const int PostPersist = 401;

        /// <summary>Event ID for storage put operations.</summary>
        public const int StoragePut = 500;
        /// <summary>Event ID for storage get operations.</summary>
        public const int StorageGet = 501;
        /// <summary>Event ID for storage find/query operations.</summary>
        public const int StorageFind = 502;
        /// <summary>Event ID for storage delete operations.</summary>
        public const int StorageDelete = 503;

        /// <summary>Event ID for iterator advance operations.</summary>
        public const int IteratorNext = 600;
        /// <summary>Event ID for iterator value retrieval.</summary>
        public const int IteratorGet = 601;

        /// <summary>Event ID for storage read-path diagnostics.</summary>
        public const int ReadStorage = 700;
        /// <summary>Event ID for storage update-path diagnostics.</summary>
        public const int UpdateStorage = 701;
    }
}
