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
    public class LeaseAssetParameters : ILeaseAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ILeaseAssetParameters Members

        public decimal Quote { get; set; }


        /// <summary>
        /// The multiplier which must be set.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Gets or sets the step up amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal StepUp { get; set; }

        /// <summary>
        /// Gets or sets the step up amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the dates.
        /// </summary>
        /// <value>The dates.</value>
        public List<DateTime> PaymentDates { get; set; }

        /// <summary>
        /// Gets or sets the discount factors to use.
        /// </summary>
        /// <value>The dfs.</value>
        public decimal[] PaymentDiscountFactors { get; set; }


        #endregion
    }
}