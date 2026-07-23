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

namespace Neo.Platform.Storage.Logging
{
    /// <summary>
    /// Event id constants for blockchain store logging.
    /// </summary>
    internal static class StoreEventId
    {
        /// <summary>Event id for store fault logs.</summary>
        public const int Fault = 100;
        /// <summary>Event id for store read logs.</summary>
        public const int Read = 200;
        /// <summary>Event id for store write logs.</summary>
        public const int Write = 300;
        /// <summary>Event id for store delete logs.</summary>
        public const int Delete = 400;
        /// <summary>Event id for store snapshot logs.</summary>
        public const int Snapshot = 500;
        /// <summary>Event id for store commit logs.</summary>
        public const int Commit = 600;
        /// <summary>Event id for store restore logs.</summary>
        public const int Restore = 700;
        /// <summary>Event id for store backup logs.</summary>
        public const int Backup = 800;
        /// <summary>Event id for store checkpoint logs.</summary>
        public const int Checkpoint = 900;
    }
}
