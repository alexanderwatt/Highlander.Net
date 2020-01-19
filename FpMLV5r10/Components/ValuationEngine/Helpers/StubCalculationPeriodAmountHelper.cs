#region Using directives

using System;
using FpML.V5r10.Reporting;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    class StubCalculationPeriodAmountHelper
    {
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