namespace CodingMonkey.CodeExecutor.CodeExecution
{
    using Models;
    using System.Collections.Generic;

    public class ExecutionCodeBuilder
    {
        private string executionCode { get; set; }

        public ExecutionCodeBuilder()
        {
            executionCode = string.Empty;
        }

        public string Build()
        {
            return this.executionCode;
        }

        public ExecutionCodeBuilder AddReturnKeyword()
        {
            executionCode = executionCode.TrimEnd() + "return";
            return this;
        }

        public ExecutionCodeBuilder CreateNewClassInstance(string className)
        {
            executionCode = executionCode.TrimEnd() + $" new {className}()";
            return this;
        }

        public ExecutionCodeBuilder CallMethod(string methodName, IEnumerable<TestInput> methodArgs = null)
        {
            if(methodArgs == null)
            {
                executionCode = executionCode.TrimEnd() + $".{methodName}()";
                return this;
            }

            executionCode = executionCode.TrimEnd() + $".{methodName}(";
            this.AddMethodArgsToCallingStatement(methodArgs);

            return this;
        }

        private void AddMethodArgsToCallingStatement(IEnumerable<TestInput> methodArgs)
        {
            foreach (var args in methodArgs)
            {
                if (args.ValueType == "String")
                {
                    executionCode += $"{args.ArgumentName}: \"{args.Value.ToString()}\",";
                }
                else
                {
                    executionCode += $"{args.ArgumentName}: {args.Value.ToString()},";
                }
            }

            executionCode = executionCode.TrimEnd(',') + ");";
        }
    }
}
