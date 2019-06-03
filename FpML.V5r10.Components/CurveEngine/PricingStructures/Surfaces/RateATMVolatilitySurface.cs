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
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class RateATMVolatilitySurface : ExpiryTermTenorATMVolatilitySurface
    {
        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="expiryTenors"></param>
        /// <param name="tenors"></param>
        /// <param name="volSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="baseDate"></param>
        /// <param name="algorithm">The algorithm for interpolation.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, String[] tenors, Double[,] volSurface, VolatilitySurfaceIdentifier surfaceId, DateTime baseDate, 
                                        string algorithm)
            : base(logger, cache, nameSpace, expiryTenors, tenors, volSurface, surfaceId, baseDate, algorithm)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="tenors">The tenors.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] expiryTenors, String[] tenors, Double[,] volSurface)
            : base(logger, cache, nameSpace, properties, expiryTenors, tenors, volSurface)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="instrumentIds">A set of valid instrumentIds</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="tenors">The v.</param>
        /// <param name="values">The values.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] instrumentIds, String[] expiryTenors, String[] tenors, Double[,] values)
            : base(logger, cache, nameSpace, properties, expiryTenors, tenors, MapFromRatePremiums(instrumentIds, values))//TODO map into the volsurface.
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(logger, cache, nameSpace, fpmlData, properties)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        #endregion  

        ///<summary>
        /// Maps from the premium values to implied vols.
        ///</summary>
        ///<param name="instrumentIds">Valid instrument ids.</param>
        ///<param name="values">The matric of values.</param>
        ///<returns></returns>
        public static Double[,] MapFromRatePremiums(String[] instrumentIds, Double[,] values)
        {
            return values;
        }
    }
}