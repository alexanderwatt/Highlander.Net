#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Contracts;
using Orion.Constants;
using Orion.Workflow;

#endregion

namespace Orion.PortfolioValuation
{
    public class WFCalculateValuationBase : WorkstepBase<RequestBase, HandlerResponse>
    {
        protected static List<string> GetSwapMetrics()
        {
            var metrics = Enum.GetNames(typeof(InstrumentMetrics));
            var result = new List<string>(metrics) { "BreakEvenRate" };
            return result;
        }
    }
}
