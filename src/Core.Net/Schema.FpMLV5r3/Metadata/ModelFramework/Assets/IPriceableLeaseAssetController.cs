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

using System;

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// The Priceable base lease asset controller
    /// </summary>
    public interface IPriceableLeaseAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the lease price.
        /// </summary>
        /// <returns></returns>
        Decimal Price { get; }

        /// <summary>
        /// Gets the lease starting amount.
        /// </summary>
        /// <returns></returns>
        Decimal StartAmount { get; }

        ///<summary>
        /// Gets the date on which the lease settles.
        ///</summary>
        DateTime SettlementDate { get; set; }

        ///<summary>
        /// Gets the date on which the lease settles.
        ///</summary>
        DateTime MaturityDate { get; set; }

        /// <summary>
        /// Gets or sets the change to base.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal ChangeToBase { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }
    }
}