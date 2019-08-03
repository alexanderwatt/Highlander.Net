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
using Orion.Analytics.Rates;
using Orion.ModelFramework;

namespace Orion.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class BondAssetAnalytic : ModelAnalyticBase<IBondAssetParameters, BondMetrics>, IBondAssetResults
    {
        private const int COne = 1;


        #region IBondAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the accrued coupon.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal AccruedInterest => EvaluateAccruedInterest();

        /// <summary>
        /// Gets the dirty price.
        /// </summary>
        /// <value>The dirty price.</value>
        public decimal DirtyPrice => EvaluateDirtyPrice();

        /// <summary>
        /// Gets the clean price.
        /// </summary>
        /// <value>The clean price.</value>
        public decimal CleanPrice => EvaluateCleanPrice();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        public Decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DV01 => 0.0m;

        /// <summary>
        /// Gets the convexity.
        /// </summary>
        /// <value>The convexity.</value>
        public decimal Convexity => 0.0m;

        /// <summary>
        /// Gets the asset swap spread.
        /// </summary>
        /// <value>The asset swap spread.</value>
        public decimal AssetSwapSpread => EvaluateAssetSwapSpread();

        /// <summary>
        /// Gets the zero coupon bond swap spread.
        /// </summary>
        /// <value>The zero coupon bond swap spread.</value>
        public decimal ZSpread => 0.0m;

        /// <summary>
        /// Gets the yield to maturity.
        /// </summary>
        /// <value>The yield to maturity.</value>
        public decimal YieldToMaturity
        {
            get
            {
                var result = 0.0m;
                if (AnalyticParameters.IsYTMQuote)
                {
                    result = AnalyticParameters.Quote;
                }
                return result;
            }//TODO Only works for ytm
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Quote;

        public decimal PandL => EvaluatePandL();

        #endregion

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var dp = EvaluateDirtyPrice();
            var pl = AnalyticParameters.NotionalAmount * (dp - AnalyticParameters.PurchasePrice);
            return AnalyticParameters.Multiplier * pl;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAssetSwapSpread()
        {
            var totalDfsCount = AnalyticParameters.PaymentDiscountFactors.Length;
            var couponAccrualFactor = EvaluateAccrualFactor();
            var bondPaymentDiscountFactor = AnalyticParameters.PaymentDiscountFactors[0];
            var assetSwapSpread = 0.0m;
            if (!AnalyticParameters.IsYTMQuote)
            {
                if (couponAccrualFactor != 0)
                {
                    assetSwapSpread = (couponAccrualFactor*AnalyticParameters.CouponRate
                                       + AnalyticParameters.PaymentDiscountFactors[totalDfsCount - 1]
                                       - AnalyticParameters.Quote*bondPaymentDiscountFactor)/couponAccrualFactor;
                }
            }
            else
            {
                if (couponAccrualFactor != 0)
                {
                    assetSwapSpread = (couponAccrualFactor * AnalyticParameters.CouponRate
                                       + AnalyticParameters.PaymentDiscountFactors[totalDfsCount - 1]
                                       - EvaluateDirtyPrice() * bondPaymentDiscountFactor) / couponAccrualFactor;
                }
            }
            //if (EvaluateSpreadLegAccrualFactor()=0)
            //{
            //    assetSwapSpread = (couponAccrualFactor * AnalyticParameters.CouponRate
            //        + AnalyticParameters.PaymentDiscountFactors[totalDfsCount - 1]
            //        - AnalyticParameters.Quote * bondPaymentDiscountFactor) / EvaluateSpreadLegAccrualFactor();
            //}
            return assetSwapSpread;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAccrualFactor()
        {
            var totalDfsCount = AnalyticParameters.PaymentDiscountFactors.Length;
            var accrualFactorTotal = 0.0m;
            for (var index = 1; index < totalDfsCount; index++)
            {
                accrualFactorTotal += (AnalyticParameters.PaymentDiscountFactors[index] *
                                       AnalyticParameters.AccrualYearFractions[index - COne] *
                                       AnalyticParameters.Weightings[index]);
            }
            return AnalyticParameters.Multiplier * accrualFactorTotal;
        }

        ///// <summary>
        ///// Evaluates the implied quote.
        ///// </summary>
        ///// <returns></returns>
        //private Decimal EvaluateSpreadLegAccrualFactor()
        //{
        //    var totalDfsCount = AnalyticParameters.SpreadDiscountFactors.Length;
        //    var accrualFactorTotal = 0.0m;
        //    for (var index = 1; index < totalDfsCount; index++)
        //    {
        //        accrualFactorTotal += (AnalyticParameters.SpreadDiscountFactors[index] *
        //                               AnalyticParameters.SpreadYearFractions[index - COne] *
        //                               AnalyticParameters.Weightings[index]);
        //    }
        //    return AnalyticParameters.Multiplier * accrualFactorTotal;
        //}

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote() //TODO check that the weightings are correct.
        {
            var totalDfsCount = AnalyticParameters.PaymentDiscountFactors.Length;
            var firstDiscountFactor = AnalyticParameters.PaymentDiscountFactors[0];
            var lastDiscountFactor = AnalyticParameters.PaymentDiscountFactors[totalDfsCount - COne];
            var result = (AnalyticParameters.Weightings[0] * firstDiscountFactor -
                          AnalyticParameters.Weightings[AnalyticParameters.Weightings.Length - COne] *
                          lastDiscountFactor) / EvaluateAccrualFactor();
            return AnalyticParameters.NotionalAmount * result;
        }

        /// <summary>
        /// Evaluates the accrued interest.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAccruedInterest()
        {
            var result = AnalyticParameters.AccruedFactor * AnalyticParameters.CouponRate;
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateCleanPrice()
        {
            var result = EvaluateDirtyPrice() - EvaluateAccruedInterest();
            return result;
        }
        
        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateDirtyPrice()
        {
            var result = 0.0m;
            if (AnalyticParameters.IsYTMQuote)
            {
                var accrualYearFraction = Convert.ToDouble(AnalyticParameters.RemainingAccruedFactor);
                var periods = AnalyticParameters.AccrualYearFractions.Length;
                var annualCoupon = Convert.ToDouble(AnalyticParameters.CouponRate);
                var h = AnalyticParameters.Frequency;
                var nextCoupon = AnalyticParameters.IsExDiv? 0.0 : annualCoupon / h;//TODO semi annual only.
                var next2Coupon = nextCoupon;
                var y = Convert.ToDouble(AnalyticParameters.Quote);                
                var v = 1/(1 + y/h);
                result = Convert.ToDecimal(BondAnalytics.ISMADP(accrualYearFraction, 0.0, periods - 1, nextCoupon,
                                                                next2Coupon, annualCoupon, 0, h, v, y));
            }
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = EvaluateDirtyPrice();
            var firstDiscountFactor = AnalyticParameters.PaymentDiscountFactors[0];
            var npv = dp * firstDiscountFactor * AnalyticParameters.NotionalAmount;
            return AnalyticParameters.Multiplier * npv;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateDeltaR()
        {
            var totalDfsCount = AnalyticParameters.PaymentDiscountFactors.Length;
            var accrualFactorTotal = 0.0m;
            for (var index = 1; index < totalDfsCount; index++)
            {
                accrualFactorTotal += (AnalyticParameters.PaymentDiscountFactors[index] *
                                       AnalyticParameters.AccrualYearFractions[index - COne] *
                                       AnalyticParameters.Weightings[index]);
            }
            return AnalyticParameters.Multiplier * accrualFactorTotal * AnalyticParameters.NotionalAmount / 10000;
        }
    }
}