#region Using directives

using System;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class FloatingRateCalculationHelper
    {
        public static FloatingRateCalculation CreateFloating(FloatingRateIndex floatingRateIndex,Period tenor)
        {
            FloatingRateCalculation floatingRateCalculation = new FloatingRateCalculation();

            floatingRateCalculation.floatingRateIndex = floatingRateIndex;
            floatingRateCalculation.indexTenor = tenor;

            return floatingRateCalculation;
        }

    }
}
