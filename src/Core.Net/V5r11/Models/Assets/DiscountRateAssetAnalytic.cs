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

namespace Highlander.Reporting.Models.V5r3.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class DiscountRateAssetAnalytic : RateAssetAnalytic
    {
        private const Decimal COne = 1.0m; //TODO not done this model yet.
        
        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateNPV()
        {
            var disc1 = 1 / (1 + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
            var result = AnalyticParameters.NotionalAmount *
                         (EvaluatePrice() - disc1);
            return result * AnalyticParameters.StartDiscountFactor;
        }

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluatePrice()
        {
            return AnalyticParameters.NotionalAmount / (1 + AnalyticParameters.YearFraction * EvaluateImpliedQuote());
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDeltaR()
        {
            return EvaluatePrice() * AnalyticParameters.YearFraction /
                   (1 + AnalyticParameters.YearFraction * AnalyticParameters.Rate) / 10000;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateImpliedQuote()
        {
            return ((AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor) - COne) /
                   AnalyticParameters.YearFraction;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor /
                   (COne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        }
    }
}