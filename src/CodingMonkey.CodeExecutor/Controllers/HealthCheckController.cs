namespace CodingMonkey.CodeExecutor.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        // GET: /<controller>/
        public JsonResult Index()
        {
            return Json(new
            {
                OK = true
            });
        }
    }
}
