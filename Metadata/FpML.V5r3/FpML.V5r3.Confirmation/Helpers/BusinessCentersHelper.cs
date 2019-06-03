#region Using directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r3.Confirmation
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
        static public string BusinessCentersString(IEnumerable<BusinessCenter> businessCenters)
        {
            List<string> centers = businessCenters.Select(businessCenter => businessCenter.Value).ToList();
            return BusinessCentersString(centers);
        }
        
        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        static public string BusinessCentersString(IEnumerable<string> businessCenters)
        {
            return string.Join("-", businessCenters.ToArray());
        }
    }
}