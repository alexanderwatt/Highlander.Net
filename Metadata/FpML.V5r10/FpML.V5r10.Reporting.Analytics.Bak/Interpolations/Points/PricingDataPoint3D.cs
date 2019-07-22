#region Using Directives

using System.Globalization;
using FpML.V5r10.Reporting;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// A class that will model an FPML coordinate. 
    /// A coordinate is a multidimensional point in a volatility surface/solid
    /// </summary>
    public class PricingDataPoint3D : Point3D
    {
        #region Constructors

        /// <summary>
        /// A PricingDataPoint3D.
        /// </summary>
        /// <param name="pt"></param>
        public PricingDataPoint3D(PricingStructurePoint pt)
            : base(PointHelpers.To3DArray(pt))
        {
            PricingDataPointCoordinate = pt.coordinate[0];
        }

        /// <summary>
        /// An FpML constructor.
        /// </summary>
        /// <param name="pt"></param>
        public PricingDataPoint3D(PricingDataPointCoordinate pt)
            : base(PointHelpers.To3DArray(pt))
        {
            PricingDataPointCoordinate = pt;
        }

        /// <summary>
        /// The complete set. We are not going to use the Generic dimension
        /// </summary>
        /// <param name="expiry">An expiry tenor dimension</param>
        /// <param name="term">A term tenor dimension or a strike dimension.</param>
        /// <param name="strike">The strike as a string.</param>
        /// <param name="value">he value of the coordinate</param>
        public PricingDataPoint3D(string expiry, string term, string strike, double value)
            : this(expiry, term, strike, null, value)
        {}

        /// <summary>
        /// The complete set. We are not going to use the Generic dimension
        /// </summary>
        /// <param name="expiry">An expiry tenor dimension</param>
        /// <param name="term">A term tenor dimension or a strike dimension.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="strike">The strike as a string.</param>
        /// <param name="value">he value of the coordinate</param>
        public PricingDataPoint3D(string expiry, string term, string strike, string generic, double value)
            : base(PointHelpers.Create3DArray(expiry, term, strike, generic, value))
        {
            PricingDataPointCoordinate = PricingDataPointCoordinateFactory.Create(expiry, term, strike,
                                                                                                       generic);
        }

        /// <summary>
        /// <example>Swaption Volatility surface coordinate point</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        public PricingDataPoint3D(string expiry, string term, string strike)
            : this(expiry, term, strike, null, 0.0)
        { }

        /// <summary>
        /// <example>Swaption Volatility surface coordinate point</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        public PricingDataPoint3D(string expiry, string term, decimal strike)
            : this(expiry, term, strike.ToString(CultureInfo.InvariantCulture), null, 0.0)
        { }

        #endregion

        #region PricingDataPoint Members

        /// <summary>
        /// A PricingDataPointCoordinate.
        /// </summary>
        public PricingDataPointCoordinate PricingDataPointCoordinate { get; private set; }

        #endregion

        #region Object overrides

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var match = false;
            if (obj.GetType() == typeof(PricingDataPoint3D))
            {
                var coord = ((PricingDataPoint3D)obj).PricingDataPointCoordinate;

                // expiry
                match = ((Period)PricingDataPointCoordinate.expiration[0].Items[0]).Equals((Period)coord.expiration[0].Items[0]);

                // tenor
                if (match)
                {
                    if (PricingDataPointCoordinate.term == null && coord.term == null)
                        return match;
                    if (PricingDataPointCoordinate.term != null && coord.term == null)
                        match = false;
                    else if (PricingDataPointCoordinate.term == null && coord.term != null)
                        match = false;
                    else if (((Period)PricingDataPointCoordinate.term[0].Items[0]).Equals((Period)coord.term[0].Items[0]))
                        return match;
                    else
                        match = false;
                }

                //strike
            }

            return match;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }


}