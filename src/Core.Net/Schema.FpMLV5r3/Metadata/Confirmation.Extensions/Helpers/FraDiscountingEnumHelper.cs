#region Using directives

using System;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class FraDiscountingEnumHelper
    {
        public static FraDiscountingEnum Parse(string s)
        {
            return (FraDiscountingEnum)Enum.Parse(typeof (FraDiscountingEnum), s, true);
        }
    }
}