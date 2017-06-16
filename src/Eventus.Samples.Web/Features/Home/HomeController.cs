using System;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Samples.Web.Features.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            throw new Exception("Bad");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            return View();
        }

        [Route("/Error/404")]
        public IActionResult Error404()
        {
            return View();
        }
    }
}
