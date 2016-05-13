namespace CodingMonkey.CodeExecutor.Models
{
    using System.Collections.Generic;

    using CodingMonkey.Models;

    public class SubmittedCode
    {
        public SubmittedCode()
        {
            this.Tests = new List<Test>();
        }

        public string Code { get; set; }

        public CodeTemplate CodeTemplate { get; set; }

        public List<Test> Tests { get; set; }

        public ResultSummary ResultSummary { get; set; }
    }
}
