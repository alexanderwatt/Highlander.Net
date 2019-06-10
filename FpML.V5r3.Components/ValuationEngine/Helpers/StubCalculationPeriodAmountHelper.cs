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

#region Using directives

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    class StubCalculationPeriodAmountHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interestRateStream"></param>
        /// <param name="stubCalculationPeriod"></param>
        /// <param name="stubValue"></param>
        public static void UpdateStubCalculationPeriod(InterestRateStream interestRateStream, CalculationPeriod stubCalculationPeriod, StubValue stubValue)
        {
            if (XsdClassesFieldResolver.StubValueHasStubRateArray(stubValue))
            {
                Decimal fixedRate = XsdClassesFieldResolver.GetStubValueStubRateArray(stubValue)[0];
                // Fixed rate
                //
                XsdClassesFieldResolver.SetCalculationPeriodFixedRate(stubCalculationPeriod, fixedRate);
            }
            //else if (XsdClassesFieldResolver.StubValue_HasFloatingRateArray(stubValue))
            //{
            //    //Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmount_GetCalculation(interestRateStream.calculationPeriodAmount);

            //    //FloatingRate floatingRate = XsdClassesFieldResolver.GetStubValue_FloatingRateArray(stubValue)[0];

            //    //FloatingRateDefinition floatingRateDefinition = ForecastRateHelper.UpdateFloatingRateDefinition(floatingRate,
            //    //                                                                             calculation.dayCountFraction,
            //    //                                                                             stubCalculationPeriod);

            //    // no observed, no calculated rate, spread == 0.0
            //    //
            //    FloatingRateDefinition floatingRateDefinition = ForecastRateHelper.CreateFloatingRateDefinition(stubCalculationPeriod);
            //    XsdClassesFieldResolver.CalculationPeriod_SetFloatingRateDefinition(stubCalculationPeriod, floatingRateDefinition);


            //    //// Floating rate is set on UpdateCashflows phase.
            //    ////
            //    //XsdClassesFieldResolver.CalculationPeriod_SetFloatingRateDefinition(stubCalculationPeriod, floatingRateDefinition);
            //    //throw new NotImplementedException("");
            //}
            else if (XsdClassesFieldResolver.StubValueHasStubAmountArray(stubValue))
            {
                throw new NotImplementedException("stubCalculationPeriodAmount with stubAmount is not implemented.");
            }
        }
    }
}