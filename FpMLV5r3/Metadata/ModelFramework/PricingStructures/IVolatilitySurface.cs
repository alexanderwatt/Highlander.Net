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

#region Using directives

#endregion

namespace Highlander.Reporting.ModelFramework.V5r3.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IVolatilitySurface : IPricingStructure
    {
        /// <summary>
        /// Returns the (interpolated or not - depends on the implementation) value of the volatility.
        /// </summary>
        double GetValue(double dimension1, double dimension2);

        ///<summary>
        /// Returns the underlying surface data
        ///</summary>
        ///<returns></returns>
        object[,] GetSurface();
    }
}