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

using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;

namespace Highlander.CurveEngine.V5r3.Assets.FX
{
    ///<summary>
    ///</summary>
    public abstract class PriceableFxAssetController : AssetControllerBase, IPriceableFxAssetController
    {
        /// <summary>
        /// The Rate quotation
        /// </summary>
        public BasicQuotation FxRate => MarketQuote;

        /// <summary>
        /// 
        /// </summary>
        public FxRateAsset FxRateAsset { get; protected set; }

        #region IPriceableAssetController Members

        /////<summary>
        /////</summary>
        //public new BasicQuotation MarketQuote
        //{
        //    get { return FxRate; }
        //    set { FxRate = value; }
        //}

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public abstract decimal ForwardAtMaturity { get; }


        #endregion
    }
}