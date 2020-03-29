using System.Web.Mvc;

namespace Highlander.WebAPI.V5r3.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Highlander Home Page";
            return View();
        }
    }
}
