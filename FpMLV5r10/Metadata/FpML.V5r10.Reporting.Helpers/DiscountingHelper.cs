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

namespace FpML.V5r10.Reporting.Helpers
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