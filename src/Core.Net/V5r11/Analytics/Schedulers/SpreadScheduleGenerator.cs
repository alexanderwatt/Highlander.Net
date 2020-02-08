/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;

namespace Highlander.Reporting.Analytics.V5r3.Schedulers
{
    /// <summary>
    /// The basic strike scheduler.
    /// </summary>
    public  class SpreadScheduleGenerator
    {
        /// <summary>
        /// Does the work in generating the notional schedule.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="step"></param>
        /// <param name="applyStepToEachNthCashflow"></param>
        /// <param name="totalNumberOfCashflows"></param>
        /// <returns></returns>
        public static List<double> GetSpreadScheduleImpl(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
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
        public static double[] GetSpreadSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var spreadSchedule = GetSpreadScheduleImpl(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);
            var result = spreadSchedule.ToArray();
            return result;
        }
    }
}