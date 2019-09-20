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

using System.Collections.Generic;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Property.Lease
{
    public class LeaseTransactionAnalytic : ModelAnalyticBase<ILeaseAssetParameters, LeaseMetrics>, ILeaseAssetResults
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
            if (AnalyticParameters.PaymentDiscountFactors.Length != AnalyticParameters.Weightings.Length)
                return AnalyticParameters.Multiplier * dp;
            foreach (var flow in AnalyticParameters.Weightings)
            {
                dp = dp + AnalyticParameters.GrossAmount * flow * AnalyticParameters.PaymentDiscountFactors[i];
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
            var dp = new List<decimal>();
            if (AnalyticParameters.PaymentDiscountFactors.Length == AnalyticParameters.Weightings.Length)
            {
                foreach (var flow in AnalyticParameters.Weightings)
                {
                    dp.Add(AnalyticParameters.Multiplier * AnalyticParameters.GrossAmount * flow);
                }
            }
            return dp.ToArray();
        }

        /// <summary>
        /// Evaluates the pv of all cashflows.
        /// </summary>
        /// <returns></returns>
        private decimal[] EvaluateCashflowPVs()
        {
            var dp = new List<decimal>();
            var i = 0;
            if (AnalyticParameters.PaymentDiscountFactors.Length == AnalyticParameters.Weightings.Length)
            {
                foreach (var flow in AnalyticParameters.Weightings)
                {
                    dp.Add(AnalyticParameters.Multiplier * AnalyticParameters.GrossAmount * flow * AnalyticParameters.PaymentDiscountFactors[i]);
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