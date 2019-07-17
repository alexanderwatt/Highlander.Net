﻿/*
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

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    ///<summary>
    ///</summary>
    public interface IPriceableCapFloor<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the seller party reference.
        /// </summary>
        /// <value>The seller party reference.</value>
        string SellerPartyReference { get; }

        /// <summary>
        /// Gets the buyer party reference.
        /// </summary>
        /// <value>The buyer party reference.</value>
        string BuyerPartyReference { get; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <value>The effective date.</value>
        DateTime EffectiveDate { get; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        DateTime TerminationDate { get; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        Boolean AdjustCalculationDatesIndicator { get; }

        ///// <summary>
        ///// Gets or sets the cap floor stream.
        ///// </summary>
        ///// <value>The cap floor stream.</value>
        //IPriceableInstrumentController<InterestRateStream> CapFloorStream { get; set; }
    }
}