#region Using directives

using System;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Analytics.LinearAlgebra;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// The vol surface interpolator
    /// </summary>
    public class VolSurfaceInterpolator : InterpolatedSurface
    {
        /// <summary>
        /// VolSurfaceInterpolator
        /// </summary>
        /// <param name="discreteCurve"></param>
        /// <param name="interpolation"></param>
        /// <param name="allowExtrapolation"></param>
        public VolSurfaceInterpolator(IDiscreteSpace discreteCurve, InterpolationMethod interpolation, bool allowExtrapolation)
            : base(discreteCurve, InterpolationFactory.Create(interpolation.Value), allowExtrapolation)
        {
        }

        /// <summary>
        /// Builds a raw volatility surface from basic data types.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpolation"></param>
        /// <param name="allowExtrapolation"></param>
        /// <param name="baseDate"></param>
        public VolSurfaceInterpolator(MultiDimensionalPricingData data, InterpolationMethod interpolation, bool allowExtrapolation, DateTime baseDate)
            : base(ProcessMultiDimensionalPricingData(data, baseDate), InterpolationFactory.Create(interpolation.Value), allowExtrapolation)
        {
        }

        /// <summary>
        /// Builds a raw volatility surface from basic data types.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="volData"></param>
        /// <param name="interpolation"></param>
        /// <param name="allowExtrapolation"></param>
        public VolSurfaceInterpolator(double[] rows, double[] columns, Matrix volData, InterpolationMethod interpolation, bool allowExtrapolation)
            : base(new DiscreteSurface(rows, columns, volData), InterpolationFactory.Create(interpolation.Value), allowExtrapolation)
        {
        }

        internal static DiscreteSurface ProcessMultiDimensionalPricingData(MultiDimensionalPricingData data, DateTime baseDate)
        {
            var points = data.point.Select(point => new PricingDataPoint2D(point, baseDate)).Cast<IPoint>().ToList();
            var result = new DiscreteSurface(points);
            return result;
        }
    }
}