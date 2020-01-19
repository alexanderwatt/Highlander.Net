using System;

namespace FpML.V5r10.Codes
{
    public static class EnumParse
    {
        public static DayCountFractionEnum ToDayCountFractionEnum(string dayCountFractionAsString)
        {
            dayCountFractionAsString = dayCountFractionAsString.Replace("ActualActual", "ACT/ACT.");
            DayCountFractionEnum dayCountFractionEnum;
            if (!DayCountFractionScheme.TryParseEnumString(dayCountFractionAsString, out dayCountFractionEnum))
            {
                // Try custom values if standard enum names fail
                switch (dayCountFractionAsString)
                {
                    case "OneOne":
                        dayCountFractionEnum = DayCountFractionEnum._1_1;
                        break;
                    case "Actual365":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_365_FIXED;
                        break;
                    case "Actual360":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_360;
                        break;
                    case "Thirty360EU":
                        dayCountFractionEnum = DayCountFractionEnum._30E_360;
                        break;
                    case "Thirty360US":
                        dayCountFractionEnum = DayCountFractionEnum._30_360;
                        break;
                    case "Business252":
                    case "ACT/252.FIXED":
                        dayCountFractionEnum = DayCountFractionEnum.BUS_252;
                        break;
                    case "ACT/ACT.AFB":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_AFB;
                        break;
                    case "ACT/ACT.ICMA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ICMA;
                        break;
                    case "ACT/ACT.ISDA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ISDA;
                        break;
                    case "ACT/ACT.ISMA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ISMA;
                        break;
                    default:
                        string message = string.Format("DayCountFraction '{0}' is not valid.", dayCountFractionAsString);
                        throw new ArgumentOutOfRangeException("dayCountFractionAsString", message);
                }
            }
            return dayCountFractionEnum;
        }

        public static CompoundingFrequencyEnum ToCompoundingFrequencyEnum(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(string.Format("CompoundingFrequencyEnum parsing failed for ''"));
            }
            if (value.Equals("Semi-Annual", StringComparison.InvariantCultureIgnoreCase)
                || value.Equals("Semi", StringComparison.InvariantCultureIgnoreCase))
            {
                value = "SemiAnnual";
            }
            CompoundingFrequencyEnum result;
            try
            {
                result = (CompoundingFrequencyEnum)Enum.Parse(typeof(CompoundingFrequencyEnum), value, true);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("CompoundingFrequencyEnum parsing failed for '{0}'", value));
            }
            return result;
        }
    }
}
