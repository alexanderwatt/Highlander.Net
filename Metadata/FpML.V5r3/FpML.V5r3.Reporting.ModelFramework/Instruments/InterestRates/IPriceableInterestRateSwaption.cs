/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Swaption instrument
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableInterestRateSwaption<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the floating payer party reference.
        /// </summary>
        /// <value>The floating payer party reference.</value>
        string BuyerPartyReference { get; }

        /// <summary>
        /// Gets the fixed payer party reference.
        /// </summary>
        /// <value>The fixed payer party reference.</value>
        string SellerPartyReference { get; }

        /// <summary>
        /// Gets the premium payment dates.
        /// </summary>
        /// <value>The payment dates.</value>
        DateTime[] PremiumPaymentDates { get; }

        /// <summary>
        /// Gets the premium payment amounts.
        /// </summary>
        /// <value>The payment amounts.</value>
        Decimal[] PremiumPaymentAmounts { get; }

        /// <summary>
        /// Gets the exercise dates.
        /// </summary>
        /// <value>The exercise dates.</value>
        DateTime[] ExerciseDates { get; }

        /// <summary>
        /// Gets or sets the priceable swap instrument.
        /// </summary>
        /// <value>The priceable swap instrument.</value>
        IPriceableInstrumentController<Swap> PriceableSwapInstrument{ get; }
    }
}