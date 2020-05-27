using System;
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;

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

        public string UpdateDiscountCurveInputs(List<(string, decimal, decimal?)> values)
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
            return _cache.CreateCurve(properties, values);
        }
    }
}