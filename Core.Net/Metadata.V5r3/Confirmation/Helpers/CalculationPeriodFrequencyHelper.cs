#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class CalculationPeriodFrequencyHelper
    {

        /// <summary>
        /// Parses the specified interval as string.
        /// </summary>
        /// <param name="intervalAsString"><example>3M,3m,14d,6Y</example></param>
        /// <param name="rollConventionAsString">The roll convention.</param>
        /// <returns></returns>
        public  static CalculationPeriodFrequency Parse(string intervalAsString, string rollConventionAsString)
        {
            var result = new CalculationPeriodFrequency();
            Period interval = PeriodHelper.Parse(intervalAsString);
            result.periodMultiplier = interval.periodMultiplier;
            result.period = interval.period.ToString();
            result.rollConvention = RollConventionEnumHelper.Parse(rollConventionAsString);
            
            return result;
        }

    }
}