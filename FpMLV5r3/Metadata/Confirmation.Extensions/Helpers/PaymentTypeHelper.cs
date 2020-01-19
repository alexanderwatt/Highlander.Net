using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
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