#region Usings

using System.Linq;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Reports;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class InterestRateSwapFpMLCashflowsReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is InterestRateSwapPricer interestRateSwap)
            {
                var fixedStream = new InterestRateStream
                                      {cashflows = new Cashflows
                                                          {
                                                              paymentCalculationPeriod = interestRateSwap.ReceiveLeg.Coupons.Select(fixedRateCoupon => fixedRateCoupon.Build()).ToArray()
                                                          }
                                      };
                var floatStream = new InterestRateStream
                                      {
                                          cashflows = new Cashflows { paymentCalculationPeriod = interestRateSwap.ReceiveLeg.Coupons.Select(payRateCoupon => payRateCoupon.Build()).ToArray() }
                                      };
                var fpmlSwap = new Swap
                                   {
                                       swapStream = new[] {fixedStream, floatStream}
                                   };
                return fpmlSwap;
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase instrument)
        {
            return null;
        }

        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer)
        {
            return null;
        }
    }
}