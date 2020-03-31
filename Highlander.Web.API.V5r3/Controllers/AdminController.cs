using Highlander.Configuration.Data.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private readonly Reference<ILogger> logger;
        private readonly ICoreCache cache;

        public AdminController(ICoreCache cache, Reference<ILogger> logger)
        {
            this.logger = logger;
            this.cache = cache;
            logger.Target.LogInfo("Instantiating AdminController...");
        }

        [HttpPut]
        [Route("load")]
        public IHttpActionResult LoadData()
        {
            try
            {
                logger.Target.LogInfo("Loading data into cache...");
                LoadConfigDataHelper.LoadConfigurationData(logger.Target, cache.Proxy, EnvironmentProp.DefaultNameSpace);
                logger.Target.LogInfo("Loaded data into cache successfully");
            }
            catch (System.Exception exception)
            {
                logger.Target.Log(exception);
                logger.Target.LogInfo("FAILED");
                throw exception;
            }
            return Ok();
        }
    }
}
