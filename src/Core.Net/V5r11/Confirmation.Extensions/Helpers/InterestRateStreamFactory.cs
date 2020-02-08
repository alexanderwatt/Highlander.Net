#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class InterestRateStreamFactory
    {
        public static InterestRateStream CreateFixedRateStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            InterestRateStream fixedRateStream = CreateGenericStream(paymentDateRelativeTo);           
            return fixedRateStream;
        }

        public static InterestRateStream CreateFloatingRateStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            InterestRateStream floatingRateStream = CreateGenericStream(paymentDateRelativeTo);
            ResetDates resetDates = new ResetDates();
            floatingRateStream.resetDates = resetDates;
            floatingRateStream.resetDates.resetRelativeTo = ResetRelativeToEnum.CalculationPeriodEndDate;//default value                     
            return floatingRateStream;
        }

        private static InterestRateStream CreateGenericStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            InterestRateStream stream = new InterestRateStream();
            CalculationPeriodDates calculationPeriodDates = new CalculationPeriodDates();
            stream.calculationPeriodDates = calculationPeriodDates;
            PaymentDates paymentDates = new PaymentDates();
            stream.paymentDates = paymentDates;
            paymentDates.payRelativeTo = paymentDateRelativeTo;
            CalculationPeriodAmount calculationPeriodAmount = new CalculationPeriodAmount();
            stream.calculationPeriodAmount = calculationPeriodAmount;
            Calculation calculation = new Calculation();
            stream.calculationPeriodAmount.Item = calculation;
            return stream;
        }
    }
}