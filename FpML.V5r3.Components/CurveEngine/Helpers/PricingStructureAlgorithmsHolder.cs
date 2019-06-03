using System;
using System.Linq;
using Core.Common;
using Orion.Constants;
using Orion.Util.Logging;
using Orion.V5r3.Configuration;

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// A PricingStructureAlgorithmsHolder.
    /// </summary>
    public class PricingStructureAlgorithmsHolder
    {
        private readonly Algorithm _pricingStructureAlgorithm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="pricingStructureType"></param>
        /// <param name="algorithmName"></param>
        public PricingStructureAlgorithmsHolder(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureTypeEnum pricingStructureType, string algorithmName)
        {
            if (cache != null)
            {
                _pricingStructureAlgorithm = GetAlgorithm(logger, cache, nameSpace, pricingStructureType, algorithmName);
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetValue(string propertyName)
        {
            var result = _pricingStructureAlgorithm.Properties.Where(a => a.name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            return result.First().Value;
        }


        /// <summary>
        /// Tries to get the algorithm.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="pricingStructureType"></param>
        /// <param name="algorithmName"></param>
        /// <param name="logger"></param>
        /// <param name="cache">The value</param>
        /// <returns>Whether the property existed or not</returns>
        public Algorithm GetAlgorithm(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureTypeEnum pricingStructureType, string algorithmName)
        {
            Algorithm algorithm = null;
            if (cache != null)
            {
                try
                {
                    algorithm = cache.LoadObject<Algorithm>(nameSpace + "." + AlgorithmsProp.GenericName + "." + pricingStructureType + "." + algorithmName);
                }
                catch (Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return algorithm;
        }
    }

    /// <summary>
    /// A PricingStructureAlgorithmsHolder.
    /// </summary>
    public class GenericRateCurveAlgorithmHolder : PricingStructureAlgorithmsHolder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        public GenericRateCurveAlgorithmHolder(ILogger logger, ICoreCache cache, String nameSpace)
            : base(logger, cache, nameSpace, PricingStructureTypeEnum.RateCurve, "FastLinearZero")
        { }
    }
}