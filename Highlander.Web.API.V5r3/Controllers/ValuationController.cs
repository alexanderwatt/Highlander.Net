using System;
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
        private readonly CurveService curveService;

        public ValuationController(PropertyService propertyService, LeaseService leaseService)
        {
            this.propertyService = propertyService;
            this.leaseService = leaseService;
        }

        [HttpPost]
        [Route("curve")]
        [ResponseType(typeof(string))]
        public IHttpActionResult UpdateCurveInputs([FromBody] CurveUpdateInputsViewModel model, string curveId = null)
        {
            var buildId =  curveService.UpdateCurveInputs(model, curveId);
            return Ok(buildId);
        }

        [HttpPost]
        [Route("dcf-value")]
        [ResponseType(typeof(decimal))]
        public IHttpActionResult GetValue([FromBody] PropertyFullViewModel model)
        {
            var transactionId = Guid.NewGuid().ToString();

            //create assets
            var propertyAsset = propertyService.CreatePropertyAsset(model, transactionId);
            model.Trade.PropertyId = propertyAsset;
            var propertyTrade = propertyService.CreatePropertyTrade(model.Trade, transactionId);
            foreach (var lease in model.Leases)
            {
                lease.ReferencePropertyIdentifier = propertyAsset;
                var result = leaseService.CreateLeaseTrade(lease, transactionId);
                if(result.Error != null)
                {
                    propertyService.CleanTransaction(transactionId);
                    throw new Exception("Failed to create a lease - valuation aborted");
                }
            }

            //value
            var value = 0m;
            //TODO

            //cleanup
            var deleted = propertyService.CleanTransaction(transactionId);

            return Ok(value);
        }
    }
}
