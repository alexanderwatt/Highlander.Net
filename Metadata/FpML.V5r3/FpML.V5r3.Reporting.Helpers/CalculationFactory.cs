#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
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
                                              Item = NotionalFactory.Create(notional),
                                              compoundingMethod = CompoundingMethodEnum.None,
                                              compoundingMethodSpecified = true,
                                              dayCountFraction = dayCountFraction,
                                              discounting = discounting,
                                              Items = new object[] { FixedRateScheduleHelper.Create(fixedRate)}
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
                Item = NotionalFactory.Create(notionalSchedule),
                compoundingMethod = CompoundingMethodEnum.None,
                compoundingMethodSpecified = true,
                dayCountFraction = dayCountFraction,
                discounting = discounting,
                Items = new object[] { FixedRateScheduleHelper.Create(fixedRate) }
            };

            return calculation;
        }

        public static Calculation CreateFixed(decimal fixedRate, decimal discountRate, Money notional, 
            CompoundingMethodEnum compoundingMethod, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingType)
        {
            var calculation = new Calculation
                                          {
                                              Item = NotionalFactory.Create(notional),
                                              compoundingMethod = compoundingMethod,
                                              compoundingMethodSpecified = true,
                                              dayCountFraction = dayCountFraction,
                                              discounting =
                                                  DiscountingHelper.Create(discountRate, dayCountFraction,
                                                                           discountingType),
                                              Items = new object[] { FixedRateScheduleHelper.Create(fixedRate) }
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
                                      Item = NotionalFactory.Create(notional),
                                      compoundingMethod = CompoundingMethodEnum.None,
                                      compoundingMethodSpecified = true,
                                      dayCountFraction = dayCountFraction,
                                      discounting = discounting,
                                      Items = new object[] { FloatingRateCalculationHelper.CreateFloating(floatingRateIndex, tenor) }
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
                                      Item = NotionalFactory.Create(notional),
                                      compoundingMethod = CompoundingMethodEnum.None,
                                      compoundingMethodSpecified = true,
                                      dayCountFraction = dayCountFraction,
                                      discounting = discounting,
                                      Items = new object[] { FloatingRateCalculationHelper.CreateFloating(floatingRateIndex, tenor) }
                                  };

            return calculation;
        }

        public static Calculation CreateFloating(Money notional, FloatingRateIndex floatingRateIndex, Period tenor, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingType)
        {
            var calculation = new Calculation
                                  {
                                      Item = NotionalFactory.Create(notional),
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
