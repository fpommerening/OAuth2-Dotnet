using FP.OAuth.ResourceServer.Business;
using Microsoft.AspNetCore.Mvc;

namespace FP.OAuth.ResourceServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IValueRepository _valueRepository;

        public HomeController(IValueRepository valueRepository)
        {
            _valueRepository = valueRepository;
        }

        public IActionResult Index()
        {
            var model = new Dashboard
            {
                ValueItems = _valueRepository.GetValue()
            };
            return View(model);
        }
    }
}