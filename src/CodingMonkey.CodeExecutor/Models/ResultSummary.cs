using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingMonkey.CodeExecutor.Models
{
    public class ResultSummary
    {
        public ResultSummary()
        {
            CompilerErrors = new List<CompilerError>();
        }

        public bool AllTestsExecuted { get; set; }
        public bool ErrorProcessingTests { get; set; }
        public bool HasCompilerErrors { get; set; }
        public bool HasRuntimeError { get; set; }
        public IList<CompilerError> CompilerErrors { get; set; }
        public RuntimeError RuntimeError { get; set; }
    }
}
