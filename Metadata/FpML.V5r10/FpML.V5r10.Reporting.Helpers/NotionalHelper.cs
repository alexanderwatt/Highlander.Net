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
