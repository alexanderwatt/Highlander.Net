#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class FloatingRateIndexHelper
    {
        public static FloatingRateIndex Parse(string s)
        {
            var result = new FloatingRateIndex {Value = s};
            return result;
        }
    }
}