namespace FpML.V5r3.Reporting.Helpers
{
    public static class DiscountingHelper
    {
        public static Discounting Create(decimal discountingRate, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingTypeEnum)
        {
            var discounting = new Discounting
                                          {
                                              discountingType = discountingTypeEnum,
                                              discountRate = discountingRate,
                                              discountRateDayCountFraction = dayCountFraction
                                              //discountRateSpecified = true
                                          };
            return discounting;
        }

        public static Discounting Create(decimal? discountingRate, DayCountFraction dayCountFraction, DiscountingTypeEnum discountingTypeEnum)
        {
            var discounting = new Discounting
            {
                discountingType = discountingTypeEnum,
                discountRateDayCountFraction = dayCountFraction
                //discountRateSpecified = true
            };
            if(discountingRate!=null)
            {
                discounting.discountRate = (decimal) discountingRate;
            }
            return discounting;
        }

        public static Discounting Parse(decimal discountingRate, string dayCountFraction, DiscountingTypeEnum discountingTypeEnum)
        {
            var discounting = new Discounting
                                          {
                                              discountingType = discountingTypeEnum,
                                              discountRate = discountingRate,
                                              discountRateDayCountFraction =
                                                  DayCountFractionHelper.Parse(dayCountFraction)
                                              //discountRateSpecified = true
                                          };
            return discounting;
        }
    }
}