#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class ResetFrequencyHelper
    {

        /// <summary>
        /// Parses the specified interval as string.
        /// </summary>
        /// <param name="intervalAsString"><example>3M,3m,14d,6Y</example></param>
        /// <returns></returns>
        public static ResetFrequency Parse(string intervalAsString)
        {
            ResetFrequency result = new ResetFrequency();

            Period interval = PeriodHelper.Parse(intervalAsString);

            result.periodMultiplier = interval.periodMultiplier;
            result.period = interval.period.ToString();
           
            return result;
        }
    }
}