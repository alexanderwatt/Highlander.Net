namespace FpML.V5r3.Confirmation
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