using FpML.V5r10.Codes;

namespace FpML.V5r10.Reporting
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