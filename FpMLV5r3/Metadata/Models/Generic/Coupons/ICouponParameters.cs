﻿/*
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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Generic.Cashflows;

#endregion

namespace Highlander.Reporting.Models.V5r3.Generic.Coupons
{
    public interface ICouponParameters : ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount curves for calculating Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        ICollection<IPricingStructure> Delta0PDHCurves { get; set; }

        /// <summary>
        /// Gets or sets the perturbation for the Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        Decimal Delta0PDHPerturbation { get; set; }

        /// <summary>
        /// Gets or sets the discounting flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean HasReset { get; set; }

        /// <summary>
        /// Gets or sets the expected amount.
        /// </summary>
        /// <value>The expected amount.</value>
        Decimal? ExpectedAmount { get; set; }

        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        /// <value>The spread.</value>
        Decimal Spread { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        Decimal Index { get; set; }

    }
}