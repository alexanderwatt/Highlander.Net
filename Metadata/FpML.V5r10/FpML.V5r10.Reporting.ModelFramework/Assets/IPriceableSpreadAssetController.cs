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

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base spread asset controller interface
    /// </summary>
    public interface IPriceableSpreadAssetController2 : IPriceableSpreadAssetController
    {
        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace);

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the market quote of the asset.</returns>
        decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace);
    }

    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableSpreadAssetController : IPriceableAssetController
    {

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        // promised by alex not used.        
        Decimal ValueAtMaturity { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        IList<Decimal> Values { get; }

        /// <summary>
        /// Returns a set of risk dates
        /// </summary>
        /// <returns></returns>
        IList<DateTime> GetRiskDates();
    }
}