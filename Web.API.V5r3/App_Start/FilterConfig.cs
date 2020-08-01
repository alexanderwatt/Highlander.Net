using System.Web;
using System.Web.Mvc;

namespace Highlander.Web.API.V5r3
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
