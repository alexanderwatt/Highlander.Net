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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Spreads
{
    ///<summary>
    ///</summary>
    public abstract class PriceableRateSpreadAssetController : PriceableRateAssetController, IPriceableRateSpreadAssetController
    {
        #region IPriceableAssetController Members

        /// <summary>
        /// The spread quotation
        /// </summary>
        public BasicQuotation Spread => MarketQuote;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public abstract IList<decimal> Values { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public abstract decimal ValueAtMaturity { get; }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public abstract IList<DateTime> GetRiskDates();

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public abstract decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace);

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the market quote of the asset.</returns>
        public abstract decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace);

        #endregion
    }
}