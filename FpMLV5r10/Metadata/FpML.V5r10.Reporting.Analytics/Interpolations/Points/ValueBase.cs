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

#region Using directives

using System;
using System.Diagnostics;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolations.Points
{
    /// <summary>
    /// The value base class.
    /// </summary>
    public abstract class ValueBase : IValue
    {
        /// <summary>
        /// The value base.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coordinate"></param>
        protected ValueBase(string name, object value, IPoint coordinate)
        {
            Name = name;
            Value = value;
            Coord = coordinate;
        }

        /// <summary>
        /// The name.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// The value.
        /// </summary>
        public virtual object Value { get; }

        /// <summary>
        /// The coordinate.
        /// </summary>
        public virtual IPoint Coord { get; }

    }

    /// <summary>
    /// This class wraps an FpML PricingStructurePoint. it holds a coordinate and a value
    /// </summary>
    public class VolatilityValue : ValueBase
    {
        #region Constructor

        /// <summary>
        /// The volatility value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coord"></param>
        public VolatilityValue(string name, object value, IPoint coord)
            : base(name, value, coord)
        {
            PricePoint = new PricingStructurePoint
            {
                id = name,
                value = Convert.ToDecimal(value),
                valueSpecified = true,
                coordinate = new[] { ((Coordinate)coord).PricingDataCoordinate }
            };
        }

        /// <summary>
        /// The volatility value.
        /// </summary>
        /// <param name="point"></param>
        public VolatilityValue(PricingStructurePoint point)
            : base(point.id, point.value, new Coordinate(point.coordinate[0]))
        {
            PricePoint = point;
        }

        #endregion

        #region Properties

        /// <summary>
        /// A price point.
        /// </summary>
        public PricingStructurePoint PricePoint { get; }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Equals override.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var match = false;
            if (obj.GetType() == typeof(VolatilityValue))
            {
                var pt = ((VolatilityValue)obj).PricePoint;

                match = PricePoint.value == pt.value;

                if (match)
                {
                    match = PricePoint.coordinate[0] == pt.coordinate[0];
                }

                if (match)
                {
                    match = PricePoint.id == pt.id;
                }
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

    /// <summary>
    /// A double value.
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}, Coord = {Coord}")]
    public class DoubleValue : ValueBase
    {
        /// <summary>
        /// A double value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coordinate"></param>
        public DoubleValue(string name, double value, IPoint coordinate)
        :base(name, value, coordinate)
        {
        }
    }
}
