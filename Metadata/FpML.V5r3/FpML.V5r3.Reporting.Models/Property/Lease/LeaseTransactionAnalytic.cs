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

#region Usings

using System;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Property.Lease
{
    public class LeaseTransactionAnalytic : ModelAnalyticBase<ILeaseAssetParameters, LeaseMetrics>, ILeaseAssetResults
    {
        #region IEquityAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public Decimal PandL => EvaluatePandL();

        /// <summary>
        /// Gets the index
        /// </summary>
        public decimal IndexAtMaturity => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = AnalyticParameters.PurchaseAmount * AnalyticParameters.Quote;
            return AnalyticParameters.Multiplier * dp;
        }

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.PurchaseAmount * (AnalyticParameters.Quote - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * pl;
        }

        #endregion    
    }
}