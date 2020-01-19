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

#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Highlander.Numerics.Solvers;
using MathNet.Numerics.Differentiation;
using Orion.Util.Helpers;

#endregion

namespace FpML.V5r10.Reporting.Models.Generic.Cashflows
{
    public class FloatingCashflowAnalytic : ModelAnalyticBase<IFloatingCashflowParameters, FloatingCashflowMetrics>, IFloatingCashflowResults
    {
        protected const Decimal COne = 1.0m;
        protected const Decimal BasisPoint = 10000.0m;

        #region Properties

        /// <summary>
        /// Gets or sets the valuation fx rate from the payment currency to the reporting currency.
        /// </summary>
        /// <value>The valuation fx rate from the payment currency to the reporting currency.</value>
        public Decimal ToReportingCurrencyRate { get; protected set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal PaymentDiscountFactor { get; protected set; }

        /// <summary>
        /// Gets or sets the floating Index.
        /// </summary>
        /// <value>The floating Index.</value>
        public Decimal FloatingIndex { get; protected set; }

        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        public decimal BucketedDelta1 => LocalCurrencyBucketedDelta1 * GetFxRate();

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        public decimal[] BucketedDeltaVector
        {
            get
            {
                var length = LocalCurrencyBucketedDeltaVector.Length;
                var result = new Decimal[length];
                if (!AnalyticParameters.IsRealised)
                {
                    for (int index = 0; index < length; index++)
                    {
                        result[index] = LocalCurrencyBucketedDeltaVector[index] * GetFxRate();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        public decimal[] BucketedDeltaVector2
        {
            get
            {
                var length = LocalCurrencyBucketedDeltaVector2.Length;
                var result = new Decimal[length];
                if (!AnalyticParameters.IsRealised)
                {
                    for (int index = 0; index < length; index++)
                    {
                        result[index] = LocalCurrencyBucketedDeltaVector2[index] * GetFxRate();
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the total delta in the base currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        public decimal LocalCurrencyAnalyticalDelta => EvaluateAnalyticalDelta();

        /// <summary>
        /// Gets the total delta in the reporting currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        public decimal AnalyticalDelta  => LocalCurrencyAnalyticalDelta* GetFxRate();

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal Delta1 => LocalCurrencyDelta1 * GetFxRate();

        /// <summary>
        /// Gets the total gamma in the base currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        public decimal LocalCurrencyAnalyticalGamma => EvaluateAnalyticalGamma();

        /// <summary>
        /// Gets the total gamma in the reporting currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        public decimal AnalyticalGamma => LocalCurrencyAnalyticalGamma * GetFxRate();

        /// <summary>
        /// Gets the Gamma1.
        /// </summary>
        /// <value>The Gamma1.</value>
        public decimal Gamma1 => LocalCurrencyGamma1 * GetFxRate();

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal ExpectedValue => LocalCurrencyExpectedValue * GetFxRate();

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal CalculatedValue => LocalCurrencyCalculatedValue * GetFxRate();

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal HistoricalValue => LocalCurrencyHistoricalValue * GetFxRate();

        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        public decimal NFV => LocalCurrencyNFV * GetFxRate();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public decimal NPV => LocalCurrencyNPV * GetFxRate();

        public IList<Pair<string, decimal>> RiskNPV => new List<Pair<string, decimal>> { new Pair<string, decimal>(AnalyticParameters.Currency, NPV) };

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        public decimal SimpleCVA => 0.0m;

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal HistoricalDelta1 => LocalCurrencyHistoricalDelta1 * GetFxRate();

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenIndex => FloatingIndex;

        /// <summary>
        /// Gets the break even strike.
        /// </summary>
        /// <value>The break even strike.</value>
        public decimal BreakEvenStrike => EvaluateBreakEvenStrike();

        /// <summary>
        /// Gets the Bucketed Delta
        /// </summary>
        /// <value>The Bucketed Delta</value>
        public Decimal LocalCurrencyBucketedDelta1
        {
            get
            {
                var result = 0.0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateBucketedDelta1();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the vector of Bucketed Delta
        /// </summary>
        /// <value>The vector of Bucketed Delta</value>
        public Decimal[] LocalCurrencyBucketedDeltaVector => EvaluateBucketDelta12();

        public Decimal[] LocalCurrencyBucketedDeltaVector2 => EvaluateBucketDelta12();

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public Decimal BreakEvenSpread => EvaluateBreakEvenSpread();

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>//TODO need to return zero if the coupon has reset.
        public decimal LocalCurrencyDelta0
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta0();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta0 => LocalCurrencyDelta0 * GetFxRate();

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal LocalCurrencyNPV
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateNPV();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The npv.</value>
        public decimal LocalCurrencySimpleCVA => 0.0m;

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal LocalCurrencyFloatingNPV => EvaluateFloatingNPV();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        public decimal FloatingNPV => LocalCurrencyFloatingNPV * GetFxRate();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => EvaluateMarketQuote();

        /// <summary>
        /// Gets the Net Future Valuie of realised cash flows.
        /// </summary>
        /// <value>The NFV.</value>
        public Decimal LocalCurrencyNFV
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateNPV();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The non-discounted expected value.</value>
        public decimal LocalCurrencyExpectedValue
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateExpectedValue();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The non-discounted expected value.</value>
        public decimal LocalCurrencyCalculatedValue => EvaluateExpectedValue();

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>//TODO need to return zero if the coupon has reset.
        public decimal LocalCurrencyHistoricalDelta0
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta0();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDelta0 => LocalCurrencyHistoricalDelta0 * GetFxRate();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal LocalCurrencyHistoricalValue
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateExpectedValue();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => FloatingIndex;

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal LocalCurrencyDelta1
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta1();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the Gamma1.
        /// </summary>
        /// <value>The Gamma1.</value>
        public decimal LocalCurrencyGamma1
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateGamma1();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal LocalCurrencyHistoricalDelta1
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta1();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        public Decimal IndexAtMaturity => 0.0m;

        /// <summary>
        /// Gets the reporting cxurrency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> Delta1PDH { get; protected set; }

        /// <summary>
        /// Gets the reporting cxurrency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> Delta0PDH { get; protected set; }

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta1PDH { get; protected set; }

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta0PDH { get; protected set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCCE.</value>
        public Decimal[] PCE => new[] { 0.0m };

        /// <summary>
        /// Gets the PCE Term.
        /// </summary>
        /// <value>The PCCE Term.</value>
        public int[] PCETerm => new[] { 0 };

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new model.
        /// </summary>
        public FloatingCashflowAnalytic()
        {
            ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// Instantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="fixingDate">The fixing date of the index.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="indexCurve">The rate curve to use for calculating the forward index.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        public FloatingCashflowAnalytic(DateTime valuationDate, DateTime fixingDate, DateTime paymentDate,
            IFxCurve reportingCurrencyFxCurve, ICurve indexCurve, IRateCurve discountCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (indexCurve != null)
            {
                FloatingIndex = (decimal)indexCurve.GetValue(new DateTimePoint1D(valuationDate, fixingDate)).Value;
            }
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateNPV()
        {
            return EvaluateExpectedValue() * GetPaymentDiscountFactor();
        }
        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAnalyticalDelta()
        {
            return EvaluateDelta1() + EvaluateDelta0();
        }


        /// <summary>
        /// Evaluates the gamma0 wrt the cash flow.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateGamma0()//TODO This is unfinished
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAnalyticalGamma()
        {
            return EvaluateGamma1() + EvaluateGamma0() + EvaluateDelta0Delta1();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta0()//TODO this is not correct
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * GetStartIndex();
            return EvaluateNPV() * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint;
        }


        /// <summary>
        /// Evaluates the cross gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta0Delta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * GetStartIndex();
            return -2 * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint * EvaluateDelta0();
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateGamma1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * GetStartIndex();
            return EvaluateDelta1() * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaCCR()
        {
            return EvaluateNPV() * AnalyticParameters.CurveYearFraction / 10000;
        }

        /// <summary>
        /// Find the index of this coupon year fraction in Array of bucketed year fractions
        /// </summary>
        /// <returns></returns>
        protected int FindYearFractionIndex()
        {
            var time = (double)(AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear);
            var cycles = Convert.ToInt32(time);

            return cycles;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal[] EvaluateBucketDelta12()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * FloatingIndex;
            var time = (AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear);
            var cycles = Convert.ToInt32(System.Math.Floor(time));
            var remainder = AnalyticParameters.CurveYearFraction - cycles * AnalyticParameters.PeriodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -EvaluateNPV() * AnalyticParameters.PeriodAsTimesPerYear / (1 + temp) / BasisPoint;
            }
            var tailValue = remainder * FloatingIndex;
            result[result.Length - 1] = -EvaluateNPV() * remainder / (1 + tailValue) / BasisPoint;
            return result;
        }

        //protected Decimal EvaluateNPV2()
        //{
        //    return EvaluateExpectedValue2() * GetPaymentDiscountFactor();
        //}

        protected Decimal[] EvaluatedBucketedRates()
        {
            var len = FindYearFractionIndex();
            var bucketedRates = new decimal[len];

            if (len == 0)
            {
                bucketedRates = new[] { 0.0m };
            }
            else
            {
                for (var i = len; i > 0; --i)
                {
                    bucketedRates[i - 1] = 0.0m;// GetRate(AnalyticParameters.BucketedDiscountFactors[i - 1],
                                                //    AnalyticParameters.BucketedDiscountFactors[i],
                                                //    AnalyticParameters.PeriodAsTimesPerYear);
                }
            }
            
            return bucketedRates;
        }

        /// <summary>
        /// Evaluates the break even spread.
        /// </summary>
        /// <returns>The break even spread</returns>
        protected  Decimal EvaluateBreakEvenSpread()
        {
            return FloatingIndex - GetStartIndex();
        }

        /// <summary>
        /// Evaluates the expected value.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateExpectedValue()
        {
            decimal result = AnalyticParameters.NotionalAmount * (GetFloatingIndex() - GetStartIndex()) * GetMultiplier();
            return result;
        }

        /// <summary>
        /// Evaluates the break even strike. For a forward this is the current index.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateBreakEvenStrike()
        {
            return FloatingIndex;
        }

        /// <summary>
        /// RealFunction used by extreme optimizer to calculate the derivative
        /// of price wrt floating rate
        /// </summary>
        /// <param name="x">The given rate</param>
        /// <returns></returns>
        protected virtual Decimal BucketedDeltaTargetFunction(Decimal x)
        {
            const decimal multiplier = 1.0m;
            var bucketedRates = EvaluatedBucketedRates();
            var len = bucketedRates.Length;
            var poly = new Polynomial(1, 0);
            var index = len;
            while (index > 0)
            {
                var coeffs = new Decimal[2];
                coeffs[0] = 1.0m;
                coeffs[1] = AnalyticParameters.PeriodAsTimesPerYear;
                var thePoly = new Polynomial(coeffs, 1);
                poly = poly * thePoly;
                --index;
            }
            return EvaluateExpectedValue() / (multiplier * poly.Value(x));
        }

        /// <summary>
        /// Evaluating Bucketed Delta
        /// </summary>
        /// <returns>The bucketed delta</returns>
        protected virtual Decimal EvaluateBucketedDelta1()
        {
            var solution = new NumericalDerivative();
            Func<double, double> f = BucketedDeltaTargetFunction;
            var rate = FloatingIndex;
            var dRate = Decimal.ToDouble(rate);
            var delta = (Decimal)solution.EvaluateDerivative(f, dRate, 1);
            return delta / BasisPoint;
        }

        /// <summary>
        /// Evaluating NPV
        /// </summary>
        /// <returns>The floating npv</returns>
        protected Decimal EvaluateFloatingNPV()
        {
            return 0.0m; 
        }

        /// <summary>
        /// Implementation of business logic used by extreme optimization
        /// </summary>
        /// <param name="x">current value</param>
        /// <returns></returns>
        protected Double BucketedDeltaTargetFunction(Double x)
        {
            var dx = (Decimal)x;
            return Decimal.ToDouble(BucketedDeltaTargetFunction(dx));
        }

        /// <summary>
        /// Evaluating the vector of Bucketed Delta
        /// </summary>
        /// <returns>The vector of bucketed delta</returns>
        protected virtual Decimal[] EvaluateBucketedDeltaVector()
        {
            var bucketedRates = EvaluatedBucketedRates();
            var len = bucketedRates.Length;
            var bucketedDeltaVector = new Decimal[len];
            const Decimal cDefault = 0.0m;
            Decimal result;
            if (len == 1 && bucketedRates[0] == cDefault)
            {
                result = cDefault;
            }
            else
            {
                result = EvaluateBucketedDelta1();
            }
            for (var i = 0; i < len; ++i)
            {
                bucketedDeltaVector[i] = result;
            }
            return bucketedDeltaVector;
        }

        /// <summary>
        /// Evaluates the rate.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateMarketQuote()
        {
            return FloatingIndex;
        }

        protected virtual Decimal EvaluateOptionStrike()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluating the fx rate.
        /// </summary>
        /// <returns>The fx rate</returns>
        protected decimal EvaluateReportingCurrencyFxRate(DateTime valuationDate, IFxCurve fxCurve)
        {
            var result = 1.0m;
            if (fxCurve != null)
            {
                result = (decimal)fxCurve.GetForward(valuationDate, valuationDate);
            }
            return result;
        }

        /// <summary>
        /// Evaluating the discount factor rate.
        /// </summary>
        /// <returns>The discount factor</returns>
        protected decimal EvaluateDiscountFactor(DateTime valuationDate, DateTime date, IRateCurve discountCurve)
        {
            var result = 1.0m;
            if (discountCurve != null)
            {
                result = (decimal)discountCurve.GetDiscountFactor(valuationDate, date);

            }
            return result;
        }

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <returns></returns>
        public decimal EvaluateForwardRate(DateTime valuationDate, DateTime startDate, DateTime forwardDate, decimal yearFraction, IRateCurve forecastCurve)
        {
            var forwardRate = 0.0d;
            if (forecastCurve != null)
            {
                var startDiscountFactor = forecastCurve.GetDiscountFactor(valuationDate, startDate);
                var endDiscountFactor = forecastCurve.GetDiscountFactor(valuationDate, forwardDate);
                if (yearFraction != 0)
                {
                    forwardRate = (startDiscountFactor / endDiscountFactor - 1) / (double)yearFraction;
                }
            }
            return (decimal)forwardRate;
        }

        public decimal GetPaymentDiscountFactor()
        {
            var paymentDiscountFactor = AnalyticParameters.PaymentDiscountFactor;
            if (paymentDiscountFactor != null)
            {
                return (decimal)paymentDiscountFactor;
            }
            return PaymentDiscountFactor;
        }

        public decimal GetFxRate()
        {
            var fxRate = AnalyticParameters.ToReportingCurrencyRate;
            if (fxRate != null)
            {
                return (decimal)fxRate;
            }
            return ToReportingCurrencyRate;
        }

        public decimal GetStartIndex()
        {
            var startIndex = AnalyticParameters.StartIndex;
            if (startIndex != null)
            {
                return (decimal)startIndex;
            }
            return 0.0m;
        }

        public decimal GetFloatingIndex()
        {
            var floatingIndex = AnalyticParameters.FloatingIndex;
            if (floatingIndex != null && AnalyticParameters.IsReset)
            {
                return (decimal)floatingIndex;
            }
            return FloatingIndex;
        }

        public decimal GetMultiplier()
        {
            var multiplier = AnalyticParameters.Multiplier;
            if (multiplier != null)
            {
                return (decimal)multiplier;
            }
            return 1.0m;
        }

        #endregion
    }
}