using System.Web.Http;
using System.Web.Http.Description;
using Highlander.Web.API.V5r3.Models;
using Highlander.Web.API.V5r3.Services;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties/valuation")]
    public class ValuationController : ApiController
    {
        private readonly PropertyService propertyService;
        private readonly LeaseService leaseService;

        public ValuationController(PropertyService propertyService, LeaseService leaseService)
        {
            this.propertyService = propertyService;
            this.leaseService = leaseService;
        }

        [HttpPost]
        [Route("dcf-value")]
        [ResponseType(typeof(decimal))]
        public IHttpActionResult GetValue([FromBody] PropertyFullViewModel model)
        {
            //create assets
            var propertyAsset = propertyService.CreatePropertyAsset(model);
            var propertyTrade = propertyService.CreatePropertyTrade(model.Trade);
            foreach (var lease in model.Leases)
            {
                var result = leaseService.CreateLeaseTrade(lease);
            }

            //value
            //TODO

            //cleanup
            propertyService.ClearCache();

            return Ok(0m);
        }
    }
}
