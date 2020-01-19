#region Using directives

using System;
using System.Collections;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class NotionalHelper
    {
        public static Decimal  GetNotionalValue(Notional notional, DateTime dateTime)
        {
            if (null == notional.notionalStepSchedule.step)
            {
                return notional.notionalStepSchedule.initialValue;
            }
            else
            {
                // I assume that the dates in a step schedule are in the ascensing order.
                //
                NonNegativeAmountSchedule notionalStepSchedule = notional.notionalStepSchedule;

                ArrayList listOfStepDates = new ArrayList();
                ArrayList listOfStepValues = new ArrayList();

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
                    else
                    {
                        decimal val = (decimal)listOfStepValues[indexOfLargeDate - 1];

                        return val;
                    }
                }
                else
                {
                    return (decimal)listOfStepValues[foundAtIndex];
                }
            }
        }

    }
}
