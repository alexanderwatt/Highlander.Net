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
using System.Linq;
using MathNet.Numerics.Differentiation;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.Reporting.Models.V5r3.Rates.Coupons
{
    public class StructuredRateCouponAnalytic : FloatingRateCouponAnalytic
    {
        #region Constructor

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve">The forecast curve.</param>
        public StructuredRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate, 
            IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve)
            : base(valuationDate, startDate, endDate, paymentDate, reportingCurrencyFxCurve, discountCurve, forecastCurve)
        {}

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
            if ((bucketedRates.Length == 0) && (bucketedRates[0] == cDefault))
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
            return EvaluateDeltaR();
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

        #endregion
    }
}