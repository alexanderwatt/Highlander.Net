/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// The basic notional scheduler.
    /// </summary>
    public  class NotionalScheduleGenerator
    {
        /// <summary>
        /// Does the work in generating the notional schedule.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="step"></param>
        /// <param name="applyStepToEachNthCashflow"></param>
        /// <param name="totalNumberOfCashflows"></param>
        /// <returns></returns>
        public static List<double> GetNotionalScheduleImpl(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
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
        /// gets the notional schedule.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="step"></param>
        /// <param name="applyStepToEachNthCashflow"></param>
        /// <param name="totalNumberOfCashflows"></param>
        /// <returns></returns>
        public static double[] GetNotionalSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var notionalSchedule = GetNotionalScheduleImpl(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);
            var result = notionalSchedule.ToArray();
            return result;
        }
    }
}