namespace CodingMonkey.CodeExecutor.UnitTests.CodeExecutionTests
{
    using System;
    using Xunit;
    using CodingMonkey.CodeExecutor.CodeExecution;
    using CodingMonkey.CodeExecutor.Models;
    using System.Collections.Generic;

    public class ExecutionCodeBuilderTests
    {
        [Fact]
        public void Assert_Code_Builder_Returns_Correct_Code_String()
        {
            // Arrange
            var executionCodeBuilder = new ExecutionCodeBuilder();
            string expectedResult = "return new TestClass().MyMethod(firstArg: \"MyString\",secondArg: 1);";
            List<TestInput> inputs = new List<TestInput>{
                new TestInput(){
                    ArgumentName = "firstArg",
                    Value = "MyString",
                    ValueType = "String"
                },
                new TestInput(){
                    ArgumentName = "secondArg",
                    Value = "1",
                    ValueType = "Integer"
                }
            };

            // Act
            string actualResult = executionCodeBuilder.AddReturnKeyword()
                                                      .CreateNewClassInstance("TestClass")
                                                      .CallMethod("MyMethod", inputs)
                                                      .Build();

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}