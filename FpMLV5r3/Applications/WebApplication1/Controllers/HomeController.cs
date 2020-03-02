using System.Web.Mvc;

namespace Highlander.WebAPI.V5r3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }
    }
}
