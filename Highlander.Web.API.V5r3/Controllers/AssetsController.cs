using Highlander.Core.Interface.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Web.Http;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Utilities.NamedValues;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/assets")]
    public class AssetsController : ApiController
    {
        private readonly PricingCache _pricingCache;
        private readonly Reference<ILogger> _logger;

        public AssetsController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            _pricingCache = pricingCache;
            _logger = logger;
            logger.Target.LogInfo("Instantiating AssetsController...");
        }


        [HttpGet]
        [Route("trades")]
        public IHttpActionResult GetPropertyAssetIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _pricingCache.NameSpace);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _pricingCache.QueryPropertyAssetIds(properties);
            if (trades == null)
            {
                return NotFound();
            }
            _logger.Target.LogInfo("Queried property trade ids.");
            return Ok(trades);
        }

        [HttpPost]
        [Route("properties")]
        public IHttpActionResult CreatePropertyAsset(string propertyId, PropertyType propertyType, string shortName, string streetIdentifier, string streetName,
            string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
            string currency, string description)
        {
            var result = _pricingCache.CreatePropertyAsset(propertyId, propertyType, shortName, streetIdentifier, streetName, suburb, city,
                postalCode, state, country, numBedrooms, numBathrooms, numParking, currency, description, null);
            _logger.Target.LogInfo($"Created property id: {result}");
            return Ok(result);
        }

        [HttpGet]
        [Route("property")]
        public IHttpActionResult GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            var instrument = _pricingCache.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
            if (instrument == null)
                return NotFound();
            return Ok(instrument);
        }
    }
}
