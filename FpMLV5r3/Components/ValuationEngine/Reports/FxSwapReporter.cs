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
using System.Collections.Generic;
using Highlander.Reporting.Helpers.V5r3;
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
    public class FxSwapReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is FxSwapPricer fxswap)
            {
                Debug.Print("Payment {0} coupons", fxswap.GetChildren().Count);
                foreach (var cashflow in fxswap.GetChildren())
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
            if (pricer is FxSwapPricer fxswap)
            {
                var result = new object[fxswap.GetChildren().Count, 5];

                var index = 0;
                foreach (var cashflow in fxswap.GetChildren())
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
            if (pricer is FxSwapPricer payment)
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
        //        result[7, 0] = "notionalAmount";
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
            if (product is FxSwap fxSwap)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[7, 2];
                result[0, 0] = "payerPartyReference";
                result[1, 0] = "receiverPartyReference";
                result[2, 0] = "amount";
                result[3, 0] = "currency";
                result[4, 0] = "paymentDate";
                result[5, 0] = "party1";
                result[6, 0] = "party2";
                var temp1 = fxSwap.nearLeg;
                result[0, 1] = temp1.exchangedCurrency1.payerPartyReference.href;
                result[1, 1] = temp1.exchangedCurrency1.receiverPartyReference.href;
                result[2, 1] = temp1.exchangedCurrency1.paymentAmount.amount;
                result[3, 1] = temp1.exchangedCurrency1.paymentAmount.currency.Value;
                var containedUnadjustedDate = AdjustableOrAdjustedDateHelper.Contains(temp1.exchangedCurrency1.paymentDate, ItemsChoiceType.unadjustedDate, out var unadjustedDate);
                if (containedUnadjustedDate)
                {
                    result[4, 1] = ((IdentifiedDate) unadjustedDate).Value;
                }              
                result[5, 1] = party1;
                result[6, 1] = party2;
                return result;
            }
            return null;
        }
    }
}