using Orion.Util.Helpers;
using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class BusinessDayConventionHelper
    {
        public static BusinessDayConventionEnum Parse(string s)
        {
            return EnumHelper.Parse<BusinessDayConventionEnum>(s);
        }
    }
}