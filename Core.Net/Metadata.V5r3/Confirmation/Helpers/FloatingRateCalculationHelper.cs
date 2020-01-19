#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class FloatingRateCalculationHelper
    {
        public static FloatingRateCalculation CreateFloating(FloatingRateIndex floatingRateIndex,Period tenor)
        {
            var floatingRateCalculation = new FloatingRateCalculation
                {
                    floatingRateIndex = floatingRateIndex,
                    indexTenor = tenor
                };

            return floatingRateCalculation;
        }

    }
}
