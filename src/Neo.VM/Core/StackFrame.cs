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

using Neo.VM.Types;
using System;
using System.Collections.Generic;

namespace Neo.VM.Core
{
    /// <summary>
    /// Holds the evaluation stack, locals, static fields, and arguments for a single call frame.
    /// </summary>
    /// <param name="parent">The parent frame for nested calls, or <see langword="null"/> for a root frame.</param>
    public class StackFrame(StackFrame? parent = default) : IDisposable
    {
        /// <summary>
        /// Gets the evaluation stack for this frame.
        /// </summary>
        public Stack<VMObject> EvaluationStack { get; } = [];

        /// <summary>
        /// Gets the local variables for this frame (indexed).
        /// </summary>
        public List<VMObject> LocalVariables { get; } = [];

        /// <summary>
        /// Gets the static fields associated with this frame.
        /// </summary>
        public List<VMObject> StaticFields { get; } = [];

        /// <summary>
        /// Gets the list used to store the arguments of the current method.
        /// </summary>
        public List<VMObject> Arguments { get; } = [];

        /// <summary>
        /// Gets the alternative stack (used by some NeoVM instructions).
        /// </summary>
        public Stack<VMObject> AltStack { get; } = [];

        /// <summary>
        /// Gets the parent frame for nested calls.
        /// </summary>
        public StackFrame? Parent { get; } = parent;

        /// <summary>
        /// Gets a value indicating whether this frame is still active.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Releases stack resources held by this frame.
        /// </summary>
        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Pushes an item onto the evaluation stack with optional reference counting.
        /// </summary>
        /// <param name="item">The item to push.</param>
        /// <param name="addReferenceItem">Whether to increment the item's reference count.</param>
        /// <param name="addReferenceChildren">Whether to increment reference counts of child items.</param>
        public void Push(VMObject item, bool addReferenceItem = true, bool addReferenceChildren = true)
        {
            if (addReferenceItem)
                item.AddReference();

            if (addReferenceChildren)
            {
                foreach (var subItem in item.GetChildren())
                {
                    if (ReferenceEquals(item, subItem)) continue;
                    subItem.AddReference();
                }
            }

            EvaluationStack.Push(item);
        }

        /// <summary>
        /// Pops an item from the evaluation stack and optionally releases references.
        /// </summary>
        /// <param name="releaseReferenceItem">Whether to release the item's reference count.</param>
        /// <param name="releaseReferenceChildren">Whether to release reference counts of child items.</param>
        /// <returns>The popped item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the evaluation stack is empty.</exception>
        public VMObject Pop(bool releaseReferenceItem = true, bool releaseReferenceChildren = true)
        {
            if (EvaluationStack.Count == 0)
                throw new InvalidOperationException("Evaluation stack underflow");

            var item = EvaluationStack.Pop();

            if (releaseReferenceItem)
                item.Release();

            if (releaseReferenceChildren)
            {
                foreach (var subItem in item.GetChildren())
                {
                    if (ReferenceEquals(item, subItem)) continue;
                    subItem.Release();
                }
            }

            return item;
        }

        /// <summary>
        /// Peeks at an evaluation-stack item by depth without removing it.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <returns>The stack item at the specified depth.</returns>
        public VMObject Peek(int index = 0)
        {
            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)
            return list[index];
        }

        /// <summary>
        /// Inserts an item into the evaluation stack at the specified depth.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, VMObject item)
        {
            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)

            list.Insert(index, item);

            EvaluationStack.Clear();

            for (var i = list.Count - 1; i >= 0; i--)
                EvaluationStack.Push(list[i]);

            list.Clear();

            item.AddReference();
        }

        /// <summary>
        /// Swaps two items on the evaluation stack by depth.
        /// </summary>
        /// <param name="fromIndex">The first depth index.</param>
        /// <param name="toIndex">The second depth index.</param>
        public void Swap(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;

            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)
            (list[fromIndex], list[toIndex]) = (list[toIndex], list[fromIndex]);

            EvaluationStack.Clear();

            for (var i = list.Count - 1; i >= 0; i--)
                EvaluationStack.Push(list[i]);

            list.Clear();
        }

        /// <summary>
        /// Reverses the order of the top <paramref name="n"/> items on the evaluation stack.
        /// </summary>
        /// <param name="n">The number of items from the top to reverse.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is outside the stack bounds.</exception>
        public void Reverse(int n)
        {
            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)

            if (n < 0 || n > list.Count)
                throw new ArgumentOutOfRangeException(nameof(n), $"Out of stack bounds: {n}/{list.Count}");

            if (n <= 1) return;

            list.Reverse(list.Count - n, n);

            EvaluationStack.Clear();

            for (var i = list.Count - 1; i >= 0; i--)
                EvaluationStack.Push(list[i]);

            list.Clear();
        }


        /// <summary>
        /// Removes and returns the evaluation-stack item at the specified depth.
        /// </summary>
        /// <param name="index">Zero-based depth from the top of the stack.</param>
        /// <returns>The removed item.</returns>
        public VMObject RemoveAt(int index)
        {
            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)
            var removed = list[index];

            list.RemoveAt(index);
            EvaluationStack.Clear();

            for (var i = list.Count - 1; i >= 0; i--)
                EvaluationStack.Push(list[i]);

            list.Clear();

            removed.Release();
            return removed;
        }

        /// <summary>
        /// Clears the evaluation stack and releases all references.
        /// </summary>
        public void Clear()
        {
            while (EvaluationStack.Count > 0)
            {
                var item = EvaluationStack.Pop();
                item?.Release();
            }
        }

        /// <summary>
        /// Ensures the local-variable list has at least the specified number of slots, filling with <see cref="VMNull.Instance"/>.
        /// </summary>
        /// <param name="count">The required number of local slots.</param>
        public void InitLocalVariables(int count)
        {
            while (LocalVariables.Count < count)
                LocalVariables.Add(VMNull.Instance);
        }

        /// <summary>
        /// Sets the local variable at the specified index, updating reference counts.
        /// </summary>
        /// <param name="index">The local-variable index.</param>
        /// <param name="value">The value to store.</param>
        public void SetLocalVariable(int index, VMObject value)
        {
            // Expand locals list if needed
            while (LocalVariables.Count < index)
                LocalVariables.Add(VMNull.Instance);

            // Release old value
            LocalVariables[index]?.Release();

            // Add new value
            value.AddReference();
            LocalVariables[index] = value;
        }

        /// <summary>
        /// Ensures the static-field list has at least the specified number of slots, filling with <see cref="VMNull.Instance"/>.
        /// </summary>
        /// <param name="count">The required number of static-field slots.</param>
        public void InitStaticFields(int count)
        {
            while (StaticFields.Count < count)
                StaticFields.Add(VMNull.Instance);
        }

        /// <summary>
        /// Sets the static field at the specified index, updating reference counts.
        /// </summary>
        /// <param name="index">The static-field index.</param>
        /// <param name="value">The value to store.</param>
        public void SetStaticField(int index, VMObject value)
        {
            StaticFields[index]?.Release();

            // Add new value
            value.AddReference();
            StaticFields[index] = value;
        }

        /// <summary>
        /// Ensures the argument list has at least the specified number of slots, filling with <see cref="VMNull.Instance"/>.
        /// </summary>
        /// <param name="count">The required number of argument slots.</param>
        public void InitArguments(int count)
        {
            while (Arguments.Count < count)
                Arguments.Add(VMNull.Instance);
        }

        /// <summary>
        /// Sets the argument at the specified index, updating reference counts.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">The value to store.</param>
        public void SetArguments(int index, VMObject value)
        {
            while (Arguments.Count < index)
                Arguments.Add(VMNull.Instance);

            Arguments[index]?.Release();

            // Add new value
            value.AddReference();
            Arguments[index] = value;
        }

        /// <summary>
        /// Gets the local variable at the specified index, or <see cref="VMNull.Instance"/> if out of range.
        /// </summary>
        /// <param name="index">The local-variable index.</param>
        /// <returns>The local value, or null when the index is invalid.</returns>
        public VMObject GetLocal(int index)
        {
            return (index >= 0 && index < LocalVariables.Count) ? LocalVariables[index] : VMNull.Instance;
        }

        /// <summary>
        /// Gets the static field at the specified index, or <see cref="VMNull.Instance"/> if out of range.
        /// </summary>
        /// <param name="index">The static-field index.</param>
        /// <returns>The static-field value, or null when the index is invalid.</returns>
        public VMObject GetStaticFields(int index)
        {
            return (index >= 0 && index < StaticFields.Count) ? StaticFields[index] : VMNull.Instance;
        }

        /// <summary>
        /// Gets the argument at the specified index, or <see cref="VMNull.Instance"/> if out of range.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns>The argument value, or null when the index is invalid.</returns>
        public VMObject GetArguments(int index)
        {
            return (index >= 0 && index < Arguments.Count) ? Arguments[index] : VMNull.Instance;
        }

        /// <summary>
        /// Determines whether this stack frame contains any circular references among its stack items.
        /// </summary>
        /// <returns><see langword="true"/> if a cycle is detected; otherwise <see langword="false"/>.</returns>
        public bool HasCircularReference()
        {
            var visited = new HashSet<VMObject>(ReferenceEqualityComparer.Instance);
            return DetectCycleInFrame(this, visited);
        }

        /// <summary>
        /// Releases all references held by this frame and marks it inactive.
        /// </summary>
        public void Cleanup()
        {
            // Release evaluation stack
            Clear();

            // Release alt stack
            while (AltStack.Count > 0)
            {
                var item = AltStack.Pop();
                item?.Release();
            }

            // Release locals
            for (var i = 0; i < LocalVariables.Count; i++)
            {
                LocalVariables[i]?.Release();
                LocalVariables[i] = VMNull.Instance;
            }

            // Release Static Fields
            for (var i = 0; i < StaticFields.Count; i++)
            {
                StaticFields[i]?.Release();
                StaticFields[i] = VMNull.Instance;
            }

            IsActive = false;
        }

        private static bool DetectCycleInFrame(StackFrame frame, HashSet<VMObject> visited)
        {
            if (frame == null) return false;

            // Check Evaluation Stack
            foreach (var item in frame.EvaluationStack)
            {
                if (DetectCycle(item, visited))
                    return true;
            }

            // Check Alt Stack
            foreach (var item in frame.AltStack)
            {
                if (DetectCycle(item, visited))
                    return true;
            }

            // Check Locals
            foreach (var item in frame.LocalVariables)
            {
                if (DetectCycle(item, visited))
                    return true;
            }

            // Check Static Fields
            foreach (var item in frame.StaticFields)
            {
                if (DetectCycle(item, visited))
                    return true;
            }

            return false;
        }

        private static bool DetectCycle(VMObject current, HashSet<VMObject> visited)
        {
            if (current == null) return false;
            if (visited.Contains(current)) return true;   // Cycle found!

            visited.Add(current);

            // Recursively check children
            foreach (var child in current.GetChildren())
            {
                if (DetectCycle(child, visited))
                    return true;
            }

            visited.Remove(current);
            return false;
        }
    }
}
