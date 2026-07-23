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
using System.Numerics;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents an ordered collection of <see cref="VMObject"/> items on the evaluation stack.
    /// </summary>
    /// <param name="items">The initial items in the array.</param>
    /// <param name="isReadonly">Whether the array is immutable after construction.</param>
    public class VMArray(IEnumerable<VMObject> items, bool isReadonly) : VMObject, IEquatable<VMArray>, IEnumerable<VMObject>, IEnumerable, ICollection<VMObject>, IReadOnlyCollection<VMObject>, IList<VMObject>, IReadOnlyList<VMObject>
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Array;

        /// <summary>
        /// Gets the number of items in the array.
        /// </summary>
        public int Count => Array.Count;

        /// <summary>
        /// Gets a value indicating whether the array is read-only.
        /// </summary>
        public bool IsReadOnly => _isReadOnly;

        private bool _isReadOnly = isReadonly;

        /// <summary>
        /// The underlying list of items.
        /// </summary>
        protected readonly List<VMObject> Array = [.. items];

        /// <summary>
        /// Initializes an empty mutable array.
        /// </summary>
        public VMArray() : this(false) { }

        /// <summary>
        /// Initializes an empty array with the specified mutability.
        /// </summary>
        /// <param name="isReadonly">Whether the array is immutable.</param>
        public VMArray(bool isReadonly) : this([], isReadonly) { }

        /// <summary>
        /// Initializes a mutable array with the specified items.
        /// </summary>
        /// <param name="items">The initial items.</param>
        public VMArray(IEnumerable<VMObject> items) : this(items, false) { }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="InvalidOperationException">Thrown when setting a value on a read-only array.</exception>
        public VMObject this[int index]
        {
            get => Array[index];
            set
            {
                if (_isReadOnly) throw new InvalidOperationException();
                Array[index] = value;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear(); // Release all items
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return AsSpan().ToHashCode(RefCount ^ 397);
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMArray);
        }

        /// <summary>
        /// Determines whether this array is equal to another array.
        /// </summary>
        /// <param name="other">The other array.</param>
        /// <returns><see langword="true"/> if the arrays are equal; otherwise <see langword="false"/>.</returns>
        public bool Equals(VMArray? other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (RefCount != other.RefCount) return false;

            if (other.Count == Count)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (this[i].Equals(other[i]) == false)
                        return false;
                }

                return true;
            }

            return false;
        }

        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<VMObject> GetEnumerator() =>
            Array.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        #endregion

        #region ICollection

        /// <summary>
        /// Adds an item to the end of the array.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public void Add(VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Add(item);
        }

        /// <summary>
        /// Removes all items from the array.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public void Clear()
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Clear();
        }

        /// <summary>
        /// Determines whether the array contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns><see langword="true"/> if the item is found; otherwise <see langword="false"/>.</returns>
        public bool Contains(VMObject item)
        {
            return Array.Contains(item);
        }

        /// <summary>
        /// Copies the items of the array to a destination array starting at the specified index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(VMObject[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count, nameof(arrayIndex));

            Array.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of the specified item from the array.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if the item was removed; otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public bool Remove(VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            return Array.Remove(item);
        }

        #endregion

        #region IList

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified item.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of the item, or -1 if not found.</returns>
        public int IndexOf(VMObject item)
        {
            return Array.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the array at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert.</param>
        /// <param name="item">The item to insert.</param>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public void Insert(int index, VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public void RemoveAt(int index)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.RemoveAt(index);
        }

        #endregion

        internal override IEnumerable<VMObject> GetChildren() =>
            [.. Array];

        /// <summary>
        /// Reverses the order of the items in the array.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the array is read-only.</exception>
        public void Reverse()
        {
            if (_isReadOnly) throw new InvalidOperationException();

            Array.Reverse();
        }

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

            var clone = new VMArray();

            objectMap.Add(this, clone);

            foreach (var item in this)
            {
                var clonedItem = item.Clone(objectMap);
                clone.Array.Add(clonedItem);
            }

            if (_isReadOnly)
                clone.SetAsReadOnly();

            return clone;
        }

        /// <inheritdoc />
        public override bool GetBoolean()
        {
            return Array.Count > 0;
        }

        /// <inheritdoc />
        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        protected override ReadOnlySpan<byte> ComputeSpan(HashSet<VMObject> visited)
        {
            var result = new List<byte>();

            foreach (var item in Array)
            {
                if (item is not VMNull)
                    result.AddRange(item.GetSafeSpan(visited));
            }

            return result.ToArray();
        }
    }
}
