using System;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Utilities.NamedValues;

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
        [Route("trade")]
        public IHttpActionResult GetTrade(string id)
        {
            var trade = _pricingCache.GetTrade(id);
            if (trade == null)
            {
                return NotFound();
            }
            return Ok(trade);
        }

        [HttpGet]
        [Route("trades")]
        public IHttpActionResult GetPropertyTradeIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _pricingCache.NameSpace);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _pricingCache.QueryTradeIds(properties);
            if (trades == null)
            {
                return NotFound();
            }
            _logger.Target.LogInfo("Queried property trade ids.");
            return Ok(trades);
        }

        [HttpGet]
        [Route("trades")]
        public IHttpActionResult GetLeaseTradeIds(string propertyId)
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _pricingCache.NameSpace);
            properties.Set(LeaseProp.ReferencePropertyIdentifier, propertyId);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.LeaseTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _pricingCache.QueryTradeIds(properties);
            if (trades == null)
            {
                return NotFound();
            }
            _logger.Target.LogInfo("Queried lease trade ids.");
            return Ok(trades);
        }

        [HttpPost]
        [Route("trades")]
        public IHttpActionResult CreatePropertyTrade(string tradeId, bool isParty1Buyer, string party1, string party2, 
            DateTime tradeDate, DateTime effectiveDate, decimal purchaseAmount, DateTime paymentDate, string propertyType, 
            string currency, string propertyIdentifier, string tradingBook)
        {
            var result = _pricingCache.CreatePropertyTrade(tradeId, isParty1Buyer, party1, party2, tradeDate, effectiveDate,
                purchaseAmount,
                paymentDate, propertyType, currency, propertyIdentifier, tradingBook);
            _logger.Target.LogInfo("Created property trade id: {}", result);
            return Ok(result);
        }

        [HttpPost]
        [Route("trades")]
        public IHttpActionResult CreateLeaseTrade(string tradeId, bool isParty1Tenant, string party1, string party2,
            DateTime tradeDate, DateTime leaseStartDate, string currency, string portfolio, decimal startGrossAmount, string leaseId,
            DateTime leaseExpiryDate, string referencePropertyIdentifier, string description)
        {
            var result = _pricingCache.CreateLeaseTrade(tradeId, isParty1Tenant, party1, party2, tradeDate, leaseStartDate,
                currency, portfolio, startGrossAmount, leaseId, leaseExpiryDate, referencePropertyIdentifier, description);
            _logger.Target.LogInfo("Created lease trade id: {}", result);
            return Ok(result);
        }

        [HttpGet]
        [Route("curve")]
        public IHttpActionResult GetCurve(string id)
        {
            var pricingStructure = _pricingCache.GetPricingStructure(id);
            return Ok(pricingStructure);
        }
    }
}
