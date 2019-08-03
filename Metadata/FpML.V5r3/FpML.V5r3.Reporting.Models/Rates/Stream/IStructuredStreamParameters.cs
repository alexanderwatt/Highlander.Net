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
using Orion.Models.Rates.Coupons;

namespace Orion.Models.Rates.Stream
{
    public interface IStructuredStreamParameters : IRateCouponParameters
    {
        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted fla.</value>
        bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        Decimal NPV { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        Decimal FloatingNPV { get; set; }

        /// <summary>
        /// Gets/Sets the weightings to be used for break even rate.
        /// </summary>
        /// <value>The Weightings.</value>
        Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets/Sets the notional to be used for break even rate.
        /// </summary>
        /// <value>The notionals.</value>
        Decimal[] CouponNotionals { get; set; }

        /// <summary>
        /// Gets/Sets the year fractions to be used for break even rate.
        /// </summary>
        Decimal[] CouponYearFractions { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        Decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the target npv for solvers.
        /// </summary>
        /// <value>The target npv.</value>
        Decimal TargetNPV { get; set; }
    }
}