#region Usings

using System.Diagnostics;
using System.Linq;
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
    public class PropertyTransactionReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is PropertyTransactionPricer equity)
            {
                Debug.Print("Equity {0} dividends", equity.GetChildren().Count);
                foreach (var cashflow in equity.GetChildren())
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
            if (pricer is PropertyTransactionPricer bond)
            {
                var result = new object[bond.GetChildren().Count, 5];
                var index = 0;
                foreach (var cashflow in bond.GetChildren())
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
            if (pricer is PropertyTransactionPricer bond)
            {
                return (from PriceableCashflow flow in bond.GetChildren() select CashflowReportHelper.DoCashflowReport(pricer.Id, flow)).ToList();
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is PropertyTransaction propertyTransaction)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[9, 2];
                result[0, 0] = "buyerPartyReference";
                result[1, 0] = "sellerPartyReference";
                result[2, 0] = "numberOfUnits";
                result[3, 0] = "unitPriceCurrency";
                result[4, 0] = "unitPriceAmount";
                result[5, 0] = "id";
                result[6, 0] = "paryt1";
                result[7, 0] = "party2";
                result[8, 0] = "description";
                var temp = propertyTransaction.property;
                result[0, 1] = propertyTransaction.buyerPartyReference.href;
                result[1, 1] = propertyTransaction.sellerPartyReference.href;
                result[2, 1] = propertyTransaction.purchasePrice.amount;
                result[3, 1] = temp.currency.Value;
                result[4, 1] = propertyTransaction.purchasePrice.amount;
                result[5, 1] = temp.id;
                result[6, 1] = party1;
                result[7, 1] = party2;
                result[8, 1] = temp.description;
                return result;
            }
            return null;
        }
    }
}