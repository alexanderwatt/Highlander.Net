using System.Web.Http;
using System.Web.Mvc;
using Highlander.WebAPI.V5r3.Areas.HelpPage.App_Start;

namespace Highlander.WebAPI.V5r3.Areas.HelpPage
{
    /// <summary>
    /// 
    /// </summary>
    public class HelpPageAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// 
        /// </summary>
        public override string AreaName => "HelpPage";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });
            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}