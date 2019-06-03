using FpML.V5r3.Codes;

namespace FpML.V5r3.Confirmation
{
    public class DayCountFractionHelper
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