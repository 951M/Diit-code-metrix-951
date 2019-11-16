using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}