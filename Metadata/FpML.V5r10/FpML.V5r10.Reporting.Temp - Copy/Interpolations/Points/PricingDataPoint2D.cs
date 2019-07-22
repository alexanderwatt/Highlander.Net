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

#region Using Directives

using System;
using System.Globalization;
using FpML.V5r10.Reporting;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolations.Points
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
            : this(expiry, strike.ToString(CultureInfo.InvariantCulture), null, true, 0.0)
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
        public PricingDataPointCoordinate PricingDataPointCoordinate { get; }

        #endregion

        #region Object overrides

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(PricingDataPoint2D))
            {
                var coord = ((PricingDataPoint2D)obj).PricingDataPointCoordinate;
                // expiry
                var match = (PricingDataPointCoordinate.expiration[0].Items[0] as Period)?.Equals(
                                 (Period) coord.expiration[0].Items[0]) ??
                             ((DateTime) PricingDataPointCoordinate.expiration[0].Items[0]).Equals(
                                 (DateTime) coord.expiration[0].Items[0]);
                // tenor
                if (match)
                {
                    if (PricingDataPointCoordinate.term == null && coord.term == null)
                        return true;
                    if (PricingDataPointCoordinate.term != null && coord.term == null)
                        return false;
                    if (PricingDataPointCoordinate.term == null && coord.term != null)
                        return false;
                    if (coord.term != null && (PricingDataPointCoordinate.term != null &&
                                               ((Period) PricingDataPointCoordinate.term[0].Items[0]).Equals(
                                                   (Period) coord.term[0].Items[0])))
                        return true;
                    return false;
                }
                //strike
            }
            return false;
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
