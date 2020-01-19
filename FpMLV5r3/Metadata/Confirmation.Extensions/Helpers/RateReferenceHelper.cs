#region Using Directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public class RateReferenceHelper
    {
        public static RateReference Parse(string href)
        {
            RateReference result = new RateReference();
            result.href = href;

            return result;
        }
    }
}
