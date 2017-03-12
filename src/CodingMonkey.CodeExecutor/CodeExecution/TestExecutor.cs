﻿namespace CodingMonkey.CodeExecutor.CodeExecution
{
    using CodingMonkey.Models;
    using CodingMonkey.CodeExecutor.Models;
    using Structs;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class TestExecutor
    {
        public static async Task RunTest(string code, CodeTemplate template, Test test, ResultSummary resultSummary)
        {
            var compiler = new RoslynCompiler();
            if (!ConvertInputValueToValueType(test.Inputs))
            {
                resultSummary.ErrorProcessingTests = true;
            }
            else
            {
                ExecutionResult result = await compiler.ExecuteAsync(code, template.ClassName, template.MainMethodName, test.Inputs);
                ProcessTestResult(result, test, resultSummary);
            }
        }

        private static void ProcessTestResult(ExecutionResult result, Test testRun, ResultSummary resultSummary)
        {
            if (!result.Successful)
            {
                resultSummary.CompilerErrors = null;
                resultSummary.HasCompilerErrors = false;
                resultSummary.HasRuntimeError = true;

                resultSummary.RuntimeError = new RuntimeError()
                {
                    Message = result.Error.Message,
                    HelpLink = result.Error.HelpLink
                };
            }

            testRun.ActualOutput = result.Value;
            if (!AddTestResult(testRun))
            {
                resultSummary.ErrorProcessingTests = true;
            }
        }

        private static bool ConvertInputValueToValueType(List<TestInput> testInputs)
        {
            foreach (var input in testInputs)
            {
                switch (input.ValueType)
                {
                    case "String":
                        {
                            return GetValueFromTestInput<string>(input);
                        }
                    case "Integer":
                        {
                            return GetValueFromTestInput<int>(input);
                        }
                    case "Boolean":
                        {
                            return GetValueFromTestInput<bool>(input);
                        }
                    default:
                        {
                            return false;
                        }
                }
            }

            return false;
        }

        private static bool GetValueFromTestInput<T>(TestInput input)
        {
            object value = GetValue<T>(input.Value);
            if (value == null)
            {
                return false;
            }
            else
            {
                input.Value = (T)value;
                return true;
            }
        }

        private static bool AddTestResult(Test test)
        {
            test.Result = new TestResult { TestExecuted = true };

            switch (test.ExpectedOutput.ValueType)
            {
                case "String":
                    {
                        return AddResultToTestResult<string>(test);
                    }
                case "Integer":
                    {
                        return AddResultToTestResult<int>(test);
                    }
                case "Boolean":
                    {
                        return AddResultToTestResult<bool>(test);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool AddResultToTestResult<T>(Test test)
        {
            try
            {
                var value = GetValue<T>(test.ExpectedOutput.Value);
                test.ExpectedOutput.Value = (T)value;

                test.Result.TestPassed = value.Equals((T)test.ActualOutput);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static object GetValue<T>(object value)
        {
            switch (typeof(T).ToString())
            {
                case "System.String":
                    {
                        return value.ToString();
                    }
                case "System.Int32":
                    {
                        int inputValue;
                        bool success = int.TryParse(value.ToString(), out inputValue);

                        if (success)
                        {
                            return inputValue;
                        }

                        return null;
                    }
                case "System.Boolean":
                    {
                        bool inputValue;
                        bool success = bool.TryParse(value.ToString(), out inputValue);

                        if (success)
                        {
                            return inputValue;
                        }

                        return null;
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}