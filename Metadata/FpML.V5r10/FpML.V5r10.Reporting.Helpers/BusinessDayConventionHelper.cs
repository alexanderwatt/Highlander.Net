using Orion.Util.Helpers;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class BusinessDayConventionHelper
    {
        public static BusinessDayConventionEnum Parse(string s)
        {
            return EnumHelper.Parse<BusinessDayConventionEnum>(s);
        }
    }
}