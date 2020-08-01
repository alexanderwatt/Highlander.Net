﻿using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Models;
using System;
using System.Collections.Generic;

namespace Highlander.Web.API.V5r3.Services
{
    public class LeaseService
    {
        private readonly PricingCache _cache;
        private readonly Reference<ILogger> _logger;

        public LeaseService(PricingCache cache, Reference<ILogger> logger)
        {
            this._cache = cache;
            this._logger = logger;
        }

        public IEnumerable<string> GetLeaseTradeIds(string propertyId)
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _cache.NameSpace);
            properties.Set(LeaseProp.ReferencePropertyIdentifier, propertyId);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.LeaseTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _cache.QueryTradeIds(properties);
            _logger.Target.LogInfo("Queried lease trade ids.");
            return trades;
        }

        public IEnumerable<(Trade Trade, NamedValueSet Props)> GetLeaseTradesWithProps(string transactionId)
        {
            var props = new NamedValueSet();
            props.Set(Constants.Constants.TransactionIdProperty, transactionId);
            var trades = _cache.GetTrades(props);
            return trades;
        }

        public decimal? GetValuationReport(string reportId)
        {
            var report = _cache.LoadItem<ValuationReport>(_cache.NameSpace, reportId).Data as ValuationReport;
            var quotation = report?.GetFirstQuotationForMetricName(LeaseTransactionMetrics.NPV.ToString());
            return quotation != null && quotation.valueSpecified ? quotation.value : (decimal?)null;
        }
        
        public (string Id, string Error) CreateLeaseTrade(LeaseTradeViewModel model, string transactionId)
        {
            var properties = new NamedValueSet();
            properties.Set(Constants.Constants.TransactionIdProperty, transactionId);
            properties.Set(LeaseProp.RollConvention, model.RollConvention ?? RollConventionEnum.EOM.ToString());
            properties.Set(LeaseProp.BusinessDayCalendar, model.BusinessCalendar ?? "AUSY");
            properties.Set(LeaseProp.BusinessDayAdjustments, model.BusinessDayAdjustments ?? "FOLLOWING");
            properties.Set(LeaseProp.UpfrontAmount, model.UpfrontAmount ?? 0m);
            properties.Set(LeaseProp.LeaseType, model.LeaseType ?? "Standard");
            properties.Set(LeaseProp.Area, model.LeaseArea ?? 0m);
            properties.Set(LeaseProp.UnitsOfArea, model.LeaseAreaUnits ?? "sqm");
            properties.Set(LeaseProp.ReviewFrequency, model.ReviewFrequency ?? "1Y");
            if (model.ReviewFrequency != null && model.NextReviewDate == null)
            {
                return (null, "When review frequency is populated, next review date must be populated");
            }
            properties.Set(LeaseProp.NextReviewDate, model.NextReviewDate ?? model.LeaseStartDate.AddYears(1));
            properties.Set(LeaseProp.ReviewChange, model.ReviewAmountUpDown ?? 0m);
            properties.Set(LeaseProp.PaymentFrequency, model.PaymentFrequency ?? "1M");
            var result = _cache.CreateLeaseTradeWithProperties(Guid.NewGuid().ToString(), true, model.Tenant, model.Owner, model.TradeDate, model.LeaseStartDate,
                model.Currency, model.Portfolio, model.StartGrossAmount, model.LeaseId ?? model.ReferencePropertyIdentifier, model.LeaseExpiryDate, model.ReferencePropertyIdentifier, model.Description, properties);
            _logger.Target.LogInfo($"Created lease trade id: {result}");
            return (result, null);
        }

        public decimal? ValueLease(string leaseId, string owner, string currency, string market)
        {
            var leaseValueId = _cache.ValueTradeFromMarket(leaseId, owner, new List<string> { "NPV" }, currency, market, DateTime.UtcNow);
            return GetValuationReport(leaseValueId);
        }
    }
}