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

namespace Orion.Models.Assets
{
    public interface IBondAssetParameters
    {
        /// <summary>
        /// Flag that sets whether the quote is yield to maturity or dirty price - as a decimal
        /// </summary>
        Boolean IsYTMQuote { get; set; }

        /// <summary>
        /// Flag that sets whether the first coupon is ex div.
        /// </summary>
        Boolean IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        Decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the accrued factor.
        /// </summary>
        /// <value>The accrued factor.</value>
        Decimal AccruedFactor { get; set; }

        /// <summary>
        /// Gets or sets the remaining accrued factor.
        /// </summary>
        /// <value>The remaining accrued factor.</value>
        Decimal RemainingAccruedFactor { get; set; }

        /// <summary>
        /// Gets or sets the accrual discount factors.
        /// </summary>
        /// <value>The accrual discount factors.</value>
        Decimal[] AccrualDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the spread leg discount factors.
        /// </summary>
        /// <value>The accrual discount factors.</value>
        Decimal[] SpreadDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the payment discount factors.
        /// </summary>
        /// <value>The payment discount factors.</value>
        Decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the coupon rate.
        /// </summary>
        /// <value>The coupon rate.</value>
        Decimal CouponRate { get; set; }

        /// <summary>
        /// Gets or sets the accrual year fractions.
        /// </summary>
        /// <value>The accrual year fractions.</value>
        Decimal[] AccrualYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the spread leg year fractions.
        /// </summary>
        /// <value>The spread leg year fractions.</value>
        Decimal[] SpreadYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the frequency as an int.
        /// </summary>
        /// <value>The frequency.</value>
        int Frequency { get; set; }

        /// <summary>
        /// Gets or sets the purchase price: dirty price.
        /// </summary>
        /// <value>The frequency.</value>
        Decimal PurchasePrice { get; set; }
    }
}