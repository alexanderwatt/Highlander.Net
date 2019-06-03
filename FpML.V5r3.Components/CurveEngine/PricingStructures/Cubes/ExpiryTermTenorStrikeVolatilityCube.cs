#region Using directives

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.LinearAlgebra;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.CurveEngine.PricingStructures.Cubes
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class ExpiryTermTenorStrikeVolatilityCube : PricingStructureBase
    {
        #region Properties

        ///<summary>
        /// Gets and sets the algorithm.
        ///</summary>
        public string Algorithm { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="expiryTenors"></param>
        /// <param name="strikes"></param>
        /// <param name="volSurface"></param>
        /// <param name="surfaceId"></param>
        public ExpiryTermTenorStrikeVolatilityCube(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, Double[] strikes, Double[,] volSurface, 
                                        VolatilitySurfaceIdentifier surfaceId)
        {
            Algorithm = surfaceId.Algorithm;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            var points = ProcessRawSurface(expiryTenors, strikes, volSurface);
            PricingStructure = new VolatilityRepresentation { name = surfaceId.Name,
                                                              id = surfaceId.Id,
                                                              asset = new AnyAssetReference { href = "Unknown" },
                                                            };
            var datapoints = new MultiDimensionalPricingData {point = points};
            PricingStructureValuation = new VolatilityMatrix
                                            {
                                                dataPoints = datapoints
                                                ,  objectReference = new AnyAssetReference { href = PricingStructure.id }
                                                ,
                                                baseDate = new IdentifiedDate { Value = surfaceId.BaseDate}
                                                ,  buildDateTime = DateTime.Now
                                                ,  buildDateTimeSpecified = true
                                            };
            var expiries = new double[expiryTenors.Length];
            var index = 0;
            foreach (var term in expiryTenors)//TODO include business day holidays and roll conventions.
            {
                expiries[index] = PeriodHelper.Parse(term).ToYearFraction();
                index++;
            }
            // Generate an interpolator to use
            Interpolator = new VolSurfaceInterpolator(expiries, strikes, new Matrix(volSurface), curveInterpolationMethod, true);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpmlData"></param>
        /// <param name="properties"></param>
        public ExpiryTermTenorStrikeVolatilityCube(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            var surfaceId = new VolatilitySurfaceIdentifier(properties);
            Algorithm = surfaceId.Algorithm;
            PricingStructureIdentifier = surfaceId;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            Algorithm = "Linear"; //Add as a propert in the id.
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            var data = (VolatilityMatrix)fpmlData.Second;
            DateTime baseDate = data.baseDate.Value;
            Interpolator = new VolSurfaceInterpolator(data.dataPoints, curveInterpolationMethod, true, baseDate);
            SetFpMLData(fpmlData);
        }

        #endregion

        #region IPricingStructure Members

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        protected void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        /// <summary>
        /// Gets the VolatilityMatrix.
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            return (VolatilityMatrix)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the VolatilityRepresentation.
        /// </summary>
        /// <returns></returns>
        public VolatilityRepresentation GetVolatilityRepresentation()
        {
            return (VolatilityRepresentation)PricingStructure;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public override IValue GetValue(IPoint pt)
        {
            return new DoubleValue("dummy", Value(pt), pt);
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
//            var result =Interpolator.GetDiscreteSpace().GetClosestValues(pt);

            //var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            //var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));

            IList<IValue> result = new List<IValue>();

            return result;
        }

        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        #endregion

        #region IPointFunction Members

        /// <summary>
        /// This method uses the supplied Interpolation function to provide a value for a given point
        /// </summary>
        /// <param name="point">A point to get a value for</param>
        /// <returns></returns>
        public override double Value(IPoint point)
        {
            return Interpolator.Value(point);
        }

        #endregion

        /// <summary>
        /// Generate an array of PricingStructurePoint from a set of input arrays
        /// The array can then be added to the Matrix
        /// </summary>
        /// <param name="expiry">Expiry values to use</param>
        /// <param name="strike">Strike values to use</param>
        /// <param name="volatility">An array of volatility values</param>
        /// <returns></returns>
        private static PricingStructurePoint[] ProcessRawSurface(String[] expiry, Double[] strike, double[,] volatility)
        {
            var expiryLength = expiry.Length;
            var strikeLength = strike.Length;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength * strikeLength];
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                for (var strikeIndex = 0; strikeIndex < strikeLength; strikeIndex++)
                {
                    // Extract the row,column indexed volatility
                    var vol = (decimal)volatility[expiryIndex, strikeIndex];

                    // Add the value to the points array (dataPoints entry in the matrix)
                    var coordinates = new PricingDataPointCoordinate[1];
                    coordinates[0] = PricingDataPointCoordinateFactory.Create(expiry[expiryIndex], null, (Decimal)strike[strikeIndex]);
                    var pt = new PricingStructurePoint { value = vol, valueSpecified = true, coordinate = coordinates };
                    points[pointIndex++] = pt;
                }
            }
            return points;
        }

    }
}