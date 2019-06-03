#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public  static class RelativeDateOffsetHelper
    {
        public static RelativeDateOffset Create(string period, DayTypeEnum dayType,
                                                              string businessDayConventionAsString,
                                                              string businessCentersAsString,
                                                              string dateRelativeTo)
        {
            var result = new RelativeDateOffset();
            Period interval = PeriodHelper.Parse(period);
            result.period = interval.period;
            result.periodSpecified = true;
            result.periodMultiplier = interval.periodMultiplier; 
            result.dayType = dayType;
            result.dayTypeSpecified = true;
            if (businessDayConventionAsString != null)
            {
                result.businessDayConvention = BusinessDayConventionHelper.Parse(businessDayConventionAsString);
                result.businessDayConventionSpecified = true;
            }
            if (businessCentersAsString != null)
            {
                result.businessCenters = BusinessCentersHelper.Parse(businessCentersAsString);
            } 
            if (dateRelativeTo != null)
            {
                var dateReference = new DateReference {href = dateRelativeTo};
                result.dateRelativeTo = dateReference;
            }           
            return result;
        }

        public static RelativeDateOffset Create(string period, DayTypeEnum dayType,
                                                              string businessDayConventionAsString,
                                                              BusinessCenters businessCenters,
                                                              string dateRelativeTo)
        {
            var result = new RelativeDateOffset();
            Period interval = PeriodHelper.Parse(period);
            result.period = interval.period;
            result.periodSpecified = true;
            result.periodMultiplier = interval.periodMultiplier;
            result.dayType = dayType;
            result.dayTypeSpecified = true;
            if (businessDayConventionAsString != null)
            {
                result.businessDayConvention = BusinessDayConventionHelper.Parse(businessDayConventionAsString);
                result.businessDayConventionSpecified = true;
            }
            result.businessCenters = businessCenters;
            if (dateRelativeTo != null)
            {
                var dateReference = new DateReference { href = dateRelativeTo };
                result.dateRelativeTo = dateReference;
            }
            return result;
        }		
    }
}