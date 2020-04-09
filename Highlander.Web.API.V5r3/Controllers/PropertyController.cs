using System;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Utilities.NamedValues;
using System.Web.Http.Description;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Core.Common;
using Highlander.Web.API.V5r3.Models;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties")]
    public class PropertyController : ApiController
    {
        private readonly PricingCache _pricingCache;
        private readonly Reference<ILogger> _logger;

        public PropertyController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            _pricingCache = pricingCache;
            _logger = logger;
            logger.Target.LogInfo("Instantiating PropertyController...");
        }

        //[HttpGet]
        //[Route("trade")]
        //public IHttpActionResult GetTrade(string id)
        //{
        //    var trade = _pricingCache.GetTrade(id);
        //    if (trade == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(trade);
        //}

        [HttpGet]
        [Route("trades")]
        [ResponseType(typeof(IEnumerable<string>))]
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

        [HttpPost]
        [Route("trades")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreatePropertyTrade([FromBody] PropertyTradeViewModel model)
        {
            var properties = new NamedValueSet();
            var result = _pricingCache.CreatePropertyTradeWithProperties(model.TradeId, true, model.Purchaser, model.Seller, model.TradeTimeUtc, model.EffectiveTimeUtc,
                model.PurchaseAmount, model.PaymentTimeUtc, model.PropertyType, model.Currency, model.PropertyId, model.TradingBook, properties);
            _logger.Target.LogInfo("Created property trade id: {0}", result);
            return Ok(result);
        }

        [HttpGet]
        [Route("assets")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetPropertyAssetIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _pricingCache.QueryPropertyAssetIds(properties);
            if (trades == null)
            {
                return NotFound();
            }
            _logger.Target.LogInfo("Queried property assets.");
            return Ok(trades);
        }

        //[HttpPost]
        //[Route("assets")]
        //[ResponseType(typeof(string))]
        //public IHttpActionResult CreatePropertyAsset(long companyId, string propertyId, PropertyType propertyType, string shortName, string streetIdentifier, string streetName,
        //    string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
        //    string currency, string description)
        //{
        //    var result = _pricingCache.CreatePropertyAsset(propertyId, propertyType, shortName, streetIdentifier, streetName, suburb, city,
        //        postalCode, state, country, numBedrooms, numBathrooms, numParking, currency, description, null);
        //    _logger.Target.LogInfo("Created property trade id: {0}", result);
        //    return Ok(result);
        //}

        [HttpPost]
        [Route("assets")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreatePropertyAsset([FromBody] PropertyAssetViewModel model)
        {
            var result = _pricingCache.CreatePropertyAsset(model.PropertyId, model.PropertyType, model.ShortName, model.StreetIdentifier, model.StreetName, model.Suburb, model.City,
                model.PostalCode, model.State, model.Country, model.NumBedrooms.ToString(), model.NumBathrooms.ToString(), model.NumParking.ToString(), model.Currency, model.Description, null);
            _logger.Target.LogInfo("Created property id: {0}", result);
            return Ok(result);
        }

        [HttpGet]
        [Route("asset")]
        [ResponseType(typeof(PropertyNodeStruct))]
        public IHttpActionResult GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            var instrument = _pricingCache.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
            if (instrument == null)
                return NotFound();
            return Ok(instrument.Data);
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
