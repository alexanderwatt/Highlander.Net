using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Models;
using Highlander.Web.API.V5r3.Services;
using Exception = System.Exception;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties/valuation")]
    public class ValuationController : ApiController
    {
        private readonly PropertyService _propertyService;
        private readonly LeaseService _leaseService;
        private readonly CurveService _curveService;
        private readonly Reference<ILogger> _logger;

        public ValuationController(
            PropertyService propertyService,
            LeaseService leaseService,
            CurveService curveService,
            Reference<ILogger> logger
            )
        {
            _propertyService = propertyService;
            _leaseService = leaseService;
            _curveService = curveService;
            _logger = logger;
        }

        [HttpPost]
        [Route("curve")]
        [ResponseType(typeof(string))]
        public IHttpActionResult UpdateCurveInputs(string nameSpace, [FromBody] CurveViewModel rateDefinitions)
        {
            var buildId = _curveService.UpdateDiscountCurveInputs(rateDefinitions);
            return Ok(buildId);
        }

        [HttpPost]
        [Route("dcf-value")]
        [ResponseType(typeof(decimal))]
        public IHttpActionResult GetValue(string nameSpace, [FromBody] PropertyFullViewModel model)
        {
            var transactionId = Guid.NewGuid().ToString();

            //create assets
            var propertyAsset = _propertyService.CreatePropertyAsset(model, transactionId);
            model.Trade.PropertyId = propertyAsset;
            //var propertyTrade = _propertyService.CreatePropertyTrade(model.Trade, transactionId);
            var leaseTradeIds = new Dictionary<string, LeaseTradeViewModel>();
            foreach (var lease in model.Leases)
            {
                lease.ReferencePropertyIdentifier = propertyAsset;
                var result = _leaseService.CreateLeaseTrade(lease, transactionId);
                if(result.Error != null)
                {
                    _propertyService.CleanTransaction(transactionId);
                    throw new Exception("Failed to create a lease - valuation aborted");
                }
                leaseTradeIds.Add(result.Id, lease);
            }

            //value
            var totalValue = 0m;
            foreach (var lease in leaseTradeIds)
            {
                var leaseValue = _leaseService.ValueLease(lease.Key, lease.Value.Owner, lease.Value.Currency, Constants.Constants.MarketName);

                if (leaseValue == null)
                {
                    throw new Exception($"Lease {lease.Key} could not be valued");
                }
                totalValue += leaseValue.Value;
            }

            //cleanup
            var deleted = _propertyService.CleanTransaction(transactionId);

            return Ok(totalValue);
        }
    }
}
