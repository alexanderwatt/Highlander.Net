#region Using directives

using System;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    internal class InterestRateSwapFairSpreadObjectiveFunction
    {
        private readonly SwapLegParametersRange _payLegParametersRange;
        private readonly SwapLegParametersRange _receiveLegParametersRange;
        private readonly ValuationRange         _valuationRange;

        public InterestRateSwapFairSpreadObjectiveFunction(SwapLegParametersRange payLegParametersRange, SwapLegParametersRange receiveLegParametersRange, ValuationRange valuationRange)
        {
            _payLegParametersRange = payLegParametersRange;
            _receiveLegParametersRange = receiveLegParametersRange;
            _valuationRange = valuationRange;
        }

        public double Value(double spread)
        {
            var pricer = new InterestRateSwapPricer();

            //  We can safely assign SPREAD to both (fixed and floating sides) legs as a FIXED side ignores this value
            //
            _payLegParametersRange.FloatingRateSpread = _receiveLegParametersRange.FloatingRateSpread = (decimal)spread;

//                    public ValuationResultRange GetPrice(
//                                    ValuationRange valuationRange,
//                                    TradeRange tradeRange,
//                                    SwapLegParametersRange leg1ParametersRange,
//                                    SwapLegParametersRange leg2ParametersRange,
//                                    List<DetailedCashflowRangeItem> leg1DetailedCashflowsListArray,
//                                    List<DetailedCashflowRangeItem> leg2DetailedCashflowsListArray,
//                                    List<PrincipleExchangeCashflowRangeItem> leg1PrincipleExchangeCashflowListArray,
//                                    List<PrincipleExchangeCashflowRangeItem> leg2PrincipleExchangeCashflowListArray,
//                                    List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
//                                    List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray)

            throw new NotImplementedException();

//            ValuationResultRange valuationResult = pricer.GetPrice(_valuationRange, null, 
//                                                                    _payLegParametersRange, _receiveLegParametersRange,
//                                                                    new List<DetailedCashflowRangeItem>(), 
//                                                                    new List<DetailedCashflowRangeItem>(), 
//                                                                    new List<PrincipleExchangeCashflowRangeItem>(), 
//                                                                    new List<PrincipleExchangeCashflowRangeItem>(), 
//                                                                    new List<AdditionalPaymentRangeItem>(), 
//                                                                    new List<AdditionalPaymentRangeItem>()
//                                                                    );
//
//
//
//            return (double)valuationResult.PresentValue;
        }
    }
}