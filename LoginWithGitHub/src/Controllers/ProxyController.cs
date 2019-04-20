using System;
using System.Linq;
using FP.OAuth.LoginWithGitHub.Business;
using Microsoft.AspNetCore.Mvc;

namespace FP.OAuth.LoginWithGitHub.Controllers
{
    public class ProxyController : Controller
    {
        private readonly IProxyRepository _repository;

        public ProxyController(IProxyRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var proxy = new Proxy {Items = _repository.GetItems().ToList()};
            return View(proxy);
        }

        public IActionResult Add(string url)
        {
            _repository.Add(url);
            var proxy = new Proxy {Items = _repository.GetItems().ToList()};
            return View("index", proxy);
        }
    }
}