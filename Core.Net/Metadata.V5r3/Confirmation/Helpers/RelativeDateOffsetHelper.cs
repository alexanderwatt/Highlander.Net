#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public  static class RelativeDateOffsetHelper
    {
        public static RelativeDateOffset Create(string s, DayTypeEnum dayType,
                                                              string businessDayConventionAsString,
                                                              string businessCentersAsString,
                                                              string dateRelativeTo)
        {
            var result = new RelativeDateOffset();

            Period interval = PeriodHelper.Parse(s);

            result.period = interval.period;
            result.periodMultiplier = interval.periodMultiplier;
            
            
            result.dayType = dayType;
            result.dayTypeSpecified = true;

            result.businessDayConvention = BusinessDayConventionHelper.Parse(businessDayConventionAsString);
            result.businessCenters = BusinessCentersHelper.Parse(businessCentersAsString);
            
            
            var dateReference = new DateReference {href = dateRelativeTo};

            result.dateRelativeTo = dateReference;

            return result;
        }

        public static RelativeDateOffset Create(string s, DayTypeEnum dayType,
                                                              string businessDayConventionAsString,
                                                              BusinessCenters businessCenters,
                                                              string dateRelativeTo)
        {
            var result = new RelativeDateOffset();

            Period interval = PeriodHelper.Parse(s);

            result.period = interval.period;
            result.periodMultiplier = interval.periodMultiplier;


            result.dayType = dayType;
            result.dayTypeSpecified = true;

            result.businessDayConvention = BusinessDayConventionHelper.Parse(businessDayConventionAsString);
            result.businessCenters = businessCenters;


            var dateReference = new DateReference {href = dateRelativeTo};

            result.dateRelativeTo = dateReference;

            return result;
        }
		
    }
}