namespace CodingMonkey.CodeExecutor.Controllers
{
    using System.Threading.Tasks;

    using CodingMonkey.CodeExecutor.CodeExecution;
    using CodingMonkey.CodeExecutor.Models;
    
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [Authorize]
    public class CompileController : Controller
    {

        [HttpPost]
        public async Task<JsonResult> Post([FromBody] SubmittedCode submittedCode)
        {
            submittedCode.ResultSummary = new ResultSummary();

            var compilerErrors = new RoslynCompiler().Compile(submittedCode.Code);

            if (compilerErrors.Count == 0)
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
