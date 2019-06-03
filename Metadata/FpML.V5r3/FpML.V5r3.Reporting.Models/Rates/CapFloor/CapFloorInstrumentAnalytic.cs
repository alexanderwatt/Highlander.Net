#region Usings

using System;
using Orion.ModelFramework;
using Orion.Models.Rates.Swap;

#endregion

namespace Orion.Models.Rates.CapFloor
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