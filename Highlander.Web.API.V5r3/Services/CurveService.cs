using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
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

        public CurveService(PricingCache cache, Reference<ILogger> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public string UpdateCurveInputs(string curveId, QuotedAssetSet quotedAssetSet)
        {
            //string[] instruments, decimal[] adjustedRates, decimal[] additional
            //var quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, additional);
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _cache.NameSpace);
            properties.Set(LeaseProp.ReferencePropertyIdentifier, curveId);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.LeaseTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            _logger.Target.LogInfo("Updated curve :" + curveId);
            return _cache.CreateCurve(properties, quotedAssetSet);
        }
    }
}