using Microsoft.AspNetCore.Mvc;

namespace HighlanderCurveViewer.Controllers
{
    public class CurvesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Welcome(string name, int numTimes = 1)
        {
            ViewData["Message"] = "Hello " + name;
            ViewData["NumTimes"] = numTimes;
            return View();
        }
    }
}