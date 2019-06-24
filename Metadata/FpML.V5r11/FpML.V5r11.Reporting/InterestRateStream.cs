/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r11.Reporting
{
    public partial class InterestRateStream
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (calculationPeriodAmount.Item is Calculation amount)
            {
                var currency = XsdClassesFieldResolver.CalculationGetNotionalSchedule(amount);
                if (currency?.notionalStepSchedule != null)
                {
                    var discountCurve = CurveNameHelpers.GetDiscountCurveName(currency.notionalStepSchedule.currency, true);
                    result.Add(discountCurve);
                }
                if (amount.Items[0] is FloatingRateCalculation floatingRateCalculation)
                {
                    result.Add(CurveNameHelpers.GetForecastCurveName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            //TODO
            if (stubCalculationPeriodAmount != null)
            {
                if (stubCalculationPeriodAmount.initialStub?.Items != null)
                {
                    result.AddRange(from value in stubCalculationPeriodAmount.initialStub.Items
                                        where value is Money
                                    select CurveNameHelpers.GetDiscountCurveName(((Money)value).currency, true));
                }
                if (stubCalculationPeriodAmount.finalStub?.Items != null)
                {
                    result.AddRange(from value in stubCalculationPeriodAmount.finalStub.Items
                                        where value is Money
                                    select CurveNameHelpers.GetDiscountCurveName(((Money)value).currency, true));
                }
            }
            return result;
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<string> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String>();
            if (calculationPeriodAmount.Item is Calculation amount)
            {
                if (amount.Items[0] is FloatingRateCalculation floatingRateCalculation)
                {
                    result.Add(CurveNameHelpers.GetRateVolatilityMatrixName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetRequiredCurrencies()
        {
            var result = new List<string>();
            var item = XsdClassesFieldResolver.CalculationGetNotionalSchedule((Calculation)calculationPeriodAmount.Item);
            if (item?.notionalStepSchedule?.currency != null)
            {
                result.Add(item.notionalStepSchedule.currency.Value);
            }
            return result;
        }
    }
}
