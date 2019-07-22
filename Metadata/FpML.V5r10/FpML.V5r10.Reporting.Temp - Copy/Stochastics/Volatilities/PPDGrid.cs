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

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;

namespace FpML.V5r10.Reporting.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// Base class for PointsPerDay (PPD) Grids.
    /// Child classes will need to implement their own internal structure based around the FpML MultiDimensionalPricingData class
    /// </summary>
    [Serializable()]
    public class PPDGrid : MultiDimensionalPricingData
    {
        // Default constructor
        ///<summary>
        ///</summary>
        public PPDGrid()
        {
            VolsPerExpiry = null;
        }

        ///<summary>
        ///</summary>
        public Dictionary<string, Dictionary<string, decimal>> VolsPerExpiry { get; }

        /// <summary>
        /// This method returns the expiries (rows) for this PPD grid
        /// </summary>
        /// <returns></returns>
        public string[] GetExpiries()
        {
            return null;
        }
    }
}