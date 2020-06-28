#region Using directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace nab.QDS.FpML.V47
{
    //PaymentCalculationPeriod

    public static class BusinessCentersHelper
    {
        private const char _businessCenterNameSeparatorCharacters = '-';

        public static BusinessCenters Parse(string businessCentersAsString)
        {
            BusinessCenters result = new BusinessCenters();          
            List<BusinessCenter> list = new List<BusinessCenter>();
            foreach (string businessCenterAsString in businessCentersAsString.Split(_businessCenterNameSeparatorCharacters))
            {
                BusinessCenter businessCenter = new BusinessCenter();
                businessCenter.Value = businessCenterAsString;
                list.Add(businessCenter);
            }
            result.businessCenter = list.ToArray();
            return result;
        }

        public static BusinessCenters Parse(string[] businessCentersAsString)
        {
            BusinessCenters result = new BusinessCenters();
            List<BusinessCenter> list = new List<BusinessCenter>();
            foreach (string businessCenterAsString in businessCentersAsString)
            {
                BusinessCenter businessCenter = new BusinessCenter();
                businessCenter.Value = businessCenterAsString;
                list.Add(businessCenter);
            }
            result.businessCenter = list.ToArray();
            return result;
        }


        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        static public string BusinessCentersString(IEnumerable<BusinessCenter> businessCenters)
        {
            List<string> centers = new List<string>();
            foreach (BusinessCenter businessCenter in businessCenters)
            {
                centers.Add(businessCenter.Value);
            }
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