namespace FpML.V5r3.Reporting.Helpers
{
    public static class CashflowTypeHelper
    {
        public static CashflowType Create(string value)
        {
            var cashflowType = new CashflowType {Value = value};
            return cashflowType;
        }
    }
}