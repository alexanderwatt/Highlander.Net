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

namespace Highlander.Web.API.V5r3.Controllers
{
    [RoutePrefix("api/properties/leases")]
    public class LeaseController : ApiController
    {
        private readonly PricingCache _pricingCache;
        private readonly Reference<ILogger> _logger;

        public LeaseController(PricingCache pricingCache, Reference<ILogger> logger)
        {
            _pricingCache = pricingCache;
            _logger = logger;
            logger.Target.LogInfo("Instantiating LeaseController...");
        }

        //[HttpGet]
        //[Route("trade")]
        //[ResponseType(typeof(Trade))]
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
        [Route("")]
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
        [Route("")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CreateLeaseTrade(string tradeId, bool isParty1Tenant, string party1, string party2,
    DateTime tradeDate, DateTime leaseStartDate, string currency, string portfolio, decimal startGrossAmount, string leaseId,
    DateTime leaseExpiryDate, string referencePropertyIdentifier, string description)
        {
            var result = _pricingCache.CreateLeaseTrade(tradeId, isParty1Tenant, party1, party2, tradeDate, leaseStartDate,
                currency, portfolio, startGrossAmount, leaseId, leaseExpiryDate, referencePropertyIdentifier, description);
            _logger.Target.LogInfo("Created lease trade id: {0}", result);
            return Ok(result);
        }

        ////Create the transaction
        //var properties = new NamedValueSet();
        //properties.Set(LeaseProp.RollConvention, rollConventionTextBox.Text);
        //properties.Set(LeaseProp.BusinessDayCalendar, businessCalendarListBox.Text);
        //properties.Set(LeaseProp.BusinessDayAdjustments, businessDayAdjustmentsListBox.Text);
        //properties.Set(LeaseProp.UpfrontAmount, 0.0m);
        //properties.Set(LeaseProp.LeaseType, leaseTypeListBox.Text);
        ////properties.Set(LeaseProp.Area, 0.0m);
        ////properties.Set(LeaseProp.UnitsOfArea, "sqm");
        //properties.Set(LeaseProp.ReviewFrequency, ReviewFrequencyListBox.Text);
        //properties.Set(LeaseProp.NextReviewDate, startDateTimePicker.Value.AddYears(1));
        //properties.Set(LeaseProp.ReviewChange, reviewAmountUpDown.Value);
        //leaseTradeIdentifierTextBox.Text = _pricingCache.CreateLeaseTradeWithProperties(leaseIdentifierTxtBox.Text, isParty1TenantCheckBox.Checked, Party1TextBox.Text,
        //Party2TextBox.Text, tradeDateTimePicker.Value, startDateTimePicker.Value, currencyListBox.Text, portfolioTextBox.Text, Convert.ToDecimal(purchaseAmountTextBox.Text),
        //leaseIdentifierTxtBox.Text, expiryDateTimePicker.Value, propertyIdentifierTextBox.Text, descriptionTextBox.Text, _properties);
    }
}
