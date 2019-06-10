/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System.Diagnostics;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Reports;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class FxOptionLegReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is VanillaEuropeanFxOptionPricer fxSwap)
            {
                Debug.Print("Payment {0} coupons", fxSwap.GetChildren().Count);
                foreach (var cashflow in fxSwap.GetChildren())
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
            if (pricer is VanillaEuropeanFxOptionPricer fxSwap)
            {
                var result = new object[fxSwap.GetChildren().Count, 5];

                var index = 0;
                foreach (var cashflow in fxSwap.GetChildren())
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
            if (pricer is VanillaEuropeanFxOptionPricer payment)
            {
                var result = new List<object[]>();
                var index = 0;
                foreach (var cashflow in payment.GetChildren())
                {
                    var flow = (PriceableCashflow)cashflow;
                    var reportHelper = CashflowReportHelper.DoCashflowReport(pricer.Id, flow);
                    reportHelper[0] = pricer.Id;
                    result[index] = reportHelper;
                    index++;
                }
                return result;
            }
            return null;
        }

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is FxOption fxOption)
            {
                var result = new object[16, 2];
                result[0, 0] = "buyerPartyReference";
                result[1, 0] = "sellerPartyReference";
                result[2, 0] = "callCurrencyAmount";
                result[3, 0] = "callCurrency";
                result[4, 0] = "putCurrencyAmount";
                result[5, 0] = "putCurrency";
                result[6, 0] = "expiryDate";
                result[7, 0] = "expiryTimeBusinessCenter";
                result[8, 0] = "expiryTime";
                result[9, 0] = "fxStrikePrice";
                result[10, 0] = "strikeQuoteBasis";
                result[11, 0] = "premium";
                result[12, 0] = "spotRate";
                result[13, 0] = "quotedTenor";
                result[14, 0] = "cashSettlement";
                result[15, 0] = "valueDate";
                result[0, 1] = fxOption.buyerPartyReference.href;
                result[1, 1] = fxOption.sellerPartyReference.href;
                result[2, 1] = fxOption.callCurrencyAmount.amount;
                result[3, 1] = fxOption.callCurrencyAmount.currency.Value;
                result[4, 1] = fxOption.putCurrencyAmount.amount;
                result[5, 1] = fxOption.putCurrencyAmount.currency.Value;
                if (fxOption.Item is FxEuropeanExercise item)
                {
                    result[6, 1] = item.expiryDate;
                    result[7, 1] = item.expiryTime.businessCenter.Value;
                    result[8, 1] = item.expiryTime.hourMinuteTime;
                    result[15, 1] = item.valueDate;
                }
                if (fxOption.strike.rateSpecified)
                {
                    result[9, 1] = fxOption.strike.rate;
                }
                if (fxOption.strike.strikeQuoteBasisSpecified)
                {
                    result[10, 1] = fxOption.strike.strikeQuoteBasis.ToString();
                }
                if (fxOption.premium != null && fxOption.premium.Length > 0)
                {
                    if (fxOption.premium[0].quote.valueSpecified)
                    {
                        result[11, 1] = fxOption.premium[0].quote.value;
                    }
                }
                if (fxOption.spotRateSpecified)
                {
                    result[12, 1] = fxOption.spotRate;
                }
                if (fxOption.tenorPeriod != null)
                {
                    result[13, 1] = fxOption.tenorPeriod.ToString();
                }
                else
                {
                    result[13, 1] = "No tenor set";
                }
                if (fxOption.cashSettlement != null)
                {
                    result[14, 1] = fxOption.cashSettlement.ToString();
                }               
                return result;
            }
            return null;
        }
    }
}