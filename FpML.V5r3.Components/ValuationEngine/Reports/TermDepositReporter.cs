#region Usings

using System.Diagnostics;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Reports;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class TermDepositReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is TermDepositPricer termDeposit)
            {
                Debug.Print("Number of coupons : {0}", termDeposit.GetChildren().Count);
                foreach (var receiveRateCoupon in termDeposit.GetChildren())
                {
                    var flow = (PriceableRateCoupon) receiveRateCoupon;
                    var forecast = flow.ForecastAmount ?? new Money();
                    Debug.Print("Coupon: coupon type: {0},payment date: {1}, notional amount : {2}, fixed rate : {3}, payment amount: {4}, forecast amount:  {5}",
                        flow.CashflowType, flow.PaymentDate, flow.Notional, flow.Rate, flow.PaymentAmount, forecast);
                }
            }
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase pricer)
        {
            if (pricer is TermDepositPricer termDeposit)
            {
                var result = new object[termDeposit.GetChildren().Count, 5];

                var index = 0;
                foreach (var cashflow in termDeposit.GetChildren())
                {
                    var flow = (PriceableCashflow)cashflow;
                    result[index, 0] = flow.CashflowType.Value;
                    result[index, 1] = flow.PaymentDate;
                    result[index, 2] = flow.PaymentAmount.currency.Value;
                    if (flow.ForecastAmount != null)
                    {
                        result[index, 3] = flow.ForecastAmount.amount;
                    }
                    else
                    {
                        result[index, 3] = flow.PaymentAmount.amount;
                    }
                    result[index, 4] = flow.ProductType.ToString();
                    //result[index, 5] = flow.;
                    index++;
                }
                return result;
            }
            return null;
        }

        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer)
        {
            if (pricer is TermDepositPricer termDeposit)
            {
                var result = new List<object[]>();
                foreach (var cashflow in termDeposit.GetChildren())
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

        //public static object[,] DoReport(TermDeposit termDeposit)
        //{
        //    if (termDeposit != null)
        //    {
        //        var result = new object[10, 2];
        //        result[0, 0] = "adjustedEffectiveDate";
        //        result[1, 0] = "fixedRate";
        //        result[2, 0] = "maturityDate";
        //        result[3, 0] = "dayCountFraction";
        //        result[4, 0] = "lenderPartyReference";
        //        result[5, 0] = "borrowerPartyReference";
        //        result[6, 0] = "currency";
        //        result[7, 0] = "notionalamount";
        //        result[8, 0] = "interest";
        //        result[9, 0] = "dayCount";

        //        result[0, 1] = termDeposit.startDate;
        //        result[1, 1] = termDeposit.fixedRate;
        //        result[2, 1] = termDeposit.maturityDate;
        //        result[3, 1] = termDeposit.dayCountFraction.Value;
        //        result[4, 1] = termDeposit.initialPayerReference.href;
        //        result[5, 1] = termDeposit.initialReceiverReference.href;
        //        result[6, 1] = termDeposit.principal.currency.Value;
        //        result[7, 1] = termDeposit.principal.amount;
        //        result[8, 1] = termDeposit.interest.amount;
        //        result[9, 1] = termDeposit.dayCountFraction.Value;
        //        return result;
        //    }
        //    return null;
        //}

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is TermDeposit termDeposit)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[11, 2];
                result[0, 0] = "adjustedEffectiveDate";
                result[1, 0] = "fixedRate";
                result[2, 0] = "maturityDate";
                result[3, 0] = "dayCountFraction";
                result[4, 0] = "lenderPartyReference";
                result[5, 0] = "borrowerPartyReference";
                result[6, 0] = "currency";
                result[7, 0] = "notionalamount";
                //result[8, 0] = "interest";
                result[8, 0] = "dayCount";
                result[9, 0] = "party1";
                result[10, 0] = "party2";
                result[0, 1] = termDeposit.startDate;
                result[1, 1] = termDeposit.fixedRate;
                result[2, 1] = termDeposit.maturityDate;
                result[3, 1] = termDeposit.dayCountFraction.Value;
                result[4, 1] = termDeposit.payerPartyReference.href;
                result[5, 1] = termDeposit.receiverPartyReference.href;
                result[6, 1] = termDeposit.principal.currency.Value;
                result[7, 1] = termDeposit.principal.amount;
                //result[8, 1] = termDeposit.interest.amount;
                result[8, 1] = termDeposit.dayCountFraction.Value;
                result[9, 1] = party1;
                result[10, 1] = party2;
                return result;
            }
            return null;
        }
    }
}