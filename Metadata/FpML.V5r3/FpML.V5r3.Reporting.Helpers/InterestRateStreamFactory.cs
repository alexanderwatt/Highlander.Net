#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
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
            var resetDates = new ResetDates();
            floatingRateStream.resetDates = resetDates;
            floatingRateStream.resetDates.resetRelativeTo = ResetRelativeToEnum.CalculationPeriodEndDate;//default value                     
            return floatingRateStream;
        }

        private static InterestRateStream CreateGenericStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            var stream = new InterestRateStream();
            var calculationPeriodDates = new CalculationPeriodDates();
            stream.calculationPeriodDates = calculationPeriodDates;
            var paymentDates = new PaymentDates();
            stream.paymentDates = paymentDates;
            paymentDates.payRelativeTo = paymentDateRelativeTo;
            var calculationPeriodAmount = new CalculationPeriodAmount();
            stream.calculationPeriodAmount = calculationPeriodAmount;
            var calculation = new Calculation();
            stream.calculationPeriodAmount.Item = calculation;
            return stream;
        }
    }
}