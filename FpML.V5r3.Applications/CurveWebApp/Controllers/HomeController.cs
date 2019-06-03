using System.Diagnostics;
using Highlander.CurveWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Highlander.CurveWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Highlander description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contact details.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
