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

#region Using directivess

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Constants;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.ValuationEngine.Generators
{
    /// <summary>
    /// Internally uses SwapGenerator to create "swap" part of the swaption.
    /// </summary>
    public static class SwaptionGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1PaymentCalendar"></param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2PaymentCalendar"></param>
        /// <param name="swaptionParameters"></param>
        /// <returns></returns>
        public static Swaption GenerateSwaptionDefinition(SwapLegParametersRange_Old leg1Parameters,
                                                        IBusinessCalendar leg1PaymentCalendar,
                                                        SwapLegParametersRange_Old leg2Parameters,
                                                        IBusinessCalendar leg2PaymentCalendar,
                                                        SwaptionParametersRange swaptionParameters)
        {

            Swap swap = SwapGenerator.GenerateDefinition(leg1Parameters, leg2Parameters);
            NonNegativeMoney premium = MoneyHelper.GetNonNegativeAmount(swaptionParameters.Premium, swaptionParameters.PremiumCurrency);
            AdjustableDate expirationDate = DateTypesHelper.ToAdjustableDate(swaptionParameters.ExpirationDate, swaptionParameters.ExpirationDateBusinessDayAdjustments, swaptionParameters.ExpirationDateCalendar);
            AdjustableOrAdjustedDate paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(swaptionParameters.PaymentDate, swaptionParameters.PaymentDateBusinessDayAdjustments, swaptionParameters.PaymentDateCalendar);
            TimeSpan earliestExerciseTimeAsTimeSpan = TimeSpan.FromDays(swaptionParameters.EarliestExerciseTime);
            DateTime earliestExerciseTime = DateTime.MinValue.Add(earliestExerciseTimeAsTimeSpan);
            TimeSpan expirationTimeAsTimeSpan = TimeSpan.FromDays(swaptionParameters.ExpirationTime);
            DateTime expirationTime = DateTime.MinValue.Add(expirationTimeAsTimeSpan);
            return SwaptionFactory.Create(swap, premium, 
                                          swaptionParameters.PremiumPayer, swaptionParameters.PremiumReceiver, 
                                          paymentDate, expirationDate,
                                          earliestExerciseTime, expirationTime, swaptionParameters.AutomaticExcercise);
        }
    }
}