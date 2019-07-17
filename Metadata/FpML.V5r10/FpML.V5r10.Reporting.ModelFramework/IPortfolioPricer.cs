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
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Curve id and stress name tuple.
    /// </summary>
    public class CurveStressPair
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string Curve;

        /// <summary>
        /// 
        /// </summary>
        public readonly string Stress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="stress"></param>
        public CurveStressPair(string curve, string stress)
        {
            Curve = curve;
            Stress = stress;
        }
    }

    ///<summary>
    ///</summary>
    public interface IPortfolioPricer
    {
        ///<summary>
        /// Returns the relevant pricing structure ids.
        ///</summary>
        ///<returns></returns>
        IEnumerable<String> GetRequiredPricingStructures();
    }
}