namespace CodingMonkey.CodeExecutor.Models
{
    using System.Collections.Generic;

    public class Test
    {
        public string Description { get; set; }
        public List<TestInput> Inputs { get; set; }
        public TestExpectedOutput ExpectedOutput { get; set; }
        public object ActualOutput { get; set; }
        public TestResult Result { get; set; }
    }
}
