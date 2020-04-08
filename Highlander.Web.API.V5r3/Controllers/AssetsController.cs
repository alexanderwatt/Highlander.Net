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

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/assets")]
    public class AssetsController : ApiController
    {
        private readonly PricingCache _pricingCache;
        private readonly Reference<ILogger> _logger;
        public const string DetailsRouteName = "Details";

        public AssetsController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            _pricingCache = pricingCache;
            _logger = logger;
            logger.Target.LogInfo("Instantiating AssetsController...");
        }

        [HttpPost]
        [Route("properties")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreatePropertyAsset(string propertyId, string propertyType, string streetIdentifier, string streetName,
            string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
            string currency, string description)
        {
            var result = _pricingCache.CreateProperty(propertyId, propertyType, streetIdentifier, streetName, suburb, city,
                postalCode, state, country, numBedrooms, numBathrooms, numParking, currency, description, null);
            _logger.Target.LogInfo($"Created property id: {result}");
            return Created(Url.Link(DetailsRouteName, new { id = result }), result);
        }

        [HttpGet]
        [Route("property", Name = DetailsRouteName)]
        [ResponseType(typeof(Instrument))]
        public IHttpActionResult GetPropertyAsset(string id)
        {
            var instrument = _pricingCache.GetPropertyAsset(id);
            if (instrument == null || !(instrument.InstrumentNodeItem is PropertyNodeStruct propertyNodeStruct))
                return NotFound();
            return Ok(instrument);
        }
    }
}
