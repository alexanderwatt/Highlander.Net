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

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    //PaymentCalculationPeriod

    public static class BusinessCentersHelper
    {
        private const char BusinessCenterNameSeparatorCharacters = '-';

        public static BusinessCenters Parse(string businessCentersAsString)
        {
            var result = new BusinessCenters
                {
                    businessCenter =
                        businessCentersAsString.Split(BusinessCenterNameSeparatorCharacters)
                                               .Select(
                                                   businessCenterAsString =>
                                                   new BusinessCenter {Value = businessCenterAsString})
                                               .ToArray()
                };
            return result;
        }

        public static BusinessCenters Parse(string[] businessCentersAsString)
        {
            var result = new BusinessCenters
                {
                    businessCenter =
                        businessCentersAsString.Select(
                            businessCenterAsString => new BusinessCenter {Value = businessCenterAsString}).ToArray()
                };
            return result;
        }


        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        public static string BusinessCentersString(IEnumerable<BusinessCenter> businessCenters)
        {
            List<string> centers = businessCenters.Select(businessCenter => businessCenter.Value).ToList();
            return BusinessCentersString(centers);
        }
        
        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        public static string BusinessCentersString(IEnumerable<string> businessCenters)
        {
            return string.Join("-", businessCenters.ToArray());
        }
    }
}