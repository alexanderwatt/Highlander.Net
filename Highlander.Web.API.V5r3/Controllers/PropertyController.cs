using Highlander.Reporting.V5r3;
using System.Web.Http;
using Highlander.Constants;
using System.Web.Http.Description;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Web.API.V5r3.Models;
using Highlander.Web.API.V5r3.Services;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties")]
    public class PropertyController : ApiController
    {
        private readonly PropertyService propertyService;

        public PropertyController(PropertyService propertyService)
        {
            this.propertyService = propertyService;
        }

        [HttpGet]
        [Route("trades")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetPropertyTradeIds()
        {
            var trades = propertyService.GetPropertyTradeIds();
            return Ok(trades);
        }

        [HttpPost]
        [Route("trades")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreatePropertyTrade([FromBody] PropertyTradeViewModel model, string transactionId = null)
        {
            var result = propertyService.CreatePropertyTrade(model, transactionId);
            return Ok(result);
        }

        [HttpGet]
        [Route("assets")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetPropertyAssetIds()
        {
            var trades = propertyService.GetPropertyAssetIds();
            return Ok(trades);
        }

        [HttpPost]
        [Route("assets")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreatePropertyAsset([FromBody] PropertyAssetViewModel model, string transactionId = null)
        {
            var result = propertyService.CreatePropertyAsset(model, transactionId);
            return Ok(result);
        }

        [HttpGet]
        [Route("asset")]
        [ResponseType(typeof(PropertyNodeStruct))]
        public IHttpActionResult GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            var result = propertyService.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Route("curve")]
        [ResponseType(typeof(PricingStructureData))]
        public IHttpActionResult GetCurve(string id)
        {
            var result = propertyService.GetValue(id);
            return Ok(result);
        }
    }
}
