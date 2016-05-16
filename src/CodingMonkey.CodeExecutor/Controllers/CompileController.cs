namespace CodingMonkey.CodeExecutor.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CodingMonkey.CodeExecutor.Models;

    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Mvc;

    [Route("api/[controller]")]
    [Authorize]
    public class CompileController : Controller
    {

        [HttpPost]
        public async Task<JsonResult> Post([FromBody] SubmittedCode submittedCode)
        {
            submittedCode.ResultSummary = new ResultSummary();

            var compilerErrors = new RoslynCompiler().Compile(submittedCode.Code);

            if (compilerErrors == null)
            {
                submittedCode.ResultSummary.HasCompilerErrors = false;
                submittedCode.ResultSummary.CompilerErrors = null;
            }
            else
            {
                submittedCode.ResultSummary.HasCompilerErrors = true;
                submittedCode.ResultSummary.CompilerErrors = compilerErrors;
            }

            return Json(submittedCode);
        }
    }
}
