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
    public class FxSingleLegReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is FxSingleLegPricer fxLeg)
            {
                Debug.Print("Payment {0} coupons", fxLeg.GetChildren().Count);
                foreach (var cashflow in fxLeg.GetChildren())
                {
                    var flow = (PriceableCashflow)cashflow;
                    var forecast = flow.ForecastAmount ?? new Money();
                    Debug.Print("Cashflow type: {0}, payment date: {1}, payment amount: {2}, forecast amount:  {3}",
                        flow.CashflowType, flow.PaymentDate, flow.PaymentAmount, forecast);
                }
            }
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase pricer)
        {
            if (pricer is FxSingleLegPricer fxLeg)
            {
                var result = new object[fxLeg.GetChildren().Count, 5];

                var index = 0;
                foreach (var cashflow in fxLeg.GetChildren())
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
            if (pricer is FxSingleLegPricer payment)
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
            if (product is FxSingleLeg fxLeg)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[13, 2];
                result[0, 0] = "ValueDate";
                result[1, 0] = "Payment1payerPartyReference";
                result[2, 0] = "Payment1receiverPartyReference";
                result[3, 0] = "Payment1amount";
                result[4, 0] = "Payment1currency";
                result[5, 0] = "Payment2payerPartyReference";
                result[6, 0] = "Payment2receiverPartyReference";
                result[7, 0] = "Payment2amount";
                result[8, 0] = "Payment2currency";
                result[9, 0] = "QuoteBasis";
                result[10, 0] = "Rate";
                result[11, 0] = "party1";
                result[12, 0] = "party2";
                result[0, 1] = "No single vlaue date.";
                if (fxLeg.Items1ElementName[0] == Items1ChoiceType.valueDate)
                {
                    result[0, 1] = fxLeg.Items1[0];
                }
                result[1, 1] = fxLeg.exchangedCurrency1.payerPartyReference.href;
                result[2, 1] = fxLeg.exchangedCurrency1.receiverPartyReference.href;
                result[3, 1] = fxLeg.exchangedCurrency1.paymentAmount.amount;
                result[4, 1] = fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
                result[5, 1] = fxLeg.exchangedCurrency2.payerPartyReference.href;
                result[6, 1] = fxLeg.exchangedCurrency2.receiverPartyReference.href;
                result[7, 1] = fxLeg.exchangedCurrency2.paymentAmount.amount;
                result[8, 1] = fxLeg.exchangedCurrency2.paymentAmount.currency.Value;
                result[9, 1] = fxLeg.exchangeRate.quotedCurrencyPair.quoteBasis.ToString();
                result[10, 1] = fxLeg.exchangeRate.rate;
                result[11, 1] = party1;
                result[12, 1] = party2;

                return result;
            }
            return null;
        }
    }
}