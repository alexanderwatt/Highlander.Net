#region Using Directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public class PartyReferenceHelper
    {
        public static PartyReference Parse(string href)
        {
            var result = new PartyReference {href = href};

            return result;
        }
    }
}
