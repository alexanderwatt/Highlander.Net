using System;
using System.Collections.Generic;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Models;

namespace Highlander.Web.API.V5r3.Services
{
    public class CurveService
    {
        private readonly PricingCache _cache;
        private readonly Reference<ILogger> _logger;

        public CurveService(string nameSpace, Reference<ILogger> logger)
        {
            _cache = new PricingCache(nameSpace, false);
            _logger = logger;
        }

        public string UpdateDiscountCurveInputs(CurveViewModel rateDefinitions)
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _cache.NameSpace);
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.ClientApi.ToString());
            properties.Set(CurveProp.PricingStructureType, PricingStructureTypeEnum.DiscountCurve.ToString());
            properties.Set(CurveProp.CurveName, "AUD-LIBOR-SENIOR");
            properties.Set(CurveProp.Market, "QR_LIVE");
            properties.Set(CurveProp.Currency1, "AUD");
            properties.Set(CurveProp.Algorithm, "FastLinearZero");
            properties.Set(CurveProp.BaseDate, DateTime.Now);
            properties.Set(CurveProp.BootStrap, true);
            properties.Set(CurveProp.UniqueIdentifier, "Market.QR_LIVE.DiscountCurve.AUD-LIBOR-SENIOR");
            properties.Set(CurveProp.CreditSeniority, "SENIOR");
            properties.Set(CurveProp.CreditInstrumentId, "LIBOR");
            _logger.Target.LogInfo("Updated curve :");

            var rates = new List<(string, decimal, decimal?)>
            {
                ("AUD-Deposit-1D", rateDefinitions.OneDayRate, null),
                ("AUD-Deposit-3M", rateDefinitions.ThreeMonthRate, null),
                ("AUD-Deposit-6M", rateDefinitions.SixMonthRate, null),
                ("AUD-IRSwap-1Y", rateDefinitions.OneYearRate, null),
                ("AUD-IRSwap-3Y", rateDefinitions.ThreeYearRate, null),
                ("AUD-IRSwap-5Y", rateDefinitions.FiveYearRate, null),
                ("AUD-IRSwap-7Y", rateDefinitions.SevenYearRate, null),
                ("AUD-IRSwap-10Y", rateDefinitions.TenYearRate, null)
            };

            return _cache.CreateCurve(properties, rates);
        }
    }
}