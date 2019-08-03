/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Collections;
using System.Diagnostics;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// A class that will model an FPML coordinate. A coordinate is a multidimensional point in a volatility surface/solid
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}")]
    public class Coordinate : IPoint
    {
        #region Constructors

        /// <summary>
        /// The complete set. We are not going to use the Generic dimension
        /// </summary>
        /// <param name="expiry">An expiry tenor dimension</param>
        /// <param name="term">A term tenor dimension</param>
        /// <param name="strike">A strike value dimension</param>
        /// <param name="generic">A generic dimension</param>
        public Coordinate(string expiry, string term, string strike, string generic)
        {
            ContainedPoint = PointHelpers.CreatePoint(expiry, term, strike);
            PricingDataCoordinate = PricingDataPointCoordinateFactory.Create(expiry, term, strike, generic);
        }
   
        /// <summary>
        /// <example>Swaption Volatility Cube coordinate point</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        public Coordinate(string expiry, string term, string strike)
            : this(expiry, term, strike, null)
        { }

        /// <summary>
        /// <example>Cap/Floor Vol surface</example>
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="strike"></param>
        public Coordinate(string expiry, string strike)
            : this(expiry, null, strike, null)
        { }


        /// <summary>
        /// Cube point
        /// </summary>
        /// <param name="expiry">An expiry tenor dimension</param>
        /// <param name="term">A term tenor dimension</param>
        /// <param name="strike">A strike value dimension</param>
        public Coordinate(string expiry, string term, decimal strike)
        {
            ContainedPoint = PointHelpers.CreatePoint(expiry, term, (double)strike);
            PricingDataCoordinate = PricingDataPointCoordinateFactory.Create(expiry, term, strike, null);
        }

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        public Coordinate(PricingDataPointCoordinate pt)
        {
            PricingDataCoordinate = pt;
        }

        #endregion

        #region IPoint Members

        ///<summary>
        ///</summary>
        public Point ContainedPoint { get; }

        ///<summary>
        ///</summary>
        public PricingDataPointCoordinate PricingDataCoordinate { get; }

        public double GetX()
        {
            throw new NotImplementedException();
        }

        public int GetNumDimensions()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve the dimension data for a coordinate
        /// Data is returned as either a (2-tuple) or a (3-tuple) (no 4-tuple unless required)
        /// A 2-tuple result has the expiration, then the strike.
        /// A 3-tuple result has expiration, term and then, the strike.
        /// </summary>
        public IList Coords
        {
            get
            {
                var list = new ArrayList {PricingDataCoordinate.expiration[0].Items[0]};
                if (PricingDataCoordinate.term != null)
                    list.Add(PricingDataCoordinate.term[0].Items[0]);
                list.Add(PricingDataCoordinate.strike[0]);
                return list;
            }
        }

        /// <summary>
        /// This refers to the interpolator - this is not useful?
        /// </summary>
        public double FunctionValue
        {
            get => ContainedPoint.FunctionValue;
            set => ContainedPoint.FunctionValue = value;
        }

        public double[] Pointarray
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        #endregion

        #region Object overrides

        public override bool Equals(object obj)
        {
            var match = false;
            if (obj.GetType() == typeof(Coordinate))
            {
                var coord = ((Coordinate)obj).PricingDataCoordinate;
                // expiry
                match = ((Period)PricingDataCoordinate.expiration[0].Items[0]).Equals((Period)coord.expiration[0].Items[0]);
                // tenor
                if (match)
                {
                    if (PricingDataCoordinate.term == null && coord.term == null)
                        return match;
                    if (PricingDataCoordinate.term != null && coord.term == null)
                        match = false;
                    else if (PricingDataCoordinate.term == null && coord.term != null)
                        match = false;
                    else if (((Period)PricingDataCoordinate.term[0].Items[0]).Equals((Period)coord.term[0].Items[0]))
                        return match;
                    else
                        match = false;
                }
                //strike
            }
            return match;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///                     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        ///                     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This instance is less than <paramref name="obj" />. 
        ///                     Zero 
        ///                     This instance is equal to <paramref name="obj" />. 
        ///                     Greater than zero 
        ///                     This instance is greater than <paramref name="obj" />. 
        /// </returns>
        /// <param name="obj">
        ///                     An object to compare with this instance. 
        ///                 </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. 
        ///                 </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}