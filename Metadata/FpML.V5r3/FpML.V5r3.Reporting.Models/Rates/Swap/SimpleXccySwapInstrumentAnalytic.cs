using System;

namespace Orion.Models.Rates.Swap
{
    public class SimpleXccySwapInstrumentAnalytic : SimpleIRSwapInstrumentAnalytic
    {
        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns></returns>
        public override  Decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            //TODO need to handle the case of a discount swap. There is no parameter flag yet for this.
            //Also need to add in the fx exchange rate.
            if (AnalyticParameters.IsPayFixedInd)
            {
                if (AnalyticParameters.PayStreamAccrualFactor != 0)
                {
                    var npv = AnalyticParameters.PayStreamFloatingNPV - AnalyticParameters.ReceiveStreamNPV;
                    result = npv / AnalyticParameters.PayStreamAccrualFactor / 10000;
                }
            }
            else
            {
                if (AnalyticParameters.ReceiveStreamAccrualFactor != 0)
                {
                    var npv = AnalyticParameters.ReceiveStreamFloatingNPV - AnalyticParameters.PayStreamNPV;
                    result = npv / AnalyticParameters.ReceiveStreamAccrualFactor / 10000;
                }
            }
            return result;
        }
    }
}