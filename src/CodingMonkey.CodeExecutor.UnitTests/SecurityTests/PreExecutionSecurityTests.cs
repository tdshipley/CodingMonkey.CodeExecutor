namespace CodingMonkey.CodeExecutor.UnitTests.SecurityTests
{
    using System;
    using Xunit;
    using CodingMonkey.CodeExecutor.Security;

    public class PreExecutionSecurityTests
    {
        [Fact]
        public void Assert_SanitiseCode_Removes_Banned_Namespace()
        {
            // Arrange
            PreExecutionSecurity preExecutionSecurity = new PreExecutionSecurity();
            string bannedNamespace = "using System.Reflection";
            string codeWithBannedNamespace = @" namespace MyTestCode
                                                {
                                                    " + bannedNamespace + @";

                                                    public class
                                                    {
                                                        public void MyMethod()
                                                        {
                                                            // Do nothing
                                                        }
                                                    }
                                                }";

            // Act
            string sanitisedCode = preExecutionSecurity.SanitiseCode(codeWithBannedNamespace);

            // Assert
            Assert.DoesNotContain(bannedNamespace, sanitisedCode);
        }

        [Fact]
        public void Assert_SanitiseCode_Does_Not_Remove_Approved_Namespace()
        {
            // Arrange
            PreExecutionSecurity preExecutionSecurity = new PreExecutionSecurity();
            string approvedNamespace = "using System";
            string codeWithApprovedNamespace = @" namespace MyTestCode
                                                {
                                                    " + approvedNamespace + @";

                                                    public class
                                                    {
                                                        public void MyMethod()
                                                        {
                                                            // Do nothing
                                                        }
                                                    }
                                                }";

            // Act
            string sanitisedCode = preExecutionSecurity.SanitiseCode(codeWithApprovedNamespace);

            // Assert
            Assert.Contains(approvedNamespace, sanitisedCode);
        }

        [Fact]
        public void Assert_SanitiseCode_Removes_Banned_Type()
        {
            // Arrange
            PreExecutionSecurity preExecutionSecurity = new PreExecutionSecurity();
            string bannedType = "Type";
            string codeWithBannedType = @" namespace MyTestCode
                                            {
                                                public class
                                                {
                                                    public void MyMethod()
                                                    {
                                                        "+ bannedType +@" myType = null;
                                                        // Do nothing
                                                    }
                                                }
                                            }";

            // Act
            string sanitisedCode = preExecutionSecurity.SanitiseCode(codeWithBannedType);

            // Assert
            Assert.DoesNotContain(bannedType, sanitisedCode);
        }

        [Fact]
        public void Assert_SanitiseCode_Removes_Banned_Type_With_Trailing_Dot()
        {
            // Arrange
            PreExecutionSecurity preExecutionSecurity = new PreExecutionSecurity();
            string bannedType = "Type";
            string codeWithBannedType = @" namespace MyTestCode
                                            {
                                                public class
                                                {
                                                    public void MyMethod()
                                                    {
                                                        TypeCode code = Type.GetTypeCode(bool);
                                                        // DoNothing
                                                    }
                                                }
                                            }";

            // Act
            string sanitisedCode = preExecutionSecurity.SanitiseCode(codeWithBannedType);

            // Assert
            Assert.DoesNotContain(bannedType, sanitisedCode);
        }

        [Fact]
        public void Assert_SanitiseCode_Throws_Exception_Past_Max_Iteration_Count()
        {
            // Arrange
            PreExecutionSecurity preExecutionSecurity = new PreExecutionSecurity();
            string codeWithBannedType = @" namespace MyTestCode
                                            {
                                                public class
                                                {
                                                    public void MyMethod()
                                                    {
                                                        TypeCode code = TyTyTyTyTyTyTyTypepepepepepepepe.GetTypeCode(bool);
                                                        // DoNothing
                                                    }
                                                }
                                            }";


            // Act & Assert
            Assert.Throws<TimeoutException>(() => preExecutionSecurity.SanitiseCode(codeWithBannedType, 5));
        }
    }
}
