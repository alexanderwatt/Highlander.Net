#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
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

    public static class FloatingLegCalculationHelper
    {
        public static FloatingLegCalculation Create(FloatingRateIndex floatingRateIndex, Period tenor, AveragingMethodEnum averagingMethod,
            decimal conversionFactor, Rounding rounding, CommodityPricingDates pricingDates, CommodityFx fx, object[] commoditySpreadArray)
        {
            var floatingRateCalculation = new FloatingLegCalculation
                {
                    averagingMethod = averagingMethod,
                    averagingMethodSpecified = true,
                    conversionFactor = conversionFactor,
                    conversionFactorSpecified = true,
                    rounding = rounding,
                    pricingDates = pricingDates,
                    fx = fx,
                    Items = commoditySpreadArray
                };
            return floatingRateCalculation;
        }
    }
}
