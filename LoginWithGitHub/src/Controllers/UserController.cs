using Microsoft.AspNetCore.Mvc;

namespace FP.OAuth.LoginWithGitHub.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index(Business.GitHubUser user)
        {
            return View(user);
        }
    }
}