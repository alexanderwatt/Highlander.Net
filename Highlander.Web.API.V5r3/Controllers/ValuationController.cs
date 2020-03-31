using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/valuation")]
    public class ValuationController : ApiController
    {
        private readonly PricingCache pricingCache;
        private readonly Reference<ILogger> logger;

        public ValuationController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            this.pricingCache = pricingCache;
            this.logger = logger;
            logger.Target.LogInfo("Instantiating ValuationController...");
        }

        [HttpGet]
        [Route("trade")]
        public IHttpActionResult GetTradeValuation(string id)
        {
            var trade = pricingCache.GetTrade(id);
            if (trade == null)
            {
                return NotFound();
            }
            return Ok(trade);
        }

        [HttpGet]
        [Route("trades")]
        public IHttpActionResult GetTrades()
        {
            //TODO
            return Ok();
        }

        [HttpPost]
        [Route("trades")]
        public IHttpActionResult Create(Trade trade)
        {
            //TODO
            //db.Add(person);
            //db.SaveChanges();
            //pricingCache.CreatePropertyTrade()
            return Ok();
        }

        [HttpGet]
        [Route("curve")]
        public IHttpActionResult GetCurve(string id)
        {
            var pricingStructure = pricingCache.GetPricingStructure(id);
            return Ok(pricingStructure);
        }
    }
}
