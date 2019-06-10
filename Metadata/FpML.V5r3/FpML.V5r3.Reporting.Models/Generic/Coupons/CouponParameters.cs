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
using System.Collections.Generic;
using Orion.ModelFramework;
using Orion.Models.Generic.Cashflows;

namespace Orion.Models.Generic.Coupons
{
    public class CouponParameters : CashflowParameters, ICouponParameters
    {
        /// <summary>
        /// Gets or sets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor { get; set; }


        /// <summary>
        /// Gets or sets the discount curves for calculating Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        public ICollection<IPricingStructure> Delta0PDHCurves { get; set; }

        /// <summary>
        /// Gets or sets the perturbation for the Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        public Decimal Delta0PDHPerturbation { get; set; }

        /// <summary>
        /// Gets or sets the discounting flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public bool HasReset { get; set; }

        /// <summary>
        /// Gets or sets the expected amount.
        /// </summary>
        /// <value>The expected amount.</value>
        public Decimal? ExpectedAmount { get; set; }

        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        /// <value>The spread.</value>
        public Decimal Spread { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public decimal Index { get; set; }

    }
}