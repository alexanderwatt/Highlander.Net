#region Using directivess

using System;
using FpML.V5r10.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Constants;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.ValuationEngine.Generators
{
    /// <summary>
    /// Internally uses SwapGenerator to create "swap" part of the swaption.
    /// </summary>
    public static class SwaptionGenerator
    {

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