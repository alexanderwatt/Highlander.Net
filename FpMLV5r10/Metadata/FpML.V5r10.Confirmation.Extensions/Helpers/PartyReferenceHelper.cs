#region Using Directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public class PartyReferenceHelper
    {
        public static PartyReference Parse(string href)
        {
            PartyReference result = new PartyReference();
            result.href = href;

            return result;
        }
    }
}
