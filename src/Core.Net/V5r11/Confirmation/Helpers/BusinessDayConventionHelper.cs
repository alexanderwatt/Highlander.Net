using Orion.Util.Helpers;

namespace FpML.V5r3.Confirmation
{
    public static class BusinessDayConventionHelper
    {
        public static BusinessDayConventionEnum Parse(string s)
        {
            return EnumHelper.Parse<BusinessDayConventionEnum>(s);
        }
    }
}