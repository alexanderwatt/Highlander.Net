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

namespace Orion.Models.Assets
{
    public enum RateMetrics { DiscountFactorAtMaturity, NPV, AccrualFactor, 
        ImpliedQuote, ConvexityAdjustment, DeltaR, MarketQuote, NPVChange}

    public interface IRateAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal NPV { get; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        decimal NPVChange { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        decimal MarketQuote { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        decimal DeltaR { get; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        decimal AccrualFactor { get; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        /// <value>The convexity adjustment.</value>
        decimal ConvexityAdjustment { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        decimal DiscountFactorAtMaturity { get; }
    }
}