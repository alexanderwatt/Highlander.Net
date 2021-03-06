#region Using directives

using System;
using System.Collections.Generic;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class CapFloorHelper
    {
        public static InterestRateStream GetPayerStream(CapFloor capFloor, string baseParty)
        {
            if (capFloor.capFloorStream.payerPartyReference.href == baseParty)
            {
                return capFloor.capFloorStream;
            }

            throw new Exception(String.Format("No payer stream was found for the specified payer : '{0}'", baseParty));
        }

        public static InterestRateStream GetReceiverStream(CapFloor capFloor, string baseParty)
        {
            if (capFloor.capFloorStream.receiverPartyReference.href == baseParty)
            {
                return capFloor.capFloorStream;
            }

            throw new Exception(String.Format("No receiver stream was found for the specified receiver : '{0}'", baseParty));
        }

        public static Money GetValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            List<Money> list = new List<Money>();

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
            List<Money> list = new List<Money>();

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
            List<Money> valueList = new List<Money>();

            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    if (baseParty == payment.receiverPartyReference.href)
                    {
                        valueList.Add(payment.presentValueAmount);
                    }
                }
            }

            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayPresentValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            List<Money> valueList = new List<Money>();

            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    if (baseParty == payment.payerPartyReference.href)
                    {
                        valueList.Add(payment.presentValueAmount);
                    }
                }
            }

            return MoneyHelper.Sum(valueList);
        }

        public static Money GetReceiveFutureValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            List<Money> valueList = new List<Money>();

            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    if (baseParty == payment.receiverPartyReference.href)
                    {
                        valueList.Add(payment.paymentAmount);
                    }
                }
            }

            return MoneyHelper.Sum(valueList);
        }

        public static Money GetPayFutureValueOfAdditionalPayments(CapFloor capFloor, string baseParty)
        {
            List<Money> valueList = new List<Money>();

            if (null != capFloor.additionalPayment)
            {
                foreach (Payment payment in capFloor.additionalPayment)
                {
                    if (baseParty == payment.payerPartyReference.href)
                    {
                        valueList.Add(payment.paymentAmount);
                    }
                }
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
            List<Money> list = new List<Money>();

            InterestRateStream stream = capFloor.capFloorStream;

            Money presentValueOfStream = CashflowsHelper.GetPresentValue(stream.cashflows);

            list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, presentValueOfStream));

            Money presentValueOfAdditionalPayments = GetPresentValueOfAdditionalPayments(capFloor, baseParty);

            list.Add(presentValueOfAdditionalPayments);

            return MoneyHelper.Sum(list);
        }

        public  static  List<Money>   GetValue(string payer, string receiver, string baseParty, Money amount)
        {
            List<Money> result = new List<Money>();

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
            List<Money> list = new List<Money>();

            InterestRateStream stream = capFloor.capFloorStream;
            Money futureValueOfStream = CashflowsHelper.GetForecastValue(stream.cashflows);
            list.AddRange(GetValue(stream.payerPartyReference.href, stream.receiverPartyReference.href, baseParty, futureValueOfStream));

            Money futureValueOfAdditionalPayments = GetValueOfAdditionalPayments(capFloor, baseParty);

            list.Add(futureValueOfAdditionalPayments);

            return MoneyHelper.Sum(list);
        }

        public static Money GetReceivePresentValue(CapFloor capFloor, string baseParty)
        {
            List<Money> list = new List<Money>();

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
            List<Money> list = new List<Money>();

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
            List<Money> list = new List<Money>();

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
            List<Money> list = new List<Money>();

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