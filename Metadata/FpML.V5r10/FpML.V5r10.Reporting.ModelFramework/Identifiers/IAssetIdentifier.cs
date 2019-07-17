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
using Orion.Constants;

namespace FpML.V5r10.Reporting.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IAssetIdentifier : IIdentifier
    {
        /// <summary>
        /// BaseDate
        /// </summary>
        DateTime BaseDate { get; set; }

        /// <summary>
        /// MaturityDate
        /// </summary>
        DateTime? MaturityDate { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        AssetTypesEnum AssetType { get; set; }

        ///<summary>
        /// The Coupon.
        ///</summary>
        Decimal? Coupon { get; set; }

        /// <summary>
        /// THe market rate
        /// </summary>
        Decimal? MarketQuote { get; set; }

        /// <summary>
        /// THe spread
        /// </summary>
        Decimal? Other { get; set; }

        /// <summary>
        /// THe Strike
        /// </summary>
        Decimal? Strike { get; set; }
    }
}