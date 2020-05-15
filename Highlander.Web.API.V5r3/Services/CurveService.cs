using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Models;
using System.Collections.Generic;

namespace Highlander.Web.API.V5r3.Services
{
    public class CurveService
    {
        private readonly PricingCache cache;
        private readonly Reference<ILogger> logger;

        public CurveService(PricingCache cache, Reference<ILogger> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public string UpdateCurveInputs(CurveUpdateInputsViewModel model, string curveId)
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, cache.NameSpace);
            properties.Set(LeaseProp.ReferencePropertyIdentifier, curveId);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.LeaseTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            logger.Target.LogInfo("Updated curve :" + curveId);
            return cache.CreateCurve(properties, null);
        }
    }
}