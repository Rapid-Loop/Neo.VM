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

namespace Neo.Core.Storage
{
    /// <summary>
    /// A mutable key/value store that supports snapshots, backups, and checkpoints.
    /// </summary>
    public interface IStore : IReadOnlyStore
    {
        /// <summary>
        /// Creates a snapshot of the current store state.
        /// </summary>
        /// <returns>A disposable store snapshot.</returns>
        IStoreSnapshot CreateSnapshot();

        /// <summary>
        /// Creates a backup handle for this store.
        /// </summary>
        /// <returns>A disposable backup handle.</returns>
        IStoreBackup CreateBackup();

        /// <summary>
        /// Creates a checkpoint of the store under the specified directory.
        /// </summary>
        /// <param name="checkpointDirectory">The directory that receives the checkpoint.</param>
        void CreateCheckpoint(string checkpointDirectory);

        /// <summary>
        /// Puts or replaces a value for the specified key.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to associate with <paramref name="key"/>.</param>
        void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);

        /// <summary>
        /// Deletes the value for the specified key, if present.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        void Delete(ReadOnlySpan<byte> key);
    }
}
