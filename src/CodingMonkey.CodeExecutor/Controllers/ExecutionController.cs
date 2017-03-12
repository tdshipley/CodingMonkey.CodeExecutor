namespace CodingMonkey.CodeExecutor.Controllers
{
    using System;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CodingMonkey.CodeExecutor.Models;
    using CodingMonkey.CodeExecutor.Structs;
    using CodingMonkey.CodeExecutor.CodeExecution;
    using CodingMonkey.Models;
    
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [Authorize]
    public class ExecutionController : Controller
    {
        // POST api/execution
        [HttpPost]
        public async Task<JsonResult> Post([FromBody]SubmittedCode submittedCode)
        {
            submittedCode.ResultSummary = new ResultSummary();

            foreach (var test in submittedCode.Tests)
            {
                await
                    TestExecutor.RunTest(
                        submittedCode.Code,
                        submittedCode.CodeTemplate,
                        test,
                        submittedCode.ResultSummary);

                if (submittedCode.ResultSummary.ErrorProcessingTests || submittedCode.ResultSummary.HasRuntimeError)
                {
                    break;
                }
            }

            submittedCode.ResultSummary.AllTestsExecuted = submittedCode.Tests.All(x => x.Result != null && x.Result.TestExecuted);

            return Json(submittedCode);
        }
    }
}
