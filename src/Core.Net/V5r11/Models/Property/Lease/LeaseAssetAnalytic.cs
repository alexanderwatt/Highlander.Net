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

using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Models.V5r3.Property.Lease
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class LeaseAssetAnalytic : ModelAnalyticBase<ILeaseAssetParameters, LeaseMetrics>, ILeaseAssetResults
    {
        #region ILeaseAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public decimal PandL => EvaluatePandL();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the expected cashflows.
        /// </summary>
        /// <value>The expected cashflows.</value>
        public decimal[] ExpectedCashflows => EvaluateExpectedCashflows();

        /// <summary>
        /// Gets the pv pf the cashflows.
        /// </summary>
        /// <value>The pv of the cashflows.</value>
        public decimal[] CashflowPVs => EvaluateCashflowPVs();

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateNPV()
        {
            var dp = 0.0m;
            var i = 0;
            if (AnalyticParameters.Weightings.Length >= AnalyticParameters.PaymentDiscountFactors.Length)
                return AnalyticParameters.Multiplier * dp;
            foreach (var flow in AnalyticParameters.Weightings)
            {
                dp = dp + AnalyticParameters.GrossAmount * flow * AnalyticParameters.PaymentDiscountFactors[i + 1];
                i++;
            }
            return AnalyticParameters.Multiplier * dp;
        }

        /// <summary>
        /// Evaluates the expected cashflows.
        /// </summary>
        /// <returns></returns>
        private decimal[] EvaluateExpectedCashflows()
        {
            return AnalyticParameters.Weightings.Select(flow => AnalyticParameters.Multiplier * AnalyticParameters.GrossAmount * flow).ToArray();
        }

        /// <summary>
        /// Evaluates the pv of all cashflows.
        /// </summary>
        /// <returns></returns>
        private decimal[] EvaluateCashflowPVs()
        {
            var dp = new List<decimal>();
            var i = 0;
            if (AnalyticParameters.PaymentDiscountFactors.Length > AnalyticParameters.Weightings.Length)
            {
                foreach (var flow in AnalyticParameters.Weightings)
                {
                    dp.Add(AnalyticParameters.Multiplier * AnalyticParameters.GrossAmount * flow * AnalyticParameters.PaymentDiscountFactors[i+1]);
                    i++;
                }
            }
            return dp.ToArray();
        }

        private decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.Multiplier * AnalyticParameters.Quote - EvaluateNPV();
            return pl;
        }

        #endregion
    }
}