using System;
using Highlander.Core.Interface.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;
using Highlander.Reporting.ModelFramework.V5r3;
using System.Web.Http.Description;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/valuation")]
    public class ValuationController : ApiController
    {
        private readonly PricingCache _pricingCache;
        private readonly Reference<ILogger> _logger;

        public ValuationController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            _pricingCache = pricingCache;
            _logger = logger;
            logger.Target.LogInfo("Instantiating ValuationController...");
        }

        [HttpGet]
        [Route("curve")]
        [ResponseType(typeof(PricingStructureData))]
        public IHttpActionResult GetCurve(string id)
        {
            var pricingStructure = _pricingCache.GetPricingStructure(id);
            return Ok(pricingStructure);
        }
    }
}
