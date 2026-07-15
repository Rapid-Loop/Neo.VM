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

using Microsoft.Extensions.DependencyInjection;
using Neo.Core.VM;
using Neo.VM.Middleware;
using Neo.VM.Tests.TestData;
using System.Collections.Generic;

namespace Neo.VM.Tests.Middleware
{
    [TestClass]
    public class UT_DebuggerMiddleware
    {
        private static List<VMTestSuite> s_suite = [];

        [AssemblyInitialize]
        public static void InitTestData(TestContext testContext)
        {
            s_suite = VMTestLoader.LoadAllTests();
        }

        [TestMethod]
        public void TestJsonFiles()
        {
            foreach (var testSuite in s_suite)
            {
                if (testSuite.Name.StartsWith("TRY_")) continue;

                foreach (var jsonTest in testSuite.Tests)
                {
                    var script = OpCodeAssembler.Assemble(jsonTest.Script);

                    using var scope = TestUtilities.Services.CreateScope();
                    using var vm = scope.ServiceProvider.GetRequiredService<TestEngine>();

                    vm.LoadScript(script);

                    var actualState = vm.Execute();
                    var actualResultStack = vm.ResultStack;

                    Assert.AreEqual(jsonTest.State, actualState, $"\nTest failed: {testSuite.Name} - {jsonTest.Name} {vm.FaultException}");
                    Assert.HasCount(jsonTest.ResultStack.Count, actualResultStack);
                }
            }
        }

        [TestMethod]
        public void TestBreakpoints()
        {
            var expectedBreakLine = 2;

            using var sb = new ScriptBuilder()
                .EmitPush(1)
                .EmitPush(2)
                .Emit(OpCode.ADD)
                .EmitReturn();

            var script = sb.ToArray();

            using var scope = TestUtilities.Services.CreateScope();
            using var vm = scope.ServiceProvider.GetRequiredService<VirtualMachineEngine>();
            vm.LoadScript(script);

            // Get the debugger that is inside the VM
            var debugger = scope.ServiceProvider.GetRequiredService<DebuggerMiddleware>();

            debugger.AddBreakpoint(expectedBreakLine);

            debugger.OnBreakpoint += (sender, e) =>
            {
                var actualContext = e.Context;

                Assert.AreEqual(expectedBreakLine, actualContext.InstructionPointer);

                debugger.StepMode = false;
                debugger.Continue();
            };

            var actualState = vm.Execute();
            var actualResults = vm.ResultStack;

            Assert.AreEqual(VMState.HALT, actualState);
            Assert.HasCount(1, actualResults);
            Assert.AreEqual(3, actualResults.Pop().GetInteger());
        }
    }
}
