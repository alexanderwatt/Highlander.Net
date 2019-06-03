namespace FpML.V5r10.Reporting.Helpers
{
    public static class PaymentTypeHelper
    {
        public static PaymentType Create(string value)
        {
            var paymentType = new PaymentType {Value = value};
            return paymentType;
        }
    }
}