#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class FloatingRateIndexHelper
    {
        public static FloatingRateIndex Parse(string s)
        {
            FloatingRateIndex result = new FloatingRateIndex();
            result.Value = s;

            return result;
        }
    }
}