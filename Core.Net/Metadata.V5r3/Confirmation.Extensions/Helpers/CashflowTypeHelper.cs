using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class CashflowTypeHelper
    {
        public static CashflowType Create(string value)
        {
            CashflowType cashflowType = new CashflowType();
            cashflowType.Value = value;

            return cashflowType;
        }
    }
}