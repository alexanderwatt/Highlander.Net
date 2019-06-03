#region Using Directives

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.Analytics.Interpolations.Points
{   
    /// <summary>
    /// A class that will model an FPML coordinate. 
    /// A coordinate is a multidimensional point in a volatility surface/solid.
    /// </summary>
    public class PricingDataPoint2D : Point2D
    {
        #region Constructors

        /// <summary>
        /// An FpML constructor.
        /// </summary>
        /// <param name="pt"></param>
        public PricingDataPoint2D(PricingStructurePoint pt)
            : base(PointHelpers.To2DArray(pt))
        {
            PricingDataPointCoordinate = pt.coordinate[0];
        }

        /// <summary>
        /// An FpML constructor.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="baseDate"></param>
        public PricingDataPoint2D(PricingStructurePoint pt, DateTime? baseDate)
            : base(PointHelpers.To2DArray(pt, baseDate))
        {
            PricingDataPointCoordinate = pt.coordinate[0];
        }

        /// <summary>
        /// An FpML constructor.
        /// </summary>
        /// <param name="pt"></param>
        public PricingDataPoint2D(PricingDataPointCoordinate pt)
            : base(PointHelpers.To2DArray(pt))
        {
            PricingDataPointCoordinate = pt;
        }

        /// <summary>
        /// The complete set. We are not going to use the Generic dimension
        /// </summary>
        /// <param name="expiry">An expiry tenor dimension</param>
        /// <param name="termStrike">A term tenor dimension or a strike dimension.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="stikeFlag">A flag which identifies whether the second dimension is strike or term.</param>
        /// <param name="value">he value of the coordinate</param>
        protected PricingDataPoint2D(string expiry, string termStrike, string generic, Boolean stikeFlag, double value)
            : base(PointHelpers.Create2DArray(expiry, termStrike, generic, stikeFlag, value))
        {
            PricingDataPointCoordinate = stikeFlag ? PricingDataPointCoordinateFactory.Create(expiry, null, termStrike,
                                                                                                       generic) : PricingDataPointCoordinateFactory.Create(expiry, termStrike, null,
                                                                                                                                                           generic);
        }

        /// <summary>
        /// <example>Swaption Volatility surface coordinate point</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        public PricingDataPoint2D(string expiry, string term)
            : this(expiry, term, null, false, 0.0)
        { }

        /// <summary>
        /// <example>Swaption Volatility surface coordinate point</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="strike"></param>
        public PricingDataPoint2D(string expiry, decimal strike)
            : this(expiry, strike.ToString(), null, true, 0.0)
        { }

        ///// <summary>
        ///// <example>Cap/Floor Vol surface</example>
        ///// </summary>
        ///// <param name="expiry"></param>
        ///// <param name="strike"></param>
        //public Coordinate(string expiry, string strike)
        //    : this(expiry, null, strike, null, true)
        //{ }

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
            bool match = false;
            if (obj.GetType() == typeof(PricingDataPoint2D))
            {
                var coord = ((PricingDataPoint2D)obj).PricingDataPointCoordinate;

                // expiry
                if (PricingDataPointCoordinate.expiration[0].Items[0] is Period)
                {
                    match = ((Period)PricingDataPointCoordinate.expiration[0].Items[0]).Equals((Period)coord.expiration[0].Items[0]);    
                }
                else
                {
                    match = ((DateTime)PricingDataPointCoordinate.expiration[0].Items[0]).Equals((DateTime)coord.expiration[0].Items[0]);    
                }

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
