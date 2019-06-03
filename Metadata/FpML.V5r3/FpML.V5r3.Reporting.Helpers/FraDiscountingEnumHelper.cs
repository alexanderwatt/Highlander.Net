#region Using directives

using System;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class FraDiscountingEnumHelper
    {
        public static FraDiscountingEnum Parse(string s)
        {
            return (FraDiscountingEnum)Enum.Parse(typeof (FraDiscountingEnum), s, true);
        }
    }
}