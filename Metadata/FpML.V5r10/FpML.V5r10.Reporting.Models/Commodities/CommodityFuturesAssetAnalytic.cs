/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.Reporting.Models.Commodities
{
    public class CommodityFuturesAssetAnalytic : ModelAnalyticBase<ICommodityFuturesAssetParameters, CommodityMetrics>, ICommodityAssetResults
    {
        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => AnalyticParameters.Position * AnalyticParameters.UnitAmount * (AnalyticParameters.Index - ImpliedQuote) * AnalyticParameters.PointValue;

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the derivative with respect to the fx forward.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal ForwardDelta => EvaluateForwardDelta();

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal SpotDelta => EvaluateSpotDelta();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal IndexAtMaturity => AnalyticParameters.Index;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Index;

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
        {
            try
            {
                return AnalyticParameters.Index;
            }
            catch
            {
                throw new System.Exception("Real solution does not exist");
            }
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateSpotDelta()
        {
            return .01m * AnalyticParameters.Position;
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateForwardDelta()
        {
            return .01m * AnalyticParameters.Position;
        }

    }
}