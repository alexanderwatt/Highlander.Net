/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;

#endregion

namespace FpML.V5r3.Reporting.Helpers
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