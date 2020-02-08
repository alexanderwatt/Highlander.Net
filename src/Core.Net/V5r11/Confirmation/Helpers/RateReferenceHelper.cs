#region Using Directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public class RateReferenceHelper
    {
        public static RateReference Parse(string href)
        {
            var result = new RateReference {href = href};
            return result;
        }
    }
}
