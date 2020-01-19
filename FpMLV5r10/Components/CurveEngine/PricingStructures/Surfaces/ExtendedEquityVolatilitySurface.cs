#region Using directives

using System;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class ExtendedEquityVolatilitySurface : ExtendedExpiryTermStrikeVolatilitySurface
    {
        #region Constructors

        ///// <summary>
        ///// Takes a range of volatilities, an array of tenor expiries and an 
        ///// array of strikes to create a VolatilitySurface
        ///// </summary>
        ///// <param name="instrumentIds">A set of valid instrumentIds</param>
        ///// <param name="expiryTenors">the expiry tenors.</param>
        ///// <param name="strikes">The strikes.</param>
        ///// <param name="values">The values.</param>
        ///// <param name="properties">The properties.</param>
        //public EquityVolatilitySurface(NamedValueSet properties, String[] instrumentIds, String[] expiryTenors, Double[] strikes, Double[,] values)
        //    : base(properties, expiryTenors, strikes, MapFromFxPremiums(instrumentIds, values))//TODO map into the volsurface.
        //{}

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="surfaceId">The id.</param>
        /// <param name="logger"></param>
        /// <param name="cache">The cache.</param>
        /// <param name="forwards">The array of forwards. The first element is the spot value. Conseuently, the length of this array is expiryTenors.Length + 1.</param>
        public ExtendedEquityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors,
                                               Double[] strikes, Double[] forwards, Double[,]
                                                                                        volSurface,
                                               VolatilitySurfaceIdentifier surfaceId)
            : base(logger, cache, nameSpace, expiryTenors, strikes, forwards, volSurface, surfaceId)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Equity);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="logger"></param>
        /// <param name="cache">The cache.</param>
        /// <param name="forwards">The array of forwards. The first element is the spot value. Conseuently, the length of this array is expiryTenors.Length + 1.</param>
        public ExtendedEquityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace,
                                               NamedValueSet properties, String[] expiryTenors, Double[] strikes,
                                               Double[] forwards, Double[,] volSurface)
            : base(logger, cache, nameSpace, properties, expiryTenors, strikes, forwards, volSurface)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Equity);
        }

        ///// <summary>
        ///// Create a surface from an FpML 
        ///// </summary>
        ///// <param name="fpmlData"></param>
        //public ExtendedEquityVolatilitySurface(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        //    : this(fpmlData, PricingStructurePropertyHelper.EquityVolatilityMatrix(fpmlData))
        //{}

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="logger"></param>
        /// <param name="cache">The cache.</param>
        /// <param name="properties">The properties.</param>
        public ExtendedEquityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace,
                                               Pair<PricingStructure, PricingStructureValuation> fpmlData,
                                               NamedValueSet properties)
            : base(logger, cache, nameSpace, fpmlData, properties)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Equity);
        }

        #endregion  

        ///<summary>
        /// Maps from the premium values to implied vols.
        ///</summary>
        ///<param name="instrumentIds">Valid instrument ids.</param>
        ///<param name="values">The matric of values.</param>
        ///<returns></returns>
        public static Double[,] MapFromFxPremiums(String[] instrumentIds, Double[,] values)
        {
            return values;
        }
    }
}