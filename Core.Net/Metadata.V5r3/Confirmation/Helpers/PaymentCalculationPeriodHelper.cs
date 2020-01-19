#region Using directives

using System;

#endregion

namespace FpML.V5r3.Confirmation
{
    public static class PaymentCalculationPeriodHelper
    {
        #region Get/SetNotionalAmount

        public static decimal GetNotionalAmount(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            decimal result = 0.0m;

            decimal numberOfPeriods = 0.0m;

            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                result += XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriod);
                numberOfPeriods += 1;
            }

            return result / numberOfPeriods;
        }

        public static void SetNotionalAmount(PaymentCalculationPeriod paymentCalculationPeriod, decimal notionalAmount)
        {
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                XsdClassesFieldResolver.CalculationPeriodSetNotionalAmount(calculationPeriod, notionalAmount);
            }
        }

        #endregion

        #region Get/SetRate

        public static decimal GetRate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            decimal result = 0.0m;
            decimal numberOfPeriods = 0.0m;
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriodHasFixedRate(calculationPeriod))
                {
                    result += XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriod);
                }
                else if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    result += XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod).calculatedRate;
                }
                else
                {
                    throw new NotImplementedException("PaymentCalculationPeriodHelper.GetRate");
                }               
                numberOfPeriods += 1;
            }
            return result / numberOfPeriods;
        }
        
        public static void SetRate(PaymentCalculationPeriod paymentCalculationPeriod, decimal rate)
        {
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriodHasFixedRate(calculationPeriod))
                {
                    XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriod, rate);
                }
                else if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    throw new NotImplementedException("Cannot modify floating rate, PaymentCalculationPeriodHelper.SetRate");
                    //XsdClassesFieldResolver.CalculationPeriod_GetFloatingRateDefinition(calculationPeriod).calculatedRate;
                }
                else
                {
                    throw new NotImplementedException("PaymentCalculationPeriodHelper.SetRate");
                }
            }
        }
        public static void ReplaceFloatingRateWithFixedRate(PaymentCalculationPeriod paymentCalculationPeriod, decimal fixedRate)
        {
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriodHasFixedRate(calculationPeriod))
                {
                    throw new System.Exception("calculation period already uses a fixed rate.");
                }
                if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    // Replace FloatingRateDefinition with decimal (fixed rate)
                    //
                    XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriod, fixedRate);
                }
                else
                {
                    throw new NotSupportedException("PaymentCalculationPeriodHelper.ReplaceFloatingRateWithFixedRate");
                }
            }
        }

        #endregion

        #region Get/SetSpread

        public static decimal GetSpread(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            decimal result = 0.0m;
            decimal numberOfPeriods = 0.0m;
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                    result += floatingRateDefinition.spread;
                }
                else
                {
                    throw new NotImplementedException("PaymentCalculationPeriodHelper.GetSpread cannot be called on fixed rate cashflow.");
                }
                numberOfPeriods += 1;
            }
            return result / numberOfPeriods;
        }

        public static void SetSpread(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                    floatingRateDefinition.spread = value;
                    floatingRateDefinition.spreadSpecified = true;
                }
                else
                {
                    throw new NotImplementedException("PaymentCalculationPeriodHelper.SetSpread cannot be called on a fixed rate cashflow.");
                }
            }
        }

        #endregion


        public static   CalculationPeriod[] GetCalculationPeriods(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
        }

        public static   CalculationPeriod GetFirstCalculationPeriod(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod)[0];
        }


        public static int GetNumberOfDays(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            int result = 0;
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                result += int.Parse(calculationPeriod.calculationPeriodNumberOfDays);
            }           
            return result;
        }

        public static DateTime GetCalculationPeriodStartDate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            return calculationPeriodArray[0].adjustedStartDate;
        }

        public static void SetCalculationPeriodStartDate(PaymentCalculationPeriod paymentCalculationPeriod, DateTime startDate)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            calculationPeriodArray[0].adjustedStartDate = startDate;
            calculationPeriodArray[0].adjustedStartDateSpecified = true;

        }

        public static DateTime GetCalculationPeriodEndDate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            return calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDate;
        }

        public static void SetCalculationPeriodEndDate(PaymentCalculationPeriod paymentCalculationPeriod, DateTime endDate)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDate = endDate;
            calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDateSpecified = true;
        }
    }
}