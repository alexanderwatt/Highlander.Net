#region Using directives

using System;
using System.Collections.Generic;
using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    //PaymentCalculationPeriod

    public static class BusinessCenterTimeHelper
    {
        public static BusinessCenterTime Parse(string businessCenterAsString, DateTime hourMinuteTime)
        {
            var result = new BusinessCenterTime
                             {
                                 hourMinuteTime = hourMinuteTime,
                                 businessCenter = new BusinessCenter
                                                      {
                                                          Value =
                                                              businessCenterAsString
                                                      }
                             };

            return result;
        }

        public static BusinessCenterTime Parse(string businessCenterAsString)
        {
            var result = new BusinessCenterTime
            {
                businessCenter = new BusinessCenter
                {
                    Value =
                        businessCenterAsString
                }
            };

            return result;
        }

        public static BusinessCenterTime Create(DateTime hourMinuteTime)
        {
            var result = new BusinessCenterTime {hourMinuteTime = hourMinuteTime};

            return result;
        }
    }
}