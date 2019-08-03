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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Differentiation;
using Orion.Analytics.Solvers;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Rates.Coupons
{
    public class FloatingRateCouponAnalytic : FixedRateCouponAnalytic
    {
        #region Properties

        /// <summary>
        /// The accrual start date
        /// </summary>
        public DateTime StartDate { get; protected set; }

        /// <summary>
        /// The accrual end date
        /// </summary>
        public DateTime EndDate { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The parameters must all be pre calculated.
        /// </summary>
        public FloatingRateCouponAnalytic()
        {}

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve"> </param>
        public FloatingRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate, IFxCurve reportingCurrencyFxCurve, 
            IRateCurve discountCurve, IRateCurve forecastCurve)
            : base(valuationDate, startDate, endDate, paymentDate, reportingCurrencyFxCurve, discountCurve, forecastCurve)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="discountRate">The discount rate.</param>
        ///  <param name="yearFraction">The yearFraction.</param>
        /// <param name="reportingCurrencyFxCurve">The reportingCurrencyFxCurve</param>
        /// <param name="discountCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountType">The discountType.</param>
        /// <param name="forecastCurve">The rate curve to use for forecasting.</param>
        public FloatingRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate, decimal? discountRate, 
            decimal yearFraction, DiscountType discountType, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve)
            : base(valuationDate, startDate, endDate, paymentDate, discountRate, yearFraction, discountType, reportingCurrencyFxCurve, discountCurve, forecastCurve)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates the bucketed delta1.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateBucketedDelta1()
        {
            var bucketedDeltaVector = EvaluateBucketedDeltaVector();
            return bucketedDeltaVector.Sum();
        }

        /// <summary>
        /// Evaluating the vector of Bucketed Delta
        /// </summary>
        /// <returns>The vector of bucketed delta</returns>
        protected override Decimal[] EvaluateBucketedDeltaVector()
        {
            Func<double, double> f = BucketedDeltaTargetFunction;
            var df = new NumericalDerivative();
            var bucketedRates = EvaluatedBucketedRates();
            var bucketedDeltaVector = new decimal[bucketedRates.Length];
            const Decimal cDefault = 0.0m;
            if (bucketedRates.Length == 0 && bucketedRates[0] == cDefault)
            {
                bucketedDeltaVector[0] = cDefault;
            }
            else
            {
                for (var index = bucketedRates.Length - 1; index >= 0; index--)
                {
                    var rate = bucketedRates[index];
                    var dRate = Decimal.ToDouble(rate);
                    var temp = (Decimal)df.EvaluateDerivative(f, dRate, 1);
                    bucketedDeltaVector[index] = temp / BasisPoint;
                }
            }
            return bucketedDeltaVector;
        } 

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDelta0()
        {
            decimal result = 0;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * GetPaymentDiscountFactor() * GetMultiplier();
            }
            else
            {
                var discountRate = GetDiscountRate();
                if (discountRate != null)
                    result = EvaluateExpectedValue() * AnalyticParameters.YearFraction / (COne + (decimal)discountRate * AnalyticParameters.YearFraction);
            }
            return -result / BasisPoint;
        }

        /// <summary>
        /// Evaluates the cross gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDelta0Delta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * ContinuousRate;
            return 2 * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint * EvaluateDelta0();
        }

        /// <summary>
        /// RealFunction used by extreme optimizer to calculate the derivative
        /// of price wrt floating rate
        /// </summary>
        /// <param name="x">The given rate</param>
        /// <returns></returns>
        protected override Decimal BucketedDeltaTargetFunction(Decimal x)
        {

            Decimal multiplier = 1.0m;
            Decimal[] bucketedRates = EvaluatedBucketedRates();
            int len = bucketedRates.Length;
            var poly = new Polynomial(1, 0);
            for (int index = len - 1; index > -1; --index)
            {
                if (Math.Abs(Convert.ToDouble(bucketedRates[index] - x)) < 1e-5)
                {
                    var coeffs = new Decimal[2];
                    coeffs[0] = 1.0m;
                    coeffs[1] = AnalyticParameters.PeriodAsTimesPerYear;
                    poly = new Polynomial(coeffs, 1);
                }
                else
                {
                    Decimal accrualFactor = AnalyticParameters.PeriodAsTimesPerYear;
                    multiplier *= 1 + bucketedRates[index] * accrualFactor;
                }
            }
            return EvaluateExpectedValue() / (multiplier * poly.Value(x));
        }

        /// <summary>
        /// Evaluates the expected value.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue()
        {
            decimal result;
            if (AnalyticParameters.ExpectedAmount != null)
            {
                result = (Decimal)AnalyticParameters.ExpectedAmount;
            }
            else
            {
                var rate = EvaluateBreakEvenRate();
                if (AnalyticParameters.DiscountType == DiscountType.None)
                {
                    result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                             (rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) * GetMultiplier();

                }
                else
                {
                    result = AnalyticParameters.NotionalAmount * GetMultiplier() -
                                 AnalyticParameters.NotionalAmount * GetMultiplier() /
                                 (COne +
                                  (rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) *
                                  AnalyticParameters.YearFraction);
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateExpectedValue(decimal breakEvenRate)
        {
            decimal result;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                            (breakEvenRate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) * GetMultiplier();

            }
            else
            {
                result = AnalyticParameters.NotionalAmount * GetMultiplier() -
                                AnalyticParameters.NotionalAmount * GetMultiplier() /
                                (COne +
                                (breakEvenRate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) *
                                AnalyticParameters.YearFraction);
            }
            return result;
        }

        /// <summary>
        /// Evaluates the expected value.
        /// </summary>
        /// <returns></returns>
        internal Decimal EvaluateNPV(decimal breakEvenRate)
        {
            return EvaluateExpectedValue(breakEvenRate) * GetPaymentDiscountFactor();
        }

        /// <summary>
        /// Evaluates the expected value2.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue2()
        {
            decimal result;
            if (AnalyticParameters.ExpectedAmount != null)
            {
                result = (Decimal)AnalyticParameters.ExpectedAmount;
            }
            else
            {
                var rate = EvaluateBreakEvenRate();
                if (AnalyticParameters.DiscountType == DiscountType.None)
                {
                    result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                             (rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate - 0.0001m) * GetMultiplier();
                }
                else
                {
                   result = AnalyticParameters.NotionalAmount * GetMultiplier() -
                                 AnalyticParameters.NotionalAmount * GetMultiplier() /
                                 (COne +
                                  (rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate - 0.0001m) *
                                  AnalyticParameters.YearFraction);
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected override IDictionary<string, decimal> EvaluateLocalCurrencyDelta0PDH()
        {
            var result = new Dictionary<string, decimal>(); //TODO Perturb the forward rate not the discount rate.
            var baseValue = EvaluateNPV();
            if (AnalyticParameters.Delta0PDHCurves != null)
            {
                foreach (var curve in AnalyticParameters.Delta0PDHCurves)
                {
                    var rateCurve = curve as IRateCurve;
                    if (rateCurve == null) continue;
                    var properties = rateCurve.GetPricingStructureId().Properties;
                    var assetId = properties.GetValue<string>("PerturbedAsset", false);
                    var df1 = (decimal)rateCurve.GetDiscountFactor(AnalyticParameters.ValuationDate, StartDate);
                    var df2 = (decimal)rateCurve.GetDiscountFactor(AnalyticParameters.ValuationDate, EndDate);
                    var forwardRate = GetRate(df1, df2, AnalyticParameters.YearFraction);
                    var npv = (EvaluateNPV(forwardRate) - baseValue)/ AnalyticParameters.Delta0PDHPerturbation;
                    if (!result.ContainsKey(assetId))
                    {
                        result.Add(assetId, -npv);
                    }
                    else
                    {
                        result.Add(assetId + ".BasisCurve", -npv);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected override IDictionary<string, decimal> EvaluateDelta0PDH()
        {
            return LocalCurrencyDelta0PDH.ToDictionary(metric => metric.Key, metric => metric.Value * GetFxRate());
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateAnalyticalDelta()
        {
            return EvaluateDelta1() + EvaluateDelta0();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateAnalyticalGamma()
        {
            return EvaluateGamma1() + EvaluateGamma0() + EvaluateDelta0Delta1();
        }

        #endregion
    }
}