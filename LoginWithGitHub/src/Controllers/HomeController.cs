using Microsoft.AspNetCore.Mvc;

namespace FP.OAuth.LoginWithGitHub.Controllers
{
    public class HomeController : Controller
    {
 

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}