
using System;
using nab.QDS.FpML.Codes;

namespace nab.QDS.FpML.V47
{
    public partial class DayCountFraction
    {
        public static DayCountFraction Parse(string dayCountFractionAsString)
        {
            DayCountFractionEnum dayCountFractionEnum = EnumParse.ToDayCountFractionEnum(dayCountFractionAsString);
            return ToDayCountFraction(dayCountFractionEnum);
        }

        public static DayCountFraction ToDayCountFraction(DayCountFractionEnum dayCountFractionEnum)
        {
            string dayCountFractionString = DayCountFractionScheme.GetEnumString(dayCountFractionEnum);
            var result = new DayCountFraction { Value = dayCountFractionString };

            return result;
        }
    }
}