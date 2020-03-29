using System.Web.Mvc;

namespace Highlander.WebAPI.V5r3.Controllers
{
    /// <summary>
    /// Creates and views trades.
    /// </summary>
    public class TradeController : Controller
    {
        // GET
        /// <summary>
        /// Gets the main view
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}