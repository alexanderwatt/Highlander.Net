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
using System.Collections.Generic;

namespace Highlander.Reporting.Models.V5r3.Property.Lease
{
    public interface ILeaseTransactionParameters
    {
        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        decimal GrossAmount { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the step up amount.
        /// </summary>
        /// <value>The amount.</value>
        decimal StepUp { get; set; }

        /// <summary>
        /// Gets or sets the dates.
        /// </summary>
        /// <value>The dates.</value>
        List<DateTime> PaymentDates { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the discount factors to use.
        /// </summary>
        /// <value>The dfs.</value>
        decimal[] PaymentDiscountFactors { get; set; }
    }
}