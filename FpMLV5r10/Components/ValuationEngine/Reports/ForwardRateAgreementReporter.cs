#region Usings

using System.Diagnostics;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Reports;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Pricers.Products;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class ForwardRateAgreementReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is FraPricer forwardRateAgreement)
            {
                Debug.Print("Fra leg {0} coupons", forwardRateAgreement.GetChildren().Count);
                foreach (var receiveRateCoupon in forwardRateAgreement.GetChildren())
                {
                    var flow = (PriceableRateCoupon) receiveRateCoupon;
                    var forecast = flow.ForecastAmount ?? new Money();
                    Debug.Print("Cashflow type: {0}, payment date: {1}, payment amount: {2}, forecast amount:  {3}",
                        flow.CashflowType, flow.PaymentDate, flow.PaymentAmount, forecast);
                }
            }
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase instrument)
        {
            if (instrument is FraPricer forwardRateAgreement)
            {
                var result = new object[forwardRateAgreement.GetChildren().Count, 7];

                var index = 0;
                foreach (var receiveRateCoupon in forwardRateAgreement.GetChildren())
                {
                    var flow = (PriceableRateCoupon)receiveRateCoupon;
                    result[index, 0] = flow.CashflowType.Value;
                    result[index, 1] = flow.PaymentDate;
                    result[index, 2] = flow.AccrualStartDate;
                    result[index, 3] = flow.AccrualEndDate;
                    result[index, 4] = flow.NotionalAmount.amount;
                    result[index, 5] = flow.Rate;
                    if (flow.ForecastAmount != null)
                    {
                        result[index, 6] = flow.ForecastAmount.amount;
                    }
                    else
                    {
                        result[index, 6] = flow.PaymentAmount.amount;
                    }

                    index++;
                }
                return result;
            }
            return null;
        }

        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer)
        {
            if (pricer is FraPricer payment)
            {
                var result = new List<object[]>();
                foreach (var cashflow in payment.GetChildren())
                {
                    var flow = (PriceableCashflow)cashflow;
                    var reportHelper = CashflowReportHelper.DoCashflowReport(pricer.Id, flow);
                    reportHelper[0] = pricer.Id;
                    result.Add(reportHelper);
                }
                return result;
            }
            return null;
        }

        public object DoReport(Fra forwardRateAgreement)
        {
            if (forwardRateAgreement != null)
            {
                var result = new object[9, 2];
                result[0, 0] = "adjustedEffectiveDate";
                result[1, 0] = "fixedRate";
                result[2, 0] = "paymentDate";
                result[3, 0] = "calculationPeriodNumberOfDays";
                result[4, 0] = "adjustedTerminationDate";
                result[5, 0] = "buyerPartyReference";
                result[6, 0] = "sellerPartyReference";
                result[7, 0] = "currency";
                result[8, 0] = "notionalamount";

                result[0, 1] = forwardRateAgreement.adjustedEffectiveDate.Value;
                result[1, 1] = forwardRateAgreement.fixedRate;
                result[2, 1] = forwardRateAgreement.paymentDate.unadjustedDate.Value;
                result[3, 1] = forwardRateAgreement.calculationPeriodNumberOfDays;
                result[4, 1] = forwardRateAgreement.adjustedTerminationDate;
                result[5, 1] = forwardRateAgreement.buyerPartyReference.href;
                result[6, 1] = forwardRateAgreement.sellerPartyReference.href;
                result[7, 1] = forwardRateAgreement.notional.currency.Value;
                result[8, 1] = forwardRateAgreement.notional.amount;

                return result;
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is Fra forwardRateAgreement)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[11, 2];
                result[0, 0] = "adjustedEffectiveDate";
                result[1, 0] = "fixedRate";
                result[2, 0] = "paymentDate";
                result[3, 0] = "calculationPeriodNumberOfDays";
                result[4, 0] = "adjustedTerminationDate";
                result[5, 0] = "buyerPartyReference";
                result[6, 0] = "sellerPartyReference";
                result[7, 0] = "currency";
                result[8, 0] = "notionalamount";
                result[9, 0] = "party1";
                result[10, 0] = "party2";

                result[0, 1] = forwardRateAgreement.adjustedEffectiveDate.Value;
                result[1, 1] = forwardRateAgreement.fixedRate;
                result[2, 1] = forwardRateAgreement.paymentDate.unadjustedDate.Value;
                result[3, 1] = forwardRateAgreement.calculationPeriodNumberOfDays;
                result[4, 1] = forwardRateAgreement.adjustedTerminationDate;
                result[5, 1] = forwardRateAgreement.buyerPartyReference.href;
                result[6, 1] = forwardRateAgreement.sellerPartyReference.href;
                result[7, 1] = forwardRateAgreement.notional.currency.Value;
                result[8, 1] = forwardRateAgreement.notional.amount;
                result[9, 1] = party1;
                result[10, 1] = party2;

                return result;
            }
            return null;
        }
    }
}