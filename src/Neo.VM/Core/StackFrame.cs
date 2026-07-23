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
    public class StackFrame(StackFrame? parent = default) : IDisposable
    {
        /// <summary>
        /// The evaluation stack for this frame
        /// </summary>
        public Stack<VMObject> EvaluationStack { get; } = [];

        /// <summary>
        /// Local variables for this frame (indexed)
        /// </summary>
        public List<VMObject> LocalVariables { get; } = [];

        public List<VMObject> StaticFields { get; } = [];

        /// <summary>
        /// The list used to store the arguments of the current method.
        /// </summary>
        public List<VMObject> Arguments { get; } = [];

        /// <summary>
        /// Alternative stack (used by some NeoVM instructions)
        /// </summary>
        public Stack<VMObject> AltStack { get; } = [];

        /// <summary>
        /// Parent frame (for nested calls)
        /// </summary>
        public StackFrame? Parent { get; } = parent;

        /// <summary>
        /// Whether this frame is currently executing
        /// </summary>
        public bool IsActive { get; private set; } = true;

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Push item onto evaluation stack with reference counting
        /// </summary>
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
        /// Pop item from evaluation stack and release reference
        /// </summary>
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
        /// Peek from top of evaluation stack without removing by an index
        /// </summary>
        public VMObject Peek(int index = 0)
        {
            var list = new List<VMObject>(EvaluationStack); // Copy to list (top is at the end)
            return list[index];
        }

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
        /// Safely clear a evaluation stack and release all references
        /// </summary>
        public void Clear()
        {
            while (EvaluationStack.Count > 0)
            {
                var item = EvaluationStack.Pop();
                item?.Release();
            }
        }

        public void InitLocalVariables(int count)
        {
            while (LocalVariables.Count < count)
                LocalVariables.Add(VMNull.Instance);
        }

        /// <summary>
        /// Set local variable at index (with proper ref counting)
        /// </summary>
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

        public void InitStaticFields(int count)
        {
            while (StaticFields.Count < count)
                StaticFields.Add(VMNull.Instance);
        }

        public void SetStaticField(int index, VMObject value)
        {
            StaticFields[index]?.Release();

            // Add new value
            value.AddReference();
            StaticFields[index] = value;
        }

        public void InitArguments(int count)
        {
            while (Arguments.Count < count)
                Arguments.Add(VMNull.Instance);
        }

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
        /// Get local variable at index
        /// </summary>
        public VMObject GetLocal(int index)
        {
            return (index >= 0 && index < LocalVariables.Count) ? LocalVariables[index] : VMNull.Instance;
        }

        public VMObject GetStaticFields(int index)
        {
            return (index >= 0 && index < StaticFields.Count) ? StaticFields[index] : VMNull.Instance;
        }

        public VMObject GetArguments(int index)
        {
            return (index >= 0 && index < Arguments.Count) ? Arguments[index] : VMNull.Instance;
        }

        /// <summary>
        /// Checks if this stack frame contains any circular references
        /// </summary>
        public bool HasCircularReference()
        {
            var visited = new HashSet<VMObject>(ReferenceEqualityComparer.Instance);
            return DetectCycleInFrame(this, visited);
        }

        /// <summary>
        /// Clean up this frame (release all references)
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
