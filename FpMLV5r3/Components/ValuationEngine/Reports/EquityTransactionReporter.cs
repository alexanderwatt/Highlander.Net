/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Highlander.Utilities.NamedValues;
using Highlander.Reporting.V5r3;
using Highlander.Codes.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.Reporting.ModelFramework.V5r3.Reports;
using Highlander.ValuationEngine.V5r3.Helpers;
using Highlander.ValuationEngine.V5r3.Instruments;
using Highlander.ValuationEngine.V5r3.Pricers;

#endregion

namespace Highlander.ValuationEngine.V5r3.Reports
{
    public class EquityTransactionReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is EquityTransactionPricer equity)
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
            if (pricer is EquityTransactionPricer bond)
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
            if (pricer is EquityTransactionPricer bond)
            {
                return (from PriceableCashflow flow in bond.GetChildren() select CashflowReportHelper.DoCashflowReport(pricer.Id, flow)).ToList();
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is EquityTransaction equityTransaction)
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
                result[6, 0] = "party1";
                result[7, 0] = "party2";
                result[8, 0] = "description";
                var temp = equityTransaction.equity;
                result[0, 1] = equityTransaction.buyerPartyReference.href;
                result[1, 1] = equityTransaction.sellerPartyReference.href;
                result[2, 1] = equityTransaction.numberOfUnits;
                result[3, 1] = equityTransaction.unitPrice.currency.Value;
                result[4, 1] = equityTransaction.unitPrice.amount;
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