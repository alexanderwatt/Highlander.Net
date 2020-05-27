﻿using System.Web.Http;
using System.Web.Http.Description;
using Highlander.Web.API.V5r3.Models;
using Highlander.Web.API.V5r3.Services;

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties/leases")]
    public class LeaseController : ApiController
    {
        private readonly LeaseService _leaseService;

        public LeaseController(LeaseService leaseService)
        {
            _leaseService = leaseService;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetLeaseTradeIds(string propertyId)
        {
            var trades = _leaseService.GetLeaseTradeIds(propertyId);
            return Ok(trades);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreateLeaseTrade([FromBody] LeaseTradeViewModel model, string transactionId = null)
        {
            var result = _leaseService.CreateLeaseTrade(model, transactionId);
            return Ok(result);
        }
    }
}
