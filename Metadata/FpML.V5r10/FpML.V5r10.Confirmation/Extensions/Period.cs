using System;
using System.Globalization;

namespace FpML.V5r3.Confirmation
{
    public static class FpMLExtensions
    {
        private static readonly Calendar Calendar = CultureInfo.CurrentCulture.Calendar;

        #region Object Methods

        public static double ToYearFraction(this Period thisPeriod)
        {
            switch (thisPeriod.period)
            {
                case PeriodEnum.D:
                    return double.Parse(thisPeriod.periodMultiplier) / 365d;
                case PeriodEnum.W:
                    return double.Parse(thisPeriod.periodMultiplier) / 52d;
                case PeriodEnum.M:
                    return double.Parse(thisPeriod.periodMultiplier) / 12d;
                case PeriodEnum.Y:
                    return double.Parse(thisPeriod.periodMultiplier);
                default:
                    throw new InvalidOperationException(string.Format("PeriodEnum '{0}' is not supported in this function", thisPeriod.period));
            }
        }

        public static Frequency ToFrequency(this Period thisPeriod)
        {
            var result
                = new Frequency
                      {
                          period = thisPeriod.period.ToString(),
                          periodMultiplier = thisPeriod.periodMultiplier
                      }; 
            return result;
        }

        public static Period Sum(this Period thisPeriod, Period periodToAdd)
        {
            if (periodToAdd == null)
            {
                throw new ArgumentNullException("periodToAdd");
            }
            if (thisPeriod.period != periodToAdd.period)
            {
                string message = String.Format("Intervals must be of the same period type and they are not. Interval1 : {0}, interval2 : {1}", thisPeriod, periodToAdd);
                throw new System.Exception(message);
            }
            string newPeriodMultiplier = (thisPeriod.GetPeriodMultiplier() + periodToAdd.GetPeriodMultiplier()).ToString(CultureInfo.InvariantCulture);
            var sum
                = new Period
                {
                    period = thisPeriod.period,
                    periodMultiplier = newPeriodMultiplier
                };
            return sum;
        }

        public static DateTime Add(this Period thisPeriod, DateTime dateTime)
        {
            int periodMultiplierAsInt = thisPeriod.GetPeriodMultiplier();
            switch (thisPeriod.period)
            {
                case PeriodEnum.D:
                    return Calendar.AddDays(dateTime, periodMultiplierAsInt);
                case PeriodEnum.W:
                    return Calendar.AddWeeks(dateTime, periodMultiplierAsInt);
                case PeriodEnum.M:
                    return Calendar.AddMonths(dateTime, periodMultiplierAsInt);
                case PeriodEnum.Y:
                    return Calendar.AddYears(dateTime, periodMultiplierAsInt);
                default:
                    throw new ArgumentException(string.Format("PeriodEnum '{0}' is not supported in this function", thisPeriod.period));
            }
        }

        public static int GetPeriodMultiplier(this Period thisPeriod)
        {
            return int.Parse(thisPeriod.periodMultiplier);
        }

        public static DateTime Subtract(this Period thisPeriod, DateTime dateToStartAt)
        {
            return thisPeriod.Negative().Add(dateToStartAt);
        }

        public static Period Subtract(this Period thisPeriod, Period periodToSubtract)
        {
            if (periodToSubtract == null)
            {
                throw new ArgumentNullException("periodToSubtract");
            }
            if (thisPeriod.period != periodToSubtract.period)
            {
                string message = String.Format("Periods must be of the same period type. This period '{0}', period to subtract '{1}'", thisPeriod, periodToSubtract);
                throw new ArgumentException(message, "periodToSubtract");
            }

            double multiplier = thisPeriod.GetPeriodMultiplier() - periodToSubtract.GetPeriodMultiplier();
            var difference
                = new Period
                {
                    period = thisPeriod.period,
                    periodMultiplier = multiplier.ToString(CultureInfo.InvariantCulture)
                };

            return difference;
        }

        public static Period Negative(this Period thisPeriod)
        {
            return thisPeriod.Multiply(-1);
        }

        public static Period Multiply(this Period thisPeriod, int multiplier)
        {
            int periodMultiplierAsInt = thisPeriod.GetPeriodMultiplier();
            var result = new Period
                             {
                                 period = thisPeriod.period,
                                 periodMultiplier =
                                     (multiplier*periodMultiplierAsInt).ToString(CultureInfo.InvariantCulture)
                             };
            //Period result = BinarySerializerHelper.Clone(thisPeriod);//TODO there is a problem with the binary serializer!
            return result;
        }

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
