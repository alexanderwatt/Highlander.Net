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



#endregion

namespace FpML.V5r10.Reporting.Helpers
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