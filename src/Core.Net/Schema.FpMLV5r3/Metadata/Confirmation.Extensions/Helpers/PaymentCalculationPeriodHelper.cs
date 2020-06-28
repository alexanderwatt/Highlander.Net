#region Using directives

using System;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class PaymentCalculationPeriodHelper
    {
        #region Get/SetNotionalAmount

        public static decimal GetNotionalAmount(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            decimal result = 0.0m;

            decimal numberOfPeriods = 0.0m;

            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                result += XsdClassesFieldResolver.CalculationPeriod_GetNotionalAmount(calculationPeriod);
                numberOfPeriods += 1;
            }

            return result / numberOfPeriods;
        }

        public static void SetNotionalAmount(PaymentCalculationPeriod paymentCalculationPeriod, decimal notionalAmount)
        {
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                XsdClassesFieldResolver.CalculationPeriod_SetNotionalAmount(calculationPeriod, notionalAmount);
            }
        }

        #endregion

        #region Get/SetRate

        public static decimal GetRate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            decimal result = 0.0m;
            decimal numberOfPeriods = 0.0m;

            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriod_HasFixedRate(calculationPeriod))
                {
                    result += XsdClassesFieldResolver.CalculationPeriod_GetFixedRate(calculationPeriod);
                }
                else if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
                {
                    result += XsdClassesFieldResolver.CalculationPeriod_GetFloatingRateDefinition(calculationPeriod).calculatedRate;
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
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriod_HasFixedRate(calculationPeriod))
                {
                    XsdClassesFieldResolver.SetCalculationPeriod_FixedRate(calculationPeriod, rate);
                }
                else if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
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
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriod_HasFixedRate(calculationPeriod))
                {
                    throw new Exception("calculation period already uses a fixed rate.");
                }
                else if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
                {
                    // Replace FloatingRateDefinition with decimal (fixed rate)
                    //
                    XsdClassesFieldResolver.SetCalculationPeriod_FixedRate(calculationPeriod, fixedRate);
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

            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
                {
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriod_GetFloatingRateDefinition(calculationPeriod);

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
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
                {
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriod_GetFloatingRateDefinition(calculationPeriod);

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
            return XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod);
        }

        public static   CalculationPeriod GetFirstCalculationPeriod(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod)[0];
        }


        public static int GetNumberOfDays(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            int result = 0;

            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod))
            {
                result += int.Parse(calculationPeriod.calculationPeriodNumberOfDays);
            }
            
            return result;
        }

        public static DateTime GetCalculationPeriodStartDate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod);

            return calculationPeriodArray[0].adjustedStartDate;
        }

        public static void SetCalculationPeriodStartDate(PaymentCalculationPeriod paymentCalculationPeriod, DateTime startDate)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod);

            calculationPeriodArray[0].adjustedStartDate = startDate;
            calculationPeriodArray[0].adjustedStartDateSpecified = true;

        }

        public static DateTime GetCalculationPeriodEndDate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod);

            return calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDate;
        }

        public static void SetCalculationPeriodEndDate(PaymentCalculationPeriod paymentCalculationPeriod, DateTime endDate)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriod_CalculationPeriodArray(paymentCalculationPeriod);

            calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDate = endDate;
            calculationPeriodArray[calculationPeriodArray.Length - 1].adjustedEndDateSpecified = true;
        }
    }
}