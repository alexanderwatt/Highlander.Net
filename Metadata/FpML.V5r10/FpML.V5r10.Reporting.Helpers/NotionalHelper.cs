/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class NotionalHelper
    {
        public static Decimal  GetNotionalValue(Notional notional, DateTime dateTime)
        {
            if (null == notional.notionalStepSchedule.step)
            {
                return notional.notionalStepSchedule.initialValue;
            }
            // I assume that the dates in a step schedule are in the ascensing order.
            //
            NonNegativeAmountSchedule notionalStepSchedule = notional.notionalStepSchedule;
            var listOfStepDates = new ArrayList();
            var listOfStepValues = new ArrayList();
            foreach (var step in notionalStepSchedule.step)
            {
                listOfStepDates.Add(step.stepDate);
                listOfStepValues.Add(step.stepValue);
            }
            int foundAtIndex = listOfStepDates.BinarySearch(dateTime);
            if (foundAtIndex < 0)
            {
                int indexOfLargeDate = ~foundAtIndex;
                if (0 == indexOfLargeDate)
                {
                    return notional.notionalStepSchedule.initialValue;
                }
                var val = (decimal)listOfStepValues[indexOfLargeDate - 1];
                return val;
            }
            return (decimal)listOfStepValues[foundAtIndex];
        }
    }
}
