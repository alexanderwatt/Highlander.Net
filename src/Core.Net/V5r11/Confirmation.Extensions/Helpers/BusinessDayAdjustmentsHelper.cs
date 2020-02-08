#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class BusinessDayAdjustmentsHelper
    {
        public static BusinessDayAdjustments Create(string businessDayConventionAsString, string businessCentersAsString)
        {
            return Create(BusinessDayConventionHelper.Parse(businessDayConventionAsString), businessCentersAsString);
        }

        public static BusinessDayAdjustments Create(BusinessDayConventionEnum businessDayConvention, string businessCentersAsString)
        {
            var result = new BusinessDayAdjustments
                                                {
                                                    businessDayConvention = businessDayConvention,
                                                    businessCenters =
                                                        BusinessCentersHelper.Parse(businessCentersAsString)
                                                };

            return result;
        }

        public static BusinessDayAdjustments Create(BusinessDayConventionEnum businessDayConvention, BusinessCenters businessCenters)
        {
            var result = new BusinessDayAdjustments
                                                {
                                                    businessDayConvention = businessDayConvention,
                                                    businessCenters = businessCenters
                                                };

            return result;
        }
    }

}
