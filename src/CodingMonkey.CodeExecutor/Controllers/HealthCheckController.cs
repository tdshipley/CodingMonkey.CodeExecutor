namespace CodingMonkey.CodeExecutor.Controllers
{
    using System.Threading.Tasks;
    using CodingMonkey.CodeExecutor.CodeExecution;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using CodingMonkey.CodeExecutor.Structs;
    using System;

    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        const string code = @"
                            public class Test
                            {
                                public int TestMethod()
                                {
                                    return 1;
                                }
                            }
                        ";

        const string expectedReturnValue = "1";

        // GET: /<controller>/
        public async Task<JsonResult> IndexAsync()
        {
            bool isServiceOK = true;

            try
            {
                this.CompileHealthcheck();
                await this.ExecuteHealthcheckAsync();
            }
            catch (System.Exception e)
            {
                Log.Logger.Fatal(e, "CodeExecutor service failed healthcheck");
                isServiceOK = false;
            }

            return Json(new
            {
                OK = isServiceOK
            });
        }

        private void CompileHealthcheck()
        {
            new RoslynCompiler().Compile(code);
        }

        private async Task ExecuteHealthcheckAsync()
        {
            ExecutionResult result = await new RoslynCompiler().SanitiseCodeAndExecuteAsync(code,
                "Test",
                "TestMethod",
                new System.Collections.Generic.List<Models.TestInput>());

            if(result.Value.ToString() != expectedReturnValue)
            {
                throw new Exception("Execute healthcheck failed to return correct value");
            }
        }
    }
}
