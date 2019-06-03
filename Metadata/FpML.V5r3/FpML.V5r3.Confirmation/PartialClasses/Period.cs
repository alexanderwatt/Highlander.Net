using System;

namespace FpML.V5r3.Confirmation
{
    public partial class Period
    {
        //private static readonly Calendar Calendar = CultureInfo.CurrentCulture.Calendar;

        #region Object Methods

        //public double ToYearFraction()
        //{
        //    switch (period)
        //    {
        //        case PeriodEnum.D:
        //            return double.Parse(periodMultiplier) / 365d;
        //        case PeriodEnum.W:
        //            return double.Parse(periodMultiplier) / 52d;
        //        case PeriodEnum.M:
        //            return double.Parse(periodMultiplier) / 12d;
        //        case PeriodEnum.Y:
        //            return double.Parse(periodMultiplier);
        //        default:
        //            throw new InvalidOperationException(string.Format("PeriodEnum '{0}' is not supported in this function", period));
        //    }
        //}

        public new string ToString()
        {
            string s = String.Format("{0}{1}", periodMultiplier, period);

            return s;
        }

        public bool Equals(Period periodToCompare)
        {
            if (periodToCompare == null)
                return false;

            bool result
                = (period == periodToCompare.period
                   && double.Parse(periodMultiplier) == double.Parse(periodToCompare.periodMultiplier));

            return result;
        }

        //public Frequency ToFrequency()
        //{
        //    var result
        //        = new Frequency
        //              {
        //                  period = period.ToString(),
        //                  periodMultiplier = periodMultiplier
        //              };
        //    return result;
        //}

        //public Period Sum(Period periodToAdd)
        //{
        //    if (periodToAdd == null)
        //    {
        //        throw new ArgumentNullException("periodToAdd");
        //    }

        //    if (period != periodToAdd.period)
        //    {
        //        string message = String.Format("Intervals must be of the same period type and they are not. Interval1 : {0}, interval2 : {1}", ToString(), periodToAdd.ToString());

        //        throw new Exception(message);
        //    }

        //    string newPeriodMultiplier = (GetPeriodMultiplier() + periodToAdd.GetPeriodMultiplier()).ToString(CultureInfo.InvariantCulture);
        //    var sum
        //        = new Period
        //        {
        //            period = period,
        //            periodMultiplier = newPeriodMultiplier
        //        };

        //    return sum;
        //}

        //public DateTime Add(DateTime dateTime)
        //{
        //    int periodMultiplierAsInt = GetPeriodMultiplier();

        //    switch (period)
        //    {
        //        case PeriodEnum.D:
        //            return Calendar.AddDays(dateTime, periodMultiplierAsInt);
        //        case PeriodEnum.W:
        //            return Calendar.AddWeeks(dateTime, periodMultiplierAsInt);
        //        case PeriodEnum.M:
        //            return Calendar.AddMonths(dateTime, periodMultiplierAsInt);
        //        case PeriodEnum.Y:
        //            return Calendar.AddYears(dateTime, periodMultiplierAsInt);
        //        default:
        //            throw new ArgumentException(string.Format("PeriodEnum '{0}' is not supported in this function", period));
        //    }
        //}

        //public int GetPeriodMultiplier()
        //{
        //    return int.Parse(periodMultiplier);
        //}

        //public DateTime Subtract(DateTime dateToStartAt)
        //{
        //    return Negative().Add(dateToStartAt);
        //}

        //public Period Subtract(Period periodToSubtract)
        //{
        //    if (periodToSubtract == null)
        //    {
        //        throw new ArgumentNullException("periodToSubtract");
        //    }
        //    if (period != periodToSubtract.period)
        //    {
        //        string message = String.Format("Periods must be of the same period type. This period '{0}', period to subtract '{1}'", ToString(), periodToSubtract.ToString());
        //        throw new ArgumentException(message, "periodToSubtract");
        //    }

        //    double multiplier = GetPeriodMultiplier() - periodToSubtract.GetPeriodMultiplier();
        //    var difference
        //        = new Period
        //        {
        //            period = period,
        //            periodMultiplier = multiplier.ToString(CultureInfo.InvariantCulture)
        //        };

        //    return difference;
        //}

        //public Period Negative()
        //{
        //    return Multiply(-1);
        //}

        //public Period Multiply(int multiplier)
        //{
        //    int periodMultiplierAsInt = GetPeriodMultiplier();
        //    Period result = BinarySerializerHelper.Clone(this);
        //    result.periodMultiplier = (multiplier * periodMultiplierAsInt).ToString(CultureInfo.InvariantCulture);
        //    return result;
        //}

        #endregion

        //#region Static Methods

        //private const string AlphaPattern = "[a-zA-Z]+";
        //private const string NumericPattern = "-*[0-9.]+";
        //private static readonly Regex AlphaRegex = new Regex(AlphaPattern, RegexOptions.IgnoreCase);
        //private static readonly Regex NumericRegex = new Regex(NumericPattern, RegexOptions.IgnoreCase);

        ///// <summary>
        ///// Method to split a Period string into its constituent parts and build an Period from them
        ///// </summary>
        ///// <param name="term">The term to convert
        ///// <example>1M, 2W, 1yr, 14day</example>
        ///// </param>
        //public static Period Parse(string term)
        //{
        //    if (string.IsNullOrEmpty(term))
        //    {
        //        throw new ArgumentNullException("term");
        //    }

        //    // Remove all spaces from the string.
        //    string tempLabel = term.Replace(" ", "").ToUpper();

        //    //Filter the strings and map to valid periods.
        //    switch (tempLabel)
        //    {
        //        case "TN":
        //        case "SN":
        //        case "ON":
        //            tempLabel = "1D";
        //            break;
        //        case "SP":
        //            tempLabel = "2D";
        //            break;
        //    }
        //    // Match the alpha and numeric components.
        //    //
        //    MatchCollection alphaMatches = AlphaRegex.Matches(tempLabel);
        //    MatchCollection numericMatches = NumericRegex.Matches(tempLabel);

        //    if ((numericMatches == null || numericMatches.Count == 0) || (alphaMatches == null || alphaMatches.Count == 0))
        //    {
        //        throw new ArgumentException(String.Format("'{0}' string value has not been recognised as interval.", term));
        //    }

        //    var result = new Period
        //    {
        //        periodMultiplier = numericMatches[0].Value,
        //        period = EnumHelper.Parse<PeriodEnum>(alphaMatches[0].Value.Substring(0, 1), true)
        //    };

        //    return result;
        //}

        ///// <summary>
        ///// Tries to parse the period
        ///// </summary>
        //public static bool TryParse(string s, out Period period)
        //{
        //    Boolean result = false;
        //    try
        //    {
        //        period = Parse(s);
        //        result = true;
        //    }
        //    catch
        //    {
        //        period = null;
        //    }
        //    return result;
        //}

        //#endregion
    }
}
