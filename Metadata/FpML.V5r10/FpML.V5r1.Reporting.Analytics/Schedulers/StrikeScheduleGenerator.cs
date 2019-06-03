#region Using Directives

using System.Collections.Generic;

#endregion

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// The basic strike scheduler.
    /// </summary>
    public  class StrikeScheduleGenerator
    {
        /// <summary>
        /// Does the work in generating the notional schedule.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="step"></param>
        /// <param name="applyStepToEachNthCashflow"></param>
        /// <param name="totalNumberOfCashflows"></param>
        /// <returns></returns>
        public static List<double> GetStrikeScheduleImpl(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var result = new List<double>();

            var cashflowNumber = 0;

            while (totalNumberOfCashflows-- > 0)
            {
                result.Add(initialValue);
                ++cashflowNumber;

                if (cashflowNumber % applyStepToEachNthCashflow == 0)
                {
                    initialValue += step;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the strike schedule.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="step"></param>
        /// <param name="applyStepToEachNthCashflow"></param>
        /// <param name="totalNumberOfCashflows"></param>
        /// <returns></returns>
        public static double[] GetStrikeSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var strikeSchedule = GetStrikeScheduleImpl(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);

            var result = strikeSchedule.ToArray();

            return result;
        }
    }
}