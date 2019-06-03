#region Usings

using System;
using FpML.V5r10.Reporting.Models.Rates.Swap;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.CapFloor
{
    public class CapFloorInstrumentAnalytic : SimpleIRSwapInstrumentAnalytic
    {
        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns></returns>
        public override Decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            return -result;
        }
    }
}