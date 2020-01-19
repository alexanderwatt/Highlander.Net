#region Usings

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Reports;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Pricers.Assets;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class FutureTransactionReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is FutureTransactionPricer future)
            {
                Debug.Print("Futures {0} underlyer", future.GetChildren().Count);
                foreach (var cashflow in future.GetChildren())
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
            if (pricer is FutureTransactionPricer future)
            {
                var result = new object[future.GetChildren().Count, 5];
                var index = 0;
                foreach (var cashflow in future.GetChildren())
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
            if (pricer is FutureTransactionPricer future)
            {
                return (from PriceableCashflow flow in future.GetChildren() select CashflowReportHelper.DoCashflowReport(pricer.Id, flow)).ToList();
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is FutureTransaction futureTransaction)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[10, 2];
                result[0, 0] = "buyerPartyReference";
                result[1, 0] = "sellerPartyReference";
                result[2, 0] = "numberOfUnits";
                result[3, 0] = "currency";
                result[4, 0] = "amount";
                result[5, 0] = "maturity";
                result[6, 0] = "id";
                result[7, 0] = "paryt1";
                result[8, 0] = "party2";
                result[9, 0] = "exchangeId";
                var temp = futureTransaction.future;
                result[0, 1] = futureTransaction.buyerPartyReference.href;
                result[1, 1] = futureTransaction.sellerPartyReference.href;
                result[2, 1] = futureTransaction.numberOfUnits;
                result[3, 1] = futureTransaction.unitPrice.currency.Value;
                result[4, 1] = 0.0m;
                if (futureTransaction.unitPrice.amountSpecified)
                {
                    result[4, 1] = futureTransaction.unitPrice.amount;
                }
                result[5, 1] = DateTime.MinValue;
                if (temp.maturitySpecified)
                {
                    result[5, 1] = temp.maturity;
                }
                result[6, 1] = temp.id;
                result[7, 1] = party1;
                result[8, 1] = party2;
                result[9, 1] = "Unknown";
                if (futureTransaction.future.exchangeId != null)
                {
                    result[9, 1] = futureTransaction.future.exchangeId.Value;
                }
                return result;
            }
            return null;
        }
    }
}