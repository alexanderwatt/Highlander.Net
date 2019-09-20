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

namespace Orion.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class SwapAssetAnalytic : SimpleSwapAssetAnalytic
    {
        #region IRateAssetResults Members

        /// <summary>
        /// Gets the fixed leg npv.
        /// </summary>
        public decimal FixedLegNPV => EvaluateFixedLegNPV();

        /// <summary>
        /// Gets the floating leg accrual factor.
        /// </summary>
        /// <value>The floating leg accrual factor.</value>
        public decimal FloatLegAccrualFactor => EvaluateFloatingLegAccrualFactor();

        /// <summary>
        /// Gets the floating leg coupon npv.
        /// </summary>
        /// <value>The floating leg coupon npv.</value>
        public decimal FloatLegNPV => EvaluateFloatingLegNPV();

        /// <summary>
        /// Gets the floating leg principal repayment npv.
        /// This is the initial DF less the final DF
        /// when there are no principal repayments,
        /// based on the weighing vector supplied.
        /// </summary>
        /// <value>The floating leg principal repayment npv.</value>
        public decimal FloatLegPrincipalNPV => EvaluateFloatingLegPrincipalValues();

        /// <summary>
        /// Gets the floating leg coupon npv.
        /// </summary>
        /// <value>The floating leg coupon npv.</value>
        public decimal FloatLegCouponNPV => EvaluateFloatingCouponNPV();

        /// <summary>
        /// Gets the floating leg spread npv.
        /// </summary>
        /// <value>The floating leg spread npv.</value>
        public decimal FloatLegCouponSpreadNPV => EvaluateFloatingLegCouponSpreadNPV();

        /// <summary>
        /// Gets the floating leg delta.
        /// </summary>
        /// <value>The floating leg delta.</value>
        public decimal FloatLegAccrualDeltaR => EvaluateFloatingDeltaR();

        /// <summary>
        /// Gets the floating leg implied quote.
        /// </summary>
        /// <value>The floating leg implied quote.</value>
        public decimal FloatLegImpliedQuote => EvaluateFloatingLegImpliedQuote();

        /// <summary>
        /// Gets the fixed leg implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal FixedLegImpliedQuote => EvaluateFixedLegImpliedQuote();

        /// <summary>
        /// Gets the fixed leg implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal BaseLegImpliedSpreadQuote => EvaluateFixedLegImpliedQuote();

        #endregion

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected decimal EvaluateMarginLegImpliedSpreadQuote() //TODO check that the weightings are correct.
        {
            var fixedPrincipals = EvaluatePrincipalValues();
            var denominator = EvaluateAccrualFactor();
            var impliedQuote = fixedPrincipals / denominator;
            return impliedQuote;
        }

        /// <summary>
        /// Evaluates the floating leg principals.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingLegPrincipalValues()
        {
            var result = 0.0m;
            if (AnalyticParameters.FloatingLegDiscountFactors == null ||
                AnalyticParameters.FloatingLegYearFractions == null ||
                AnalyticParameters.FloatingLegWeightings == null) return result;
            var totalDfsCount = AnalyticParameters.FloatingLegDiscountFactors.Length;
            var initialPrincipal = AnalyticParameters.FloatingLegWeightings[0] * AnalyticParameters.FloatingLegDiscountFactors[0];
            var principalFactorTotal = 0.0m;
            for (var index = 0; index < totalDfsCount; index++)
            {
                var repayment = AnalyticParameters.FloatingLegWeightings[index] - AnalyticParameters.FloatingLegWeightings[index - 1];
                principalFactorTotal += AnalyticParameters.FloatingLegDiscountFactors[index] * repayment;
            }
            var finalRepayment = -AnalyticParameters.FloatingLegWeightings[totalDfsCount - 2] * AnalyticParameters.FloatingLegDiscountFactors[totalDfsCount - 1];
            result = initialPrincipal + principalFactorTotal + finalRepayment;
            return result;
        }

        /// <summary>
        /// Evaluates the floating leg accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingLegAccrualFactor()
        {
            var accrualFactorTotal = 0.0m;
            if (AnalyticParameters.FloatingLegDiscountFactors == null ||
                AnalyticParameters.FloatingLegYearFractions == null ||
                AnalyticParameters.FloatingLegWeightings == null) return accrualFactorTotal;
            var totalDfsCount = AnalyticParameters.FloatingLegDiscountFactors.Length;
            for (var index = 1; index < totalDfsCount; index++)
            {
                accrualFactorTotal += AnalyticParameters.FloatingLegDiscountFactors[index]*
                                      AnalyticParameters.FloatingLegYearFractions[index - COne]*
                                      AnalyticParameters.FloatingLegWeightings[index - COne];
            }
            return accrualFactorTotal;
        }

        ///TODO
        /// <summary>
        /// Evaluates the floating leg NPV.
        /// This does not include the principal repayments.
        /// The assumption is that repayments all net out. 
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingLegNPV()
        {
            var fixedLegCouponNPV = EvaluateFloatingCouponNPV();
            var fixedLegCouponSpreadNPV = EvaluateFloatingLegCouponSpreadNPV();
            var npv = fixedLegCouponNPV + fixedLegCouponSpreadNPV;
            return npv;
        }

        /// <summary>
        /// Evaluates the floating leg NPV.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingCouponNPV()
        {
            var floatingLeg = 0.0m;
            if (AnalyticParameters.FloatingLegDiscountFactors == null ||
                AnalyticParameters.FloatingLegYearFractions == null ||
                AnalyticParameters.FloatingLegWeightings == null)
                return floatingLeg;
            var totalDfsCount = AnalyticParameters.FloatingLegDiscountFactors.Length;
            for (var index = 1; index < totalDfsCount; index++)
            {
                //The floor is in place to handle the case when there are discount factors that are 0
                //in the solver.
                decimal df = 1/1000000m;
                if (AnalyticParameters.FloatingLegForecastDiscountFactors[index] > 0)
                {
                    df = AnalyticParameters.FloatingLegForecastDiscountFactors[index];
                }
                var rateAccrual = AnalyticParameters.FloatingLegForecastDiscountFactors[index - 1] / df - 1;
                floatingLeg += AnalyticParameters.FloatingLegDiscountFactors[index] *
                                      AnalyticParameters.FloatingLegWeightings[index - 1] * rateAccrual;
            }
            var floatingLegNPV = floatingLeg * AnalyticParameters.NotionalAmount;
            return floatingLegNPV;
        }

        /// <summary>
        /// Evaluates the floating leg coupon spread NPV.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingLegCouponSpreadNPV()
        {
            if (AnalyticParameters.FloatingLegSpread == 0)
                return 0.0m;
            var floatingLeg = 0.0m;
            if (AnalyticParameters.FloatingLegDiscountFactors == null ||
                AnalyticParameters.FloatingLegYearFractions == null ||
                AnalyticParameters.FloatingLegWeightings == null) return floatingLeg;
            var totalDfsCount = AnalyticParameters.FloatingLegDiscountFactors.Length;
            for (var index = 1; index < totalDfsCount; index++)
            {
                floatingLeg += AnalyticParameters.FloatingLegDiscountFactors[index] *
                                  AnalyticParameters.FloatingLegYearFractions[index - COne] *
                                  AnalyticParameters.FloatingLegWeightings[index - COne] *
                                  AnalyticParameters.FloatingLegSpread;
            }
            var floatingLegNPV = floatingLeg * AnalyticParameters.NotionalAmount;
            return floatingLegNPV;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected override decimal EvaluateImpliedQuote()
        {
            var impliedQuote = 0.0m;
            var fixedLegDelta = EvaluateDeltaR();
            var floatingLegNPV = EvaluateFloatingLegNPV();
            if (fixedLegDelta != 0)
            {
                impliedQuote = floatingLegNPV / fixedLegDelta;
            }
            return impliedQuote / CBasisPoint;
        }

        /// <summary>
        /// Evaluates the floating leg implied quote.
        /// </summary>
        /// <returns></returns>
        protected decimal EvaluateFloatingLegImpliedQuote()
        {
            var floatPrincipals = EvaluateFloatingLegPrincipalValues();
            var floatingLegAccrualFactor = EvaluateFloatingLegAccrualFactor();
            var impliedQuote = floatPrincipals / floatingLegAccrualFactor;
            return impliedQuote;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFixedLegImpliedQuote()
        {
            var fixedPrincipals = EvaluatePrincipalValues();
            var denominator = EvaluateAccrualFactor();
            var impliedQuote = fixedPrincipals / denominator;
            return impliedQuote;
        }

        /// <summary>
        /// Evaluates the npv. The assumption here is that the principal 
        /// movements net out to zero at every payment date.
        /// </summary>
        /// <returns></returns>
        protected override decimal EvaluateNPV()
        {
            var fixedLegNPV = EvaluateFixedLegNPV();
            var floatingLegNPV = EvaluateFloatingLegNPV();
            var npv = fixedLegNPV - floatingLegNPV;
            return npv;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateFloatingDeltaR()
        {
            return EvaluateFloatingLegAccrualFactor() * AnalyticParameters.NotionalAmount / CBasisPoint;
        }
    }
}