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

#region Using directives

using System;
using System.Collections.Generic;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Assets.Swaps
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class IRSwapAssetAnalytic : ModelAnalyticBase<IIRSwapAssetParameters, RateMetrics>, IIRSwapAssetResults
    {
        #region IRateOptionAssetResults Members

        /// <summary>
        /// Gets the npv change form a base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public Decimal NPVChange => throw new NotImplementedException();

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal IRateAssetResults.ImpliedQuote => throw new NotImplementedException();

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR => throw new NotImplementedException();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        decimal IRateAssetResults.AccrualFactor => throw new NotImplementedException();

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        /// <value>The convexity adjustment.</value>
        public decimal ConvexityAdjustment => throw new NotImplementedException();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity => throw new NotImplementedException();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        decimal IRateAssetResults.MarketQuote => throw new NotImplementedException();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv .</value>
        decimal IRateAssetResults.NPV => throw new NotImplementedException();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv .</value>
        decimal IRateAssetResults.NPVChange => throw new NotImplementedException();

        #endregion

        /// <summary>
        /// Evaluates the rates.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateLeg1Rates()
        {
            var rates = new List<double>();
            var dfs = AnalyticParameters.Leg1ForecastDiscountFactors;
            var fractions = AnalyticParameters.Leg1YearFractions;
            var resetLength = AnalyticParameters.Leg1ForwardRates?.Count ?? 0;
            if (ValidateInputs(dfs, fractions))
            {
                for (var index = 0; index < dfs.Count-1; index++)
                {
                    if (index < resetLength)
                    {
                        if (AnalyticParameters.Leg1ForwardRates != null)
                        {
                            var rate = AnalyticParameters.Leg1ForwardRates[index];
                            rates.Add(rate);
                        }
                    }
                    else
                    {
                        var rate = (dfs[index] / dfs[index + 1] - 1) / fractions[index];
                        rates.Add(rate);
                    }                  
                }
            }
            return rates;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateLeg1AccrualFactors()
        {
            var accrualFactors = new List<double>();
            var fractions = AnalyticParameters.Leg1YearFractions;           
            if (ValidateInputs(new List<ICollection<double>>{fractions, AnalyticParameters.Leg1Notionals}))
            {
                var index = 0;
                foreach (var factor in fractions)
                {
                    var accrualFactor = factor * AnalyticParameters.Leg1PaymentDiscountFactors[index + 1] *
                                        AnalyticParameters.Leg1Notionals[index] / AnalyticParameters.Leg1Notionals[0];
                    accrualFactors.Add(accrualFactor);
                    index++;
                }
            }
            return accrualFactors;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateLeg1NPV()//We assume in arrears payment initially.
        {
            var expectedValues = EvaluateLeg1ExpectedValues();
            var npvs = new List<double>();
            if (ValidateInputs(AnalyticParameters.Leg1PaymentDiscountFactors, expectedValues))
            {
                var index = 0;
                foreach (var expectedValue in expectedValues)
                {
                    npvs.Add(expectedValue * AnalyticParameters.Leg1PaymentDiscountFactors[index + 1]);
                    index++;
                }
            }
            return npvs;
        }

        /// <summary>
        /// Evaluates the expected values.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateLeg1ExpectedValues()
        {
            var rawValues = EvaluateLeg1Rates();
            var expectedValues = new List<double>();
            if (
                ValidateInputs(new List<ICollection<double>>{rawValues, AnalyticParameters.Leg1Notionals,
                                                             AnalyticParameters.Leg1YearFractions}))
            {
                var index = 0;
                foreach (var rawValue in rawValues)
                {
                    expectedValues.Add(rawValue * AnalyticParameters.Leg1Notionals[index] * AnalyticParameters.Leg1YearFractions[index]);
                    index++;
                }
            }
            return expectedValues;
        }

        private static Boolean ValidateInputs(ICollection<double> dfs, ICollection<double> other)
        {
            bool result = dfs.Count - 1 <= other.Count;
            return result;
        }

        private static Boolean ValidateInputs(IList<ICollection<double>> listOfArrays)
        {
            for(int i = 0; i <  listOfArrays.Count - 1; i++)
            {
                if (listOfArrays[i].Count != listOfArrays[i+1].Count)
                {
                    return false;
                }
            }
            return true;
        }

        #region Implementation of IIRSwapAssetResults

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1CashFlowDetails => throw new NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1RiskDetails => throw new NotImplementedException();

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2CashFlowDetails => throw new NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2RiskDetails => throw new NotImplementedException();

        #endregion
    }
}