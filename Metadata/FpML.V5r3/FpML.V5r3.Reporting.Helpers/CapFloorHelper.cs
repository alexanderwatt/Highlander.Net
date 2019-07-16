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

#region Using directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class CapFloorHelper
    {
        public static InterestRateStream GetPayerStream(CapFloor capFloor, string baseParty)
        {
            if (capFloor.capFloorStream.payerPartyReference.href == baseParty)
            {
                return capFloor.capFloorStream;
            }
            throw new System.Exception($"No payer stream was found for the specified payer : '{baseParty}'");
        }

        public static InterestRateStream GetReceiverStream(CapFloor capFloor, string baseParty)
        {
            if (capFloor.capFloorStream.receiverPartyReference.href == baseParty)
            {
                return capFloor.capFloorStream;
            }
            throw new System.Exception($"No receiver stream was found for the specified receiver : '{baseParty}'");
        }

        public static Money GetValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var list = new List<MoneyBase>();
            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    list.AddRange(GetValue(payment.payerPartyReference.href, payment.receiverPartyReference.href, baseParty, payment.paymentAmount));
                }    
            }
            return MoneyHelper.Sum(list);
        }

        public static Money GetPresentValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    list.AddRange(GetValue(payment.payerPartyReference.href, payment.receiverPartyReference.href, baseParty, payment.presentValueAmount));
                }
            }
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceivePresentValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var valueList = new List<Money>();
            if (null != capFloor.additionalPayment)
            {
                valueList.AddRange(from payment in capFloor.additionalPayment where baseParty == payment.receiverPartyReference.href select payment.presentValueAmount);
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayPresentValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var valueList = new List<Money>();
            if (null != capFloor.additionalPayment)
            {
                valueList.AddRange(from payment in capFloor.additionalPayment where baseParty == payment.payerPartyReference.href select payment.presentValueAmount);
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetReceiveFutureValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var valueList = new List<MoneyBase>();
            if (null != capFloor.additionalPayment)
            {
                valueList.AddRange((from payment in capFloor.additionalPayment where baseParty == payment.receiverPartyReference.href select payment.paymentAmount));
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayFutureValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            var valueList = new List<MoneyBase>();
            if (null != capFloor.additionalPayment)
            {
                valueList.AddRange((from payment in capFloor.additionalPayment where baseParty == payment.payerPartyReference.href select payment.paymentAmount));
            }
            return MoneyHelper.Sum(valueList);
        }

        /// <summary>
        /// Returns present value of the swap for a specified baseParty.
        /// </summary>
        /// <param name="capFloor">The cap floor.</param>
        /// <param name="baseParty">The party, from which point of view the valuations are computed.</param>
        /// <returns></returns>
        public static Money GetPresentValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);
            list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, presentValueOfStream));
            Money presentValueOfAdditionalPayments = GetPresentValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(presentValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static List<Money> GetValue(string payer, string receiver, string baseParty, NonNegativeMoney amount)
        {
            var result = new List<Money>();
            if (baseParty == payer)
            {
                result.Add(MoneyHelper.Neg(amount));
            }
            if (baseParty == receiver)
            {
                result.Add(MoneyHelper.CopyFromNonNegativeToMoney(amount));
            }
            return result;
        }


        public static List<Money> GetValue(string payer, string receiver, string baseParty, Money amount)
        {
            var result = new List<Money>();
            if (baseParty == payer)
            {
                result.Add(MoneyHelper.Neg(amount));
            }
            if (baseParty == receiver)
            {
                result.Add(amount);
            }
            return result;
        }

        public static Money GetFutureValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            Money futureValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);
            list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, futureValueOfStream));
            Money futureValueOfAdditionalPayments = GetValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(futureValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceivePresentValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            {
                Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);

                if (baseParty == stream.receiverPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money receivePresentValueOfAdditionalPayments = GetReceivePresentValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(receivePresentValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetPayPresentValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            {
                Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);

                if (baseParty == stream.payerPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money payPresentValueOfAdditionalPayments = GetPayPresentValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(payPresentValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceiveFutureValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            {
                Money presentValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);

                if (baseParty == stream.receiverPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money receiveFutureValueOfAdditionalPayments = GetReceiveFutureValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(receiveFutureValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetPayFutureValue(CapFloor capFloor, string baseParty)
        {
            var list = new List<Money>();
            InterestRateStream stream = capFloor.capFloorStream;
            {
                Money presentValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);

                if (baseParty == stream.payerPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money payFutureValueOfAdditionalPayments = GetPayFutureValueOfAdditionalPayments(capFloor, baseParty);
            list.Add(payFutureValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }


//        Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
//        Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);
//
//        Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
//        Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);
//
//        Money payFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[0].cashflows);
//        Money payFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[0].cashflows);
//
//        Money receiveFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[1].cashflows);
//        Money receiveFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[1].cashflows);


    }
}