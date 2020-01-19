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
    public static class CalculationFactory
    {
        public static Calculation CreateFixed(decimal fixedRate, Money notional,
            DayCountFraction dayCountFraction, DiscountingTypeEnum? discountingType)
        {
            var discounting = discountingType != null
                                  ? DiscountingHelper.Create(fixedRate, dayCountFraction, (DiscountingTypeEnum)discountingType)
                                  : null;
            var calculation = new Calculation
            {
                Items = new object[] {NotionalFactory.Create(notional)},
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting = discounting,
                Items1 = new object[] { FixedRateScheduleHelper.Create(fixedRate) }
            };
            return calculation;
        }

        public static Calculation CreateFixed(decimal fixedRate, NonNegativeAmountSchedule notionalSchedule,
            DayCountFraction dayCountFraction, DiscountingTypeEnum? discountingType)
        {
            var discounting = discountingType != null
                                  ? DiscountingHelper.Create(fixedRate, dayCountFraction, (DiscountingTypeEnum)discountingType)
                                  : null;
            var calculation = new Calculation
            {
                Items = new object[] {NotionalFactory.Create(notionalSchedule)},
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting = discounting,
                Items1 = new object[] { FixedRateScheduleHelper.Create(fixedRate) }
            };
            return calculation;
        }

        public static Calculation CreateFixed(decimal fixedRate, decimal discountRate, Money notional,
            CompoundingMethodEnum compoundingMethod, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingType)
        {
            var calculation = new Calculation
            {
                Items = new object[] { NotionalFactory.Create(notional) },
                compoundingMethod = compoundingMethod,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting =
                                                  DiscountingHelper.Create(discountRate, dayCountFraction,
                                                                           discountingType),
                Items1 = new object[] { FixedRateScheduleHelper.Create(fixedRate) }
            };
            return calculation;
        }

        public static Calculation CreateFloating(decimal fixedRate, Money notional, FloatingRateIndex floatingRateIndex, Period tenor,
            DayCountFraction dayCountFraction, DiscountingTypeEnum? discountingType)
        {
            var discounting = discountingType != null
                ? DiscountingHelper.Create(fixedRate, dayCountFraction, (DiscountingTypeEnum)discountingType)
                : null;
            var calculation = new Calculation
            {
                Items = new object[] { NotionalFactory.Create(notional) },
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting = discounting,
                Items1 = new object[] { FloatingRateCalculationHelper.CreateFloating(floatingRateIndex, tenor) }
            };
            return calculation;
        }

        public static Calculation CreateFloating(Money notional, FloatingRateIndex floatingRateIndex, Period tenor, DayCountFraction dayCountFraction, DiscountingTypeEnum? discountingType)
        {
            var discounting = discountingType != null
                                  ? DiscountingHelper.Create(null, dayCountFraction, (DiscountingTypeEnum)discountingType)
                                  : null;
            var calculation = new Calculation
            {
                Items = new object[] { NotionalFactory.Create(notional) },
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting = discounting,
                Items1 = new object[] { FloatingRateCalculationHelper.CreateFloating(floatingRateIndex, tenor) }
            };
            return calculation;
        }

        public static Calculation CreateFloating(Money notional, FloatingRateIndex floatingRateIndex, Period tenor, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingType)
        {
            var calculation = new Calculation
            {
                Items = new object[] { NotionalFactory.Create(notional) },
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction
            };
            var discounting = new Discounting
            {
                discountingType = discountingType,
                discountRateDayCountFraction = dayCountFraction
            };
            calculation.discounting = discounting;
            calculation.Items = new object[] { FloatingRateCalculationHelper.CreateFloating(floatingRateIndex, tenor) };
            return calculation;
        }
    }
}
