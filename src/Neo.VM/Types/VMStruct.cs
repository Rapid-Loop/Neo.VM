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

using Neo.Core.VM.Type;
using System.Collections.Generic;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents a NeoVM structure: a value-type compound similar to <see cref="VMArray"/>
    /// that deep-clones its fields when copied.
    /// </summary>
    /// <param name="items">The initial field values.</param>
    public class VMStruct(IEnumerable<VMObject> items) : VMArray(items)
    {
        /// <inheritdoc />
        public override VMObjectType Type => VMObjectType.Struct;

        /// <summary>
        /// Initializes an empty structure.
        /// </summary>
        public VMStruct() : this([]) { }

        /// <inheritdoc />
        protected override VMObject CloneCore(Dictionary<VMObject, VMObject> objectMap)
        {
            if (objectMap.TryGetValue(this, out var thisItem)) return thisItem;

            var clone = new VMStruct();

            objectMap.Add(this, clone);

            foreach (var item in this)
            {
                var clonedItem = item.Clone(objectMap);
                clone.Array.Add(clonedItem);
            }

            return clone;
        }
    }
}
