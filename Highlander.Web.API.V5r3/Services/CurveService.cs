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

        public string UpdateCurveInputs(string curveId, QuotedAssetSet quotedAssetSet)
        {
            //string[] instruments, decimal[] adjustedRates, decimal[] additional
            //var quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, additional);
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _cache.NameSpace);
            properties.Set(CurveProp.Market, "test");
            _logger.Target.LogInfo("Updated curve :" + curveId);
            return _cache.CreateCurve(properties, quotedAssetSet);
        }
    }
}