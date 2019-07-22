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

using FpML.V5r10.Reporting.ModelFramework.Business;

// COM interop attributes

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Italian 30/360 day count convention.
    /// </summary>
    public sealed class Thirty360Italian : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly Thirty360Italian Instance = new Thirty360Italian();

        private Thirty360Italian()
            : base("Thirty360Italian", DayCountConvention.Italian, 360) 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "30E/360";
        }
    }
}