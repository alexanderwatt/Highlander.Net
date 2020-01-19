namespace FpML.V5r3.Confirmation
{
    public static class CashflowTypeHelper
    {
        public static CashflowType Create(string value)
        {
            var cashflowType = new CashflowType();
            cashflowType.Value = value;
            return cashflowType;
        }
    }
}