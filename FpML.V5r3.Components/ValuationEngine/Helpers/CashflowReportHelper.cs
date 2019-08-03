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
using System.Linq;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class CashflowReportHelper
    {
        public static object DoReport(PriceableCashflow expectedCashflow)
        {
            if (expectedCashflow != null)
            {
                Debug.Print("Coupon: coupon type: {0},payment date: {1}, npv : {2}, multiplier : {3}, payment amount: {4}, forecast amount: {5}",
                    expectedCashflow.CashflowType, expectedCashflow.PaymentDate,
                            expectedCashflow.NPV, expectedCashflow.Multiplier, expectedCashflow.PaymentAmount, expectedCashflow.ForecastAmount);
            }
            return null;
        }

        public static object[] DoCashflowReport(string identifier, PriceableCashflow expectedCashflow)
        {
            if (expectedCashflow != null)
            {
                var result = new object[11];
                result[0] = identifier;
                result[1] = expectedCashflow.CashflowType.Value;
                result[2] = expectedCashflow.PaymentDate;
                result[3] = expectedCashflow.PaymentAmount.currency.Value;
                if (expectedCashflow.ForecastAmount != null)
                {
                    result[4] = expectedCashflow.ForecastAmount.amount;
                }
                else
                {
                    result[4] = expectedCashflow.PaymentAmount.amount;
                }
                result[5] = expectedCashflow.ModelIdentifier;
                result[6] = expectedCashflow.PaymentDiscountFactor;
                result[7] = expectedCashflow.NPV.amount;
                result[8] = expectedCashflow.YearFractionToCashFlowPayment;
                result[9] = expectedCashflow.IsCollateralised;
                result[10] = expectedCashflow.PayerIsBaseParty;
                return result;
            }
            return null;
        }

        public static List<object[]> DoCashflowReport(string identifier, PriceableInterestRateStream interestRateStream)
        {
            if (interestRateStream != null)
            {
                var result = new List<object[]>();
                if (interestRateStream.Coupons != null)
                {
                    result.AddRange(interestRateStream.Coupons.Select(cashflow => DoCashflowReport(identifier, cashflow)));
                }
                if (interestRateStream.Exchanges != null)
                {
                    result.AddRange(interestRateStream.Exchanges.Select(principal => DoCashflowReport(identifier, principal)));
                }
                return result;
            }
            return null;
        }
    }
}