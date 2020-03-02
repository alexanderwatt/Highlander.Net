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

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IStrikeVolatilitySurface : IVolatilitySurface
    {
        /// <summary>
        /// Gets the volatility using a DateTime expiry and a strike value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expirationAsDate">The expiration date.</param>
        /// <param name="strike">The strike required.</param>
        /// <returns>The interpolated value.</returns>
        Double GetValueByExpiryDateAndStrike(DateTime baseDate, DateTime expirationAsDate, double strike);

        /// <summary>
        /// Gets the volatility using a term expiry and a strike value.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        Double GetValueByExpiryTermAndStrike(String term, double strike);
    }
}