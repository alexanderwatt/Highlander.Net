/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
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
