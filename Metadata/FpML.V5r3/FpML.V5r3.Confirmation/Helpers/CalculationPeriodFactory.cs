#region Using directives

using System;

#endregion

namespace FpML.V5r3.Confirmation
{
    public static partial class CalculationPeriodFactory
    {
        public static CalculationPeriod Create(DateTime adjustedStartDate, DateTime adjustedEndDate, DateTime unadjustedStartDate, DateTime unadjustedEndDate)
        {
            var calculationPeriod = new CalculationPeriod
                {
                    unadjustedStartDate = unadjustedStartDate,
                    unadjustedStartDateSpecified = true,
                    unadjustedEndDate = unadjustedEndDate,
                    unadjustedEndDateSpecified = true,
                    adjustedStartDate = adjustedStartDate,
                    adjustedStartDateSpecified = true,
                    adjustedEndDate = adjustedEndDate,
                    adjustedEndDateSpecified = true
                };


            return calculationPeriod;
        }

    }
}
