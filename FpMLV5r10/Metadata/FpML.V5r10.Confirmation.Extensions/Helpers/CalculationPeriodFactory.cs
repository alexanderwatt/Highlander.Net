#region Using directives

using System;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static partial class CalculationPeriodFactory
    {
        public static CalculationPeriod Create(DateTime adjustedStartDate, DateTime adjustedEndDate, DateTime unadjustedStartDate, DateTime unadjustedEndDate)
        {
            CalculationPeriod calculationPeriod = new CalculationPeriod();

            calculationPeriod.unadjustedStartDate = unadjustedStartDate;
            calculationPeriod.unadjustedStartDateSpecified = true;

            calculationPeriod.unadjustedEndDate = unadjustedEndDate;
            calculationPeriod.unadjustedEndDateSpecified = true;

            calculationPeriod.adjustedStartDate = adjustedStartDate;
            calculationPeriod.adjustedStartDateSpecified = true;

            calculationPeriod.adjustedEndDate = adjustedEndDate;
            calculationPeriod.adjustedEndDateSpecified = true;


            return calculationPeriod;
        }

    }
}
