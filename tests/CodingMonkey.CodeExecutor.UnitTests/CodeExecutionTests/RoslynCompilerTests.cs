namespace CodingMonkey.CodeExecutor.UnitTests.CodeExecutionTests
{
    using System;
    using Xunit;
    using CodingMonkey.CodeExecutor.CodeExecution;
    using System.Collections.Generic;

    public class RoslynCompilerTests
    {
        [Fact]
        public void Assert_Compile_Returns_Errors_For_Invalid_Code()
        {
            // Arrange
            var roslynCompiler = new RoslynCompiler();
            string invalidCode = @"namespace MyTestCode
                                    {
                                        using System;

                                        public class
                                        {
                                            // Mising paren
                                            public void MyMethod(
                                            {
                                                // Do nothing
                                            }
                                        }
                                    }";

            // Act
            var compilerErrors = roslynCompiler.Compile(invalidCode);

            // Assert
            Assert.Equal(5, compilerErrors.Count);
        }

        public void Assert_Compile_Returns_No_Errors_For_Valid_Code()
        {
            // Arrange
            var roslynCompiler = new RoslynCompiler();
            string validCode = @"namespace MyTestCode
                                {
                                    using System;

                                    public class
                                    {
                                        // Mising paren
                                        public void MyMethod()
                                        {
                                            // Do nothing
                                        }
                                    }
                                }";

            // Act
            var compilerErrors = roslynCompiler.Compile(validCode);

            // Assert
            Assert.Equal(0, compilerErrors.Count);
        }
    }
}