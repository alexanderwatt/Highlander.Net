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

namespace Highlander.Reporting.Models.V5r3.Property.Lease
{
    public interface ILeaseTransactionResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal NPV { get; }

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
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        decimal PandL { get; }

        /// <summary>
        /// Gets the expected cashflows.
        /// </summary>
        /// <value>The expected cashflows.</value>
        decimal[] ExpectedCashflows { get; }

        /// <summary>
        /// Gets the pv pf the cashflows.
        /// </summary>
        /// <value>The pv of the cashflows.</value>
        decimal[] CashflowPVs { get; }
    }
}