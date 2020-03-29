using Highlander.Codes.V5r3;
using Highlander.Core.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/valuation")]
    public class ValuationController : ApiController
    {
        private readonly ICoreClient client;
        private readonly ICoreCache cache;

        public ValuationController(ICoreClient client, ICoreCache cache)
        {
            this.client = client;
            this.cache = cache;
        }

        [HttpGet]
        [Route("trade")]
        public IHttpActionResult GetTradeValuation(string tradeId)
        {
            var trade = cache.LoadItem<Trade>(tradeId);
            if(trade == null)
            {
                return NotFound();
            }
            return Ok(trade);
        }

        [HttpGet]
        [Route("trades")]
        public IHttpActionResult GetTrades()
        {
            var trades = cache.LoadItems<Trade>(Expr.IsEQU("productType", "swap"));
            return Ok(trades);
        }
    }
}
