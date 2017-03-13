﻿namespace CodingMonkey.CodeExecutor.CodeExecution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using CodingMonkey.CodeExecutor.Models;
    using CodingMonkey.CodeExecutor.Security;
    using CodingMonkey.CodeExecutor.Structs;

    public class RoslynCompiler
    {
        public PreExecutionSecurity Security { get; set; }

        public RoslynCompiler()
        {
            this.Security = new PreExecutionSecurity();
        }

        public IList<CompilerError> Compile(string code)
        {
            try
            {
                code = this.Security.SanitiseCode(code);
            }
            catch (Exception ex)
            {
                CompilerError error = new CompilerError(ex);
                return new List<CompilerError>()
                           {
                               error
                           };
            }

            var script = CSharpScript.Create(code);
            return CheckScriptForErrors(script);
        }

        public async Task<ExecutionResult> ExecuteAsync(string code, string className, string mainMethodName, List<TestInput> inputs, int timeoutSeconds = 15)
        {
            string executionCode = this.CreateExecutionCode(className, mainMethodName, inputs);

            try
            {
                code = this.Security.SanitiseCode(code);
                var returnValue = await this.ExecuteCodeWithTimeoutAsync(1000 * timeoutSeconds, code, executionCode);
                return new ExecutionResult() { Successful = true, Value = returnValue, Error = null };
            }
            catch (Exception ex)
            {
                return new ExecutionResult() { Successful = false, Value = null, Error = ex };
                throw;
            }
        }

        private async Task<object> ExecuteCodeWithTimeoutAsync(int timeoutMilliseconds, string code, string executionCode)
        {
            object returnValue = null;
            Exception userSubmittedCodeRuntimeException = null;

            var task = new Task(
                async () =>
                    {
                        ScriptOptions scriptOptions = this.GetScriptOptions();
                        var script = CSharpScript.Create(code).ContinueWith(executionCode);

                        try
                        {
                            returnValue = (await script.WithOptions(scriptOptions).RunAsync()).ReturnValue;
                        }
                        catch (Exception ex)
                        {
                            userSubmittedCodeRuntimeException = ex;
                        }
                    });

            Task completedTask;
            task.Start();
            completedTask = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds));

            if (completedTask == task)
            {
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is rethrown.
                await task;

                if (userSubmittedCodeRuntimeException != null)
                {
                    throw userSubmittedCodeRuntimeException;
                }
            }
            else
            {
                int timeoutInSeconds = timeoutMilliseconds / 1000;
                throw new TimeoutException($"Code failed to complete execution before timeout of {timeoutInSeconds.ToString()} seconds.");
            }

            return returnValue;
        }

        private List<CompilerError> CheckScriptForErrors(Script code)
        {
            List<CompilerError> errors = new List<CompilerError>();
            IList<Diagnostic> errorsFromSource = code.Compile();

            // If no errors - do less work!
            if(errorsFromSource.Count <= 0)
            {
                return errors;
            }

            errors = errorsFromSource.Select(error => new CompilerError(error)).ToList();

            // The user doesn't see using statements added by pre security checks
            // so move the error line numbers to the right place.
            foreach (var error in errors)
            {
                error.StartLineNumber = error.StartLineNumber - this.Security.LinesOfCodeAdded;
                error.EndLineNumber = error.EndLineNumber - this.Security.LinesOfCodeAdded;
            }

            return errors;
        }

        private string CreateExecutionCode(string className, string mainMethodName, List<TestInput> inputs)
        {
            // Statements need a return in front of them to get the value see:
            // https://github.com/dotnet/roslyn/issues/5279
            string executionCode = $"return new {className}().{mainMethodName}(";

            foreach (var input in inputs)
            {
                if (input.ValueType == "String")
                {
                    executionCode += $"{input.ArgumentName}: \"{input.Value.ToString()}\",";
                }
                else
                {
                    executionCode += $"{input.ArgumentName}: {input.Value.ToString()},";
                }
            }

            executionCode = executionCode.TrimEnd(',') + ");";

            return executionCode;
        }

        /// <summary>
        /// Gets a script options for rosyln to use when running code.
        /// Sets up the dlls which we allow the code to run with
        /// and a custom metadata resolver to stop it trying to find
        /// missing assemblies. 
        /// </summary>
        /// <returns>Script options to use when running code</returns>
        private ScriptOptions GetScriptOptions()
        {
            ScriptOptions scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithMetadataResolver(new CodingMonkeyMetadataReferenceResolver());

            //Add reference to mscorlib & system core
            var mscorlib = typeof(object).GetTypeInfo().Assembly;
            var systemCore = typeof(Enumerable).GetTypeInfo().Assembly;

            return scriptOptions.WithReferences(mscorlib, systemCore);
        }
    }
}
