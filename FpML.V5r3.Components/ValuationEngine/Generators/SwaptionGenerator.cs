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
        public static Swaption GenerateSwaptionDefiniton(SwapLegParametersRange_Old leg1Parameters,
                                                        IBusinessCalendar leg1PaymentCalendar,
                                                        SwapLegParametersRange_Old leg2Parameters,
                                                        IBusinessCalendar leg2PaymentCalendar,
                                                        SwaptionParametersRange swaptionParameters)
        {

            Swap swap = SwapGenerator.GenerateDefiniton(leg1Parameters, leg2Parameters);
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