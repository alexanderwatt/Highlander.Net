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

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class NonNegativeScheduleHelper
    {
        public static NonNegativeSchedule Create(Decimal initialValue)
        {
            var result = new NonNegativeSchedule {initialValue = initialValue, initialValueSpecified = true};
            return result;
        }

        public static NonNegativeSchedule Create(IList<DateTime> scheduleDates, IList<decimal> scheduleValues)
        {
            var scheduleDatesAndValues = new List<Pair<DateTime, decimal>>();
            for (int i = 0; i < scheduleDates.Count-1; ++i)
            {
                var pair = new Pair<DateTime, decimal>(scheduleDates[i], scheduleValues[i]);
                scheduleDatesAndValues.Add(pair);
            }
            return Create(scheduleDatesAndValues);
        }

        public static NonNegativeSchedule Create(IList<Pair<DateTime, decimal>> scheduleDatesAndValues)
        {
            var result = new NonNegativeSchedule();
            var listOfSteps = new List<NonNegativeStep>();
            for (int i = 0; i < scheduleDatesAndValues.Count; ++i)
            {
                Pair<DateTime, decimal> pair = scheduleDatesAndValues[i];
                if (i == 0) //create a Schedule from the initial value
                    result = Create(pair.Second);
                else
                    listOfSteps.Add(new NonNegativeStep { stepDate = pair.First, stepValue = pair.Second, stepDateSpecified = true, stepValueSpecified = true});
            }
            result.step = listOfSteps.ToArray();
            return result;
        }
    }

    public static class ScheduleHelper
    {
        public static Schedule Create(Decimal initialValue)
        {
            var result = new Schedule {initialValue = initialValue, initialValueSpecified = true};
            return result;
        }
        

        public static Schedule Create(List<Pair<DateTime, decimal>> scheduleDatesAndValues)
        {
            var result = new Schedule();           
            var listOfSteps = new List<Step>();
            for (int i = 0; i < scheduleDatesAndValues.Count; ++i)
            {
                Pair<DateTime, decimal> pair = scheduleDatesAndValues[i];
                if (i == 0)//create a Schedule from the initial value
                {
                    result = Create(pair.Second);//don't have to use DateTime component for initial value
                }
                else
                {
                    var step = new Step { stepDate = pair.First, stepValue = pair.Second, stepDateSpecified = true, stepValueSpecified = true };
                    listOfSteps.Add(step);
                }
            }
            result.step = listOfSteps.ToArray();            
            return result;
        }

        public static List<DateTime> GetStepDates(Schedule schedule)
        {
            return schedule.step.Select(step => step.stepDate).ToList();
        }

        public static List<DateTime> GetStepDates(NonNegativeAmountSchedule schedule)
        {
            return schedule.step.Select(step => step.stepDate).ToList();
        }

        public static decimal GetValue(Schedule schedule, DateTime dateTime)
        {
            //TODO: Check if the date elements has ASC are ordered.
            //

            //  If the schedule have no step elements.
            //
            if (null == schedule.step || 0 == schedule.step.Length)
            {
                return schedule.initialValue;
            }
            DateTime dateOfFirstStep = schedule.step[0].stepDate;
            //  If a specified datetime happens to be before the first step. 
            //  
            if (dateTime < dateOfFirstStep)
            {
                return schedule.initialValue;
            }
            decimal returnValue = schedule.step[0].stepValue;
            foreach (Step step in schedule.step)
            {
                if (dateTime >= step.stepDate)
                {
                    returnValue = step.stepValue;
                }
            }
            return returnValue;
        }


        public static decimal GetValue(NonNegativeAmountSchedule schedule, DateTime dateTime)
        {
            // override of above
            if (null == schedule.step || 0 == schedule.step.Length)
                return schedule.initialValue;
            DateTime dateOfFirstStep = schedule.step[0].stepDate;
            if (dateTime < dateOfFirstStep)
                return schedule.initialValue;
            decimal returnValue = schedule.step[0].stepValue;
            foreach (var step in schedule.step)
            {
                if (dateTime >= step.stepDate)
                    returnValue = step.stepValue;
            }
            return returnValue;
        }
    }
}