#region Using directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class SwapHelper
    {
        public static InterestRateStream GetPayerStream(Swap swap, string baseParty)
        {
            foreach (InterestRateStream stream in swap.swapStream)
            {
                if (stream.payerPartyReference.href == baseParty)
                {
                    return stream;                    
                }
            }
            throw new System.Exception($"No payer stream was found for the specified payer : '{baseParty}'");
        }

        public static InterestRateStream GetReceiverStream(Swap swap, string baseParty)
        {
            foreach (InterestRateStream stream in swap.swapStream)
            {
                if (stream.payerPartyReference.href == baseParty)
                {
                    return stream;
                }
            }
            throw new System.Exception($"No receiver stream was found for the specified receiver : '{baseParty}'");
        }

        public static Money GetValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            if (null != swap.additionalPayment)
            {
                foreach (Payment payment in swap.additionalPayment)
                {
                    list.AddRange(GetValue(payment.payerPartyReference.href, payment.receiverPartyReference.href,
                                               baseParty, payment.paymentAmount));
                }    
            }
            return MoneyHelper.Sum(list);
        }

        public static Money GetPresentValueOfAdditionalPayments(Swap swap, string baseParty, Currency currencyToAdd)
        {
            var list = new List<Money>();
            if (null != swap.additionalPayment)
            {
                foreach (Payment payment in swap.additionalPayment)
                {
                    list.AddRange(GetValue(payment.payerPartyReference.href, payment.receiverPartyReference.href, baseParty, payment.presentValueAmount));
                }
            }
            return MoneyHelper.Sum(list, currencyToAdd);
        }

        public static Money GetPresentValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            if (null != swap.additionalPayment)
            {
                foreach (Payment payment in swap.additionalPayment)
                {
                    list.AddRange(GetValue(payment.payerPartyReference.href, payment.receiverPartyReference.href, baseParty, payment.presentValueAmount));
                }
            }
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceivePresentValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var valueList = new List<Money>();
            if (null != swap.additionalPayment)
            {
                valueList.AddRange(from payment in swap.additionalPayment where baseParty == payment.receiverPartyReference.href select payment.presentValueAmount);
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayPresentValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var valueList = new List<Money>();
            if (null != swap.additionalPayment)
            {
                valueList.AddRange(from payment in swap.additionalPayment where baseParty == payment.payerPartyReference.href select payment.presentValueAmount);
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetReceiveFutureValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var valueList = new List<MoneyBase>();
            if (null != swap.additionalPayment)
            {
                valueList.AddRange((from payment in swap.additionalPayment where baseParty == payment.receiverPartyReference.href select payment.paymentAmount));
            }
            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayFutureValueOfAdditionalPayments(Swap swap, string baseParty)
        {
            var valueList = new List<MoneyBase>();
            if (null != swap.additionalPayment)
            {
                valueList.AddRange((from payment in swap.additionalPayment where baseParty == payment.payerPartyReference.href select payment.paymentAmount));
            }
            return MoneyHelper.Sum(valueList);
        }

        /// <summary>
        /// Returns present value of the swap for a specified baseParty.
        /// </summary>
        /// <param name="swap"></param>
        /// <param name="baseParty">The party, from which point of view the valuations are computed.</param>
        /// <returns></returns>
        public static Money GetPresentValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);
                list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, presentValueOfStream));
            }
            Money sumPVs = MoneyHelper.Sum(list);
            //only add a pv in the same currency.
            Money presentValueOfAdditionalPayments = GetPresentValueOfAdditionalPayments(swap, baseParty, sumPVs.currency);
            sumPVs = MoneyHelper.Add(sumPVs, presentValueOfAdditionalPayments);
            return sumPVs;
        }

        public  static  List<Money>   GetValue(string payer, string receiver, string baseParty, Money amount)
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

        public static Money GetFutureValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money futureValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);
                list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, futureValueOfStream));
            }
            Money futureValueOfAdditionalPayments = GetValueOfAdditionalPayments(swap, baseParty);
            list.Add(futureValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceivePresentValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);
                if (baseParty == stream.receiverPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money receivePresentValueOfAdditionalPayments = GetReceivePresentValueOfAdditionalPayments(swap, baseParty);
            list.Add(receivePresentValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetPayPresentValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);
                if (baseParty == stream.payerPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money payPresentValueOfAdditionalPayments = GetPayPresentValueOfAdditionalPayments(swap, baseParty);
            list.Add(payPresentValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetReceiveFutureValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money presentValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);
                if (baseParty == stream.receiverPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money receiveFutureValueOfAdditionalPayments = GetReceiveFutureValueOfAdditionalPayments(swap, baseParty);
            list.Add(receiveFutureValueOfAdditionalPayments);
            return MoneyHelper.Sum(list);
        }

        public static Money GetPayFutureValue(Swap swap, string baseParty)
        {
            var list = new List<Money>();
            foreach (InterestRateStream stream in swap.swapStream)
            {
                Money presentValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);
                if (baseParty == stream.payerPartyReference.href)
                {
                    list.Add(presentValueOfStream);
                }
            }
            Money payFutureValueOfAdditionalPayments = GetPayFutureValueOfAdditionalPayments(swap, baseParty);
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