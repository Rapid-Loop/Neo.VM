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
using Neo.Core.VM.Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents a key/value map stack item. Keys are limited to <see cref="MaxKeySize"/> bytes.
    /// </summary>
    public class VMMap : VMObject, IEquatable<VMMap>, IDictionary<VMObject, VMObject>, IReadOnlyDictionary<VMObject, VMObject>
    {
        /// <summary>
        /// The maximum allowed size of a map key in bytes.
        /// </summary>
        public const int MaxKeySize = 64;

        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Map;

        /// <summary>
        /// Gets the number of key/value pairs in the map.
        /// </summary>
        public int Count => _map.Count;

        /// <summary>
        /// Gets a collection containing the keys of the map.
        /// </summary>
        public ICollection<VMObject> Keys => _map.Keys;

        /// <summary>
        /// Gets a collection containing the values of the map.
        /// </summary>
        public ICollection<VMObject> Values => _map.Values;

        /// <summary>
        /// Gets a value indicating whether the map is read-only.
        /// </summary>
        public bool IsReadOnly => _isReadOnly;

        /// <inheritdoc />
        IEnumerable<VMObject> IReadOnlyDictionary<VMObject, VMObject>.Keys => Keys;

        /// <inheritdoc />
        IEnumerable<VMObject> IReadOnlyDictionary<VMObject, VMObject>.Values => Values;

        private readonly Dictionary<VMObject, VMObject> _map = [];
        private bool _isReadOnly = false;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        public VMObject this[VMObject key]
        {
            get
            {
                if (key.Size > MaxKeySize)
                    throw new ArgumentException($"Key size {key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(key));
                return _map[key];
            }
            set => Add(key, value);
        }

        /// <summary>
        /// Initializes an empty mutable map.
        /// </summary>
        public VMMap() { }

        /// <summary>
        /// Initializes an empty map with the specified mutability.
        /// </summary>
        /// <param name="isReadOnly">Whether the map is immutable.</param>
        public VMMap(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Initializes a map from the specified key/value pairs.
        /// </summary>
        /// <param name="items">The initial entries.</param>
        /// <param name="isReadOnly">Whether the map is immutable after construction.</param>
        public VMMap(IEnumerable<KeyValuePair<VMObject, VMObject>> items, bool isReadOnly = false)
        {
            foreach (var kvp in items)
                Add(kvp);

            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Initializes a map by pairing consecutive items as key/value pairs.
        /// </summary>
        /// <param name="items">A flat list of alternating keys and values.</param>
        /// <param name="isReadOnly">Whether the map is immutable after construction.</param>
        public VMMap(IList<VMObject> items, bool isReadOnly = false)
        {
            for (var i = 0; i < items.Count - 2; i += 2)
                Add(new(items[i], items[i + 1]));

            _isReadOnly = isReadOnly;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear(); // Release all keys and values
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Determines whether this map is equal to another map.
        /// </summary>
        /// <param name="other">The other map.</param>
        /// <returns><see langword="true"/> if the maps are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals(VMMap? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;

            if (other.Count == Count)
            {
                var children = GetChildren().ToArray();
                var otherChildren = other.GetChildren().ToArray();

                for (var i = 0; i < Count; i++)
                {
                    if (children[i].Equals(otherChildren[i]) == false)
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMMap);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return AsSpan().ToHashCode(RefCount * 397);
        }

        /// <summary>
        /// Adds or replaces the value for the specified key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with the key.</param>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the map is read-only.</exception>
        public void Add(VMObject key, VMObject value)
        {
            if (key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(key));

            if (_isReadOnly)
                throw new InvalidOperationException();

            _map[key] = value;
        }

        /// <summary>
        /// Determines whether the map contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><see langword="true"/> if the key is present; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        public bool ContainsKey(VMObject key)
        {
            if (key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(key));

            return key is not VMNull && _map.ContainsKey(key);
        }

        /// <summary>
        /// Removes the entry with the specified key.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns><see langword="true"/> if the entry was removed; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the map is read-only.</exception>
        public bool Remove(VMObject key)
        {
            if (key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(key));

            if (_isReadOnly)
                throw new InvalidOperationException();

            if (_map.TryGetValue(key, out var value))
                value.Release();

            key.Release();
            return _map.Remove(key);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the key, if found.</param>
        /// <returns><see langword="true"/> if the key was found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        public bool TryGetValue(VMObject key, [MaybeNullWhen(false)] out VMObject value)
        {
            if (key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(key));

            return _map.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds the specified key/value pair to the map.
        /// </summary>
        /// <param name="item">The pair to add.</param>
        public void Add(KeyValuePair<VMObject, VMObject> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all entries from the map.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the map is read-only.</exception>
        public void Clear()
        {
            if (_isReadOnly)
                throw new InvalidOperationException();
            _map.Clear();
        }

        /// <summary>
        /// Determines whether the map contains the specified key/value pair.
        /// </summary>
        /// <param name="item">The pair to locate.</param>
        /// <returns><see langword="true"/> if the pair is present; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        public bool Contains(KeyValuePair<VMObject, VMObject> item)
        {
            if (item.Key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {item.Key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(item));

            return ContainsKey(item.Key) && _map.Values.Any(a => a == item.Value);
        }

        /// <summary>
        /// Copies the map entries to an array starting at the specified index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(KeyValuePair<VMObject, VMObject>[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count, nameof(arrayIndex));

            _map.ToArray().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the entry with the specified key from the map.
        /// </summary>
        /// <param name="item">The pair whose key identifies the entry to remove.</param>
        /// <returns><see langword="true"/> if the entry was removed; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the key exceeds <see cref="MaxKeySize"/>.</exception>
        public bool Remove(KeyValuePair<VMObject, VMObject> item)
        {
            if (item.Key.Size > MaxKeySize)
                throw new ArgumentException($"Key size {item.Key.Size} bytes exceeds maximum allowed size of {MaxKeySize} bytes.", nameof(item));

            return Remove(item.Key);
        }

        internal override IEnumerable<VMObject> GetChildren()
        {
            foreach (var kvp in _map)
            {
                yield return kvp.Key;
                yield return kvp.Value;
            }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<VMObject, VMObject>> GetEnumerator() =>
            _map.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        internal void SetAsReadOnly() =>
            _isReadOnly = true;

        /// <inheritdoc />
        public override VMObject Clone()
        {
            var objectMap = new Dictionary<VMObject, VMObject>(ReferenceEqualityComparer.Instance);
            return Clone(objectMap);
        }

        /// <inheritdoc />
        protected override VMObject CloneCore(Dictionary<VMObject, VMObject> objectMap)
        {
            if (objectMap.TryGetValue(this, out var thisItem)) return thisItem;

            var clone = new VMMap();

            objectMap.Add(this, clone);

            foreach (var (key, value) in _map)
                clone[key] = value.Clone(objectMap);

            if (_isReadOnly)
                clone.SetAsReadOnly();

            return clone;
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            var result = new List<byte>();

            foreach (var (key, value) in _map)
            {
                result.AddRange(key.GetSafeSpan(visited));
                result.AddRange(value.GetSafeSpan(visited));
            }

            return result.ToArray();
        }

        /// <inheritdoc />
        public override bool GetBoolean() =>
            _map.Count > 0;

        /// <inheritdoc />
        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            throw new InvalidOperationException();
        }
    }
}
