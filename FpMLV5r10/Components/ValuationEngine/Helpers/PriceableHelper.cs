
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework.Instruments;

namespace Orion.ValuationEngine.Helpers
{
    public class PriceableHelper
    {
        //  aggregate type, e.g. sum, something else
        //
        public static AssetValuation GetValue(IList<InstrumentControllerBase> listIPriceable, IInstrumentControllerData modelData)
        {
            var list = listIPriceable.Select(pr => pr.Calculate(modelData)).ToList();
            AssetValuation sum = AssetValuationHelper.Sum(list);
            return sum;
        }
    }
}