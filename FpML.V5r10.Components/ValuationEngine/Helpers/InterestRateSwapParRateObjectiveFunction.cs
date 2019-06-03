#region Using directives

using System;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    internal class InterestRateSwapParRateObjectiveFunction
    {
        private readonly SwapLegParametersRange _payLegParametersRange;
        private readonly SwapLegParametersRange _receiveLegParametersRange;
        private readonly ValuationRange _valuationRange;

        public InterestRateSwapParRateObjectiveFunction(SwapLegParametersRange payLegParametersRange, SwapLegParametersRange receiveLegParametersRange, ValuationRange valuationRange)
        {
            _payLegParametersRange = payLegParametersRange;
            _receiveLegParametersRange = receiveLegParametersRange;
            _valuationRange = valuationRange;
        }

        public double Value(double parRate)
        {
            var pricer = new InterestRateSwapPricer();

            //  We can safely assign FIXED rate it to both (fixed and floating sides) legs as a floating side ignores this value
            //
            _payLegParametersRange.CouponOrLastResetRate = _receiveLegParametersRange.CouponOrLastResetRate = (decimal)parRate;
            //            ValuationResultRange valuationResult = pricer.GetPrice(_payLegParametersRange, _receiveLegParametersRange, _valuationRange);
            //
            //            return (double)valuationResult.PresentValue;

            throw new NotImplementedException();
        }
    }
}