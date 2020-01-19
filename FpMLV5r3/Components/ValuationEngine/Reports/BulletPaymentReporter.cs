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
    public class BulletPaymentReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is BulletPaymentPricer payment)
            {
                Debug.Print("Payment {0} coupons", payment.GetChildren().Count);
                foreach (var cashflow in payment.GetChildren())
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
            if (pricer is BulletPaymentPricer payment)
            {
                var result = new object[payment.GetChildren().Count, 5];
                var index = 0;
                foreach (var cashflow in payment.GetChildren())
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
            if (pricer is BulletPaymentPricer payment)
            {
                return (from PriceableCashflow flow in payment.GetChildren() select CashflowReportHelper.DoCashflowReport(pricer.Id, flow)).ToList();
            }
            return null;
        }

        //public static object[,] DoReport(BulletPayment bulletPayment)
        //{
        //    if (bulletPayment != null)
        //    {
        //        var result = new object[7, 2];
        //        result[0, 0] = "adjustedPaymentDate";
        //        result[1, 0] = "paymentAmount";
        //        result[2, 0] = "currency";
        //        result[3, 0] = "paymentType";
        //        result[4, 0] = "lenderPartyReference";
        //        result[5, 0] = "borrowerPartyReference";
        //        result[6, 0] = "currency";

        //        result[0, 1] = bulletPayment.payment.adjustedPaymentDate;
        //        result[1, 1] = bulletPayment.payment.paymentAmount.amount;
        //        result[2, 1] = bulletPayment.payment.paymentAmount.currency.Value;
        //        result[3, 1] = bulletPayment.payment.paymentType.Value;
        //        result[4, 1] = bulletPayment.payment.payerPartyReference.href;
        //        result[5, 1] = bulletPayment.payment.receiverPartyReference.href;
        //        result[6, 1] = bulletPayment.payment.presentValueAmount!= null ? bulletPayment.payment.presentValueAmount.amount : 0.0m;

        //        return result;
        //    }
        //    return null;
        //}

        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is BulletPayment payment)
            {
                var party1 = properties.GetValue<string>(TradeProp.Party1, true);
                var party2 = properties.GetValue<string>(TradeProp.Party2, true);
                var result = new object[7, 2];
                result[0, 0] = "payerPartyReference";
                result[1, 0] = "receiverPartyReference";
                result[2, 0] = "amount";
                result[3, 0] = "currency";
                result[4, 0] = "businessDayConvention";
                result[5, 0] = "party1";
                result[6, 0] = "party2";
                var temp = payment.payment;
                result[0, 1] = temp.payerPartyReference.href;
                result[1, 1] = temp.receiverPartyReference.href;
                result[2, 1] = temp.paymentAmount.amount;
                result[3, 1] = temp.paymentAmount.currency.Value;
                var containsBusinessCenters = AdjustableOrAdjustedDateHelper.Contains(temp.paymentDate, ItemsChoiceType.dateAdjustments, out var businessDayAdjustments);
                if (containsBusinessCenters && businessDayAdjustments != null)
                {
                    var businessDayConvention = ((BusinessDayAdjustments)businessDayAdjustments).businessDayConvention.ToString();
                    result[4, 1] = businessDayConvention;
                }
                else
                {
                    result[4, 1] = "NONE";
                }
                result[5, 1] = party1;
                result[6, 1] = party2;
                return result;
            }
            return null;
        }
    }
}