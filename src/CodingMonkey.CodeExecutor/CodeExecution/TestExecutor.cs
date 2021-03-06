﻿namespace CodingMonkey.CodeExecutor.CodeExecution
{
    using Serilog;
    using CodingMonkey.Models;
    using CodingMonkey.CodeExecutor.Models;
    using Structs;
    using System;
    using System.Threading.Tasks;
    using System.Linq;

    public static class TestExecutor
    {
        public static async Task RunTest(string code, CodeTemplate template, Test test, ResultSummary resultSummary)
        {
            var compiler = new RoslynCompiler();

            try
            {
                ExecutionResult result = await compiler.SanitiseCodeAndExecuteAsync(code, template.ClassName, template.MainMethodName, test.Inputs);
                ProcessTestResult(result, test, resultSummary);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to run test");
                resultSummary.ErrorProcessingTests = true;
                throw e;
            }
        }

        private static void ProcessTestResult(ExecutionResult result, Test testRun, ResultSummary resultSummary)
        {
            resultSummary.CompilerErrors = null;
            resultSummary.HasCompilerErrors = false;

            if (!result.Successful)
            {
                resultSummary.HasRuntimeError = true;

                resultSummary.RuntimeError = new RuntimeError()
                {
                    Message = result.Error.Message,
                    HelpLink = result.Error.HelpLink
                };
            }

            testRun.ActualOutput = result.Value;
            testRun.Result = new TestResult { TestExecuted = true };

            testRun.ExpectedOutput.Value = testRun.ExpectedOutput.Value.ToString();

            if(testRun.ExpectedOutput.ValueType == "Boolean")
            {
                testRun.Result.TestPassed = testRun.ExpectedOutput.Value.ToString().ToLower().Equals(testRun.ActualOutput.ToString().ToLower());
            }
            else
            {
                testRun.Result.TestPassed = testRun.ExpectedOutput.Value.ToString().Equals(testRun.ActualOutput.ToString());
            }
        }
    }
}
