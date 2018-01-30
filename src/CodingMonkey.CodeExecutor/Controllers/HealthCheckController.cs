namespace CodingMonkey.CodeExecutor.Controllers
{
    using System.Threading.Tasks;
    using CodingMonkey.CodeExecutor.CodeExecution;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using CodingMonkey.CodeExecutor.Structs;
    using System;
    using Microsoft.Extensions.Options;
    using CodingMonkey.CodeExecutor.Configuration;
    using System.Net.Http;

    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        const string code = @"
                            public class Test
                            {
                                public int TestCode()
                                {
                                    return 1;
                                }
                            }
                        ";

        const string expectedReturnValue = "1";

        private IOptions<IdentityServerConfig> _identityServerConfig { get; set; }

        public HealthCheckController(IOptions<IdentityServerConfig> identityServerConfig)
        {
            this._identityServerConfig = identityServerConfig;
        }

        // GET: /<controller>/
        public async Task<JsonResult> IndexAsync()
        {
            bool isServiceOK = true;

            try
            {
                this.CompileHealthcheck();
                await this.ExecuteHealthcheckAsync();
                await this.IdentityServerHealthcheckAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Fatal(e, "CodeExecutor service failed healthcheck");
                throw e;
            }

            return Json(new
            {
                OK = isServiceOK
            });
        }

        private async Task IdentityServerHealthcheckAsync()
        {
            try
            {
                var baseAddress = this._identityServerConfig.Value.Authority;
                var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };

                var response = await httpClient.GetAsync("/.well-known/openid-configuration");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Could not connect to identity server");
                }
            }
            catch (Exception e)
            {
                Log.Fatal("Healthcheck: failed to compile code in code executor.");
                throw e;
            }
        }

        private void CompileHealthcheck()
        {
            try
            {
                new RoslynCompiler().Compile(code);
            }
            catch (Exception e)
            {
                Log.Fatal("Healthcheck: failed to compile code in code executor.");
                throw e;
            }
            
        }

        private async Task ExecuteHealthcheckAsync()
        {
            try
            {
                ExecutionResult result = await new RoslynCompiler().SanitiseCodeAndExecuteAsync(code,
                "Test",
                "TestCode",
                new System.Collections.Generic.List<Models.TestInput>());

                if (result.Value.ToString() != expectedReturnValue)
                {
                    throw new Exception("Execute healthcheck failed to return correct value");
                }
            }
            catch (Exception e)
            {
                Log.Fatal("Healthcheck: failed to execute code in code executor.");
                throw e;
            }
        }
    }
}
