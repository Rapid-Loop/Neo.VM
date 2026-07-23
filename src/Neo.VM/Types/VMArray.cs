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
    public class VMArray(IEnumerable<VMObject> items, bool isReadonly) : VMObject, IEquatable<VMArray>, IEnumerable<VMObject>, IEnumerable, ICollection<VMObject>, IReadOnlyCollection<VMObject>, IList<VMObject>, IReadOnlyList<VMObject>
    {
        public override VMObjectType Type => VMObjectType.Array;

        public int Count => Array.Count;

        public bool IsReadOnly => _isReadOnly;

        private bool _isReadOnly = isReadonly;

        protected readonly List<VMObject> Array = [.. items];

        public VMArray() : this(false) { }

        public VMArray(bool isReadonly) : this([], isReadonly) { }

        public VMArray(IEnumerable<VMObject> items) : this(items, false) { }

        public VMObject this[int index]
        {
            get => Array[index];
            set
            {
                if (_isReadOnly) throw new InvalidOperationException();
                Array[index] = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear(); // Release all items
            }
            base.Dispose(disposing);
        }

        public override int GetHashCode()
        {
            return AsSpan().ToHashCode(RefCount ^ 397);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null) return false;
            return Equals(obj as VMArray);
        }

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

        public IEnumerator<VMObject> GetEnumerator() =>
            Array.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        #endregion

        #region ICollection

        public void Add(VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Add(item);
        }

        public void Clear()
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Clear();
        }

        public bool Contains(VMObject item)
        {
            return Array.Contains(item);
        }

        public void CopyTo(VMObject[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count, nameof(arrayIndex));

            Array.CopyTo(array, arrayIndex);
        }

        public bool Remove(VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            return Array.Remove(item);
        }

        #endregion

        #region IList

        public int IndexOf(VMObject item)
        {
            return Array.IndexOf(item);
        }

        public void Insert(int index, VMObject item)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (_isReadOnly) throw new InvalidOperationException();
            Array.RemoveAt(index);
        }

        #endregion

        internal override IEnumerable<VMObject> GetChildren() =>
            [.. Array];

        public void Reverse()
        {
            if (_isReadOnly) throw new InvalidOperationException();

            Array.Reverse();
        }

        internal void SetAsReadOnly() =>
            _isReadOnly = true;

        public override VMObject Clone()
        {
            var objectMap = new Dictionary<VMObject, VMObject>(ReferenceEqualityComparer.Instance);
            return Clone(objectMap);
        }

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

        public override bool GetBoolean()
        {
            return Array.Count > 0;
        }

        [DoesNotReturn]
        public override BigInteger GetInteger()
        {
            throw new InvalidOperationException();
        }

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
