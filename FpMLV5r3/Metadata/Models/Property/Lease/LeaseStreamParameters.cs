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
    public class LeaseStreamParameters : ILeaseStreamParameters
    {
        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        public decimal NPV { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Quote { get; set; }

        public decimal GrossAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal StepUp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DateTime> PaymentDates { get; set; }

        /// <summary>
        /// Gets/Sets the weightings to be used for break even rate.
        /// </summary>
        /// <value>The Weightings.</value>
        public decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets/Sets the notional to be used for break even rate.
        /// </summary>
        /// <value>The notionals.</value>
        public decimal[] Amounts { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        public decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the target npv for solvers.
        /// </summary>
        /// <value>The target npv.</value>
        public decimal TargetNPV { get; set; }
    }
}