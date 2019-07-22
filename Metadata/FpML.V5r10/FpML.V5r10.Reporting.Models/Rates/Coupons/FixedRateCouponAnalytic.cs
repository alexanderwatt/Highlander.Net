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
using System.Linq;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Highlander.Numerics.Solvers;
using MathNet.Numerics.Differentiation;
using Orion.Constants;
using Orion.Util.Helpers;
using Double=System.Double;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Coupons
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class FixedRateCouponAnalytic : ModelAnalyticBase<IRateCouponParameters, InstrumentMetrics>, IRateInstrumentResults
    {
        protected const Decimal COne = 1.0m;
        protected const Decimal BasisPoint = 10000.0m;

        #region Properties

        /// <summary>
        /// Gets or sets the continuous rate.
        /// </summary>
        /// <value>The continuous rate.</value>
        public Decimal ContinuousRate { get; protected set; }

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
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal StartDiscountFactor { get; protected set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal EndDiscountFactor { get; protected set; }

        /// <summary>
        /// Gets or sets the discount rate.
        /// </summary>
        /// <value>The discount rate.</value>
        public Decimal? DiscountRate { get; protected set; } 

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
                if (AnalyticParameters.IsRealised) return result;
                for (int index = 0; index < length; index++)
                {
                    result[index] = LocalCurrencyBucketedDeltaVector[index] * GetFxRate();
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
                if (AnalyticParameters.IsRealised) return result;
                for (int index = 0; index < length; index++)
                {
                    result[index] = LocalCurrencyBucketedDeltaVector2[index] * GetFxRate();
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
        public decimal AnalyticalDelta => LocalCurrencyAnalyticalDelta * GetFxRate();

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

        /// <summary>
        /// For use especially with nettable cash flows. Otherwise the same as NPV.
        /// </summary>
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
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal LocalCurrencyAccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor => LocalCurrencyAccrualFactor * GetFxRate();

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate => EvaluateBreakEvenRate();

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
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        public decimal HistoricalAccrualFactor => LocalCurrencyHistoricalAccrualFactor * GetFxRate();

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
        /// Gets the second derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The gamma wrt the fixed rate.</value>
        public decimal Gamma0 => LocalCurrencyGamma0 * GetFxRate();

        /// <summary>
        /// Gets the cross derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The cross gamma wrt the fixed rate.</value>
        public decimal Delta0Delta1 => LocalCurrencyDelta0Delta1 * GetFxRate();

        /// <summary>
        /// Gets the local currency gamma1.
        /// </summary>
        /// <value>The Gamma1.</value>
        public decimal LocalCurrencyGamma1 => EvaluateGamma1();

        /// <summary>
        /// Gets the local currency gamma0.
        /// </summary>
        /// <value>The Gamma0.</value>
        public decimal LocalCurrencyGamma0 => EvaluateGamma0();

        /// <summary>
        /// Gets the local currency cross gamma.
        /// </summary>
        /// <value>The cross gamma.</value>
        public decimal LocalCurrencyDelta0Delta1 => EvaluateDelta0Delta1();

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
        /// Gets the historical accrual factor.
        /// </summary>
        public decimal LocalCurrencyHistoricalAccrualFactor
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateAccrualFactor();
                }
                return result;
            }
        }

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
        /// Gets the historical delta wrt the fixed rate R.
        /// </summary>
        public decimal LocalCurrencyHistoricalDeltaR
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDeltaR();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDeltaR => LocalCurrencyHistoricalDeltaR * GetFxRate();

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
        public Decimal ImpliedQuote => EvaluateBreakEvenRate();

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
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDeltaR
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateDeltaR();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR => LocalCurrencyDeltaR * GetFxRate();

        /// <summary>
        /// Gets the derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The fistorical delta1.</value>
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
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();


        /// <summary>
        /// Gets the reporting cxurrency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> Delta1PDH => EvaluateDelta1PDH();

        /// <summary>
        /// Gets the reporting cxurrency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> Delta0PDH => EvaluateDelta0PDH();

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta1PDH => EvaluateLocalCurrencyDelta1PDH();

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta0PDH => EvaluateLocalCurrencyDelta0PDH();

        /// <summary>
        /// Gets the BES.
        /// </summary>
        /// <value>The BES.</value>
        public decimal BreakEvenStrike => EvaluateOptionStrike();

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCCE.</value>
        public Decimal[] PCE => new[] { 0.0m };

        /// <summary>
        /// Gets the PCE Term.
        /// </summary>
        /// <value>The PCCE Term.</value>
        public Decimal[] PCETerm => new[] { 0.0m };

        #endregion

        #region Constructor

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        public FixedRateCouponAnalytic()
        {
            ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        public FixedRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate,
            IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
                StartDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, startDate);
                EndDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, endDate);
                var days = (paymentDate - valuationDate).Days * 365.25;
                ContinuousRate = 0.0m;
                if (System.Math.Abs(days) > 0)
                {
                    ContinuousRate = -(Decimal)(System.Math.Log((double)PaymentDiscountFactor) / days);
                }
            }
        }

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve">The rate curve to use for forecasting.</param>
        public FixedRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate,
            IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
                StartDiscountFactor = (decimal)forecastCurve.GetDiscountFactor(valuationDate, startDate);
                EndDiscountFactor = (decimal)forecastCurve.GetDiscountFactor(valuationDate, endDate);
                var days = (paymentDate - valuationDate).Days * 365.25;
                ContinuousRate = 0.0m;
                if (System.Math.Abs(days) > 0)
                {
                    ContinuousRate = -(Decimal)(System.Math.Log((double)PaymentDiscountFactor) / days);
                }
            }
        }

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        public FixedRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate,
            decimal? discountRate, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
                StartDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, startDate);
                EndDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, endDate);
            }
            DiscountRate = discountRate;
            var days = (paymentDate - valuationDate).Days * 365.25;
            ContinuousRate = 0.0m;
            if (System.Math.Abs(days) > 0)
            {
                ContinuousRate = -(Decimal)(System.Math.Log((double)PaymentDiscountFactor) / days);
            }
        }

        /// <summary>
        /// Intantiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="yearFraction">The yearFraction.</param>
        /// <param name="discountType">The discount type. </param>
        /// <param name="reportingCurrencyFxCurve">The fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve">The rate curve to use for forecasting.</param>
        public FixedRateCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate, decimal? discountRate, 
            decimal yearFraction, DiscountType discountType, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
                StartDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, startDate);
                EndDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, endDate);
                DiscountRate = EvaluateDiscountRate(valuationDate, startDate, endDate, discountType, discountRate, yearFraction, discountCurve);
            }
            if (forecastCurve != null)
            {
                StartDiscountFactor = (decimal)forecastCurve.GetDiscountFactor(valuationDate, startDate);
                EndDiscountFactor = (decimal)forecastCurve.GetDiscountFactor(valuationDate, endDate);
                DiscountRate = EvaluateDiscountRate(valuationDate, startDate, endDate, discountType, discountRate, yearFraction, forecastCurve);
            }
            var days = (paymentDate - valuationDate).Days * 365.25;
            ContinuousRate = 0.0m;
            if (System.Math.Abs(days) > 0)
            {
                ContinuousRate = -(Decimal) (System.Math.Log((double) PaymentDiscountFactor)/days);
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
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateNPV(double paymentDF)
        {
            return EvaluateExpectedValue() * (decimal)paymentDF;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAnalyticalDelta()
        {
            return EvaluateDelta1();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta0()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * ContinuousRate;
            return EvaluateNPV() * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaCCR()
        {
            return EvaluateNPV() * AnalyticParameters.CurveYearFraction / BasisPoint;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAnalyticalGamma()
        {
            return EvaluateGamma1();
        }

        /// <summary>
        /// Evaluates the gamma0 wrt the cash flow.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateGamma0()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the cross gamma for the cash flow.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta0Delta1()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the gamma1 wrt the cash flow.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateGamma1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * ContinuousRate;
            return EvaluateDelta1() * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint;
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
            var temp = AnalyticParameters.PeriodAsTimesPerYear * ContinuousRate;
            var time = AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear;
            var cycles = Convert.ToInt32(System.Math.Abs(System.Math.Floor(time)));
            var remainder = AnalyticParameters.CurveYearFraction - cycles * AnalyticParameters.PeriodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -EvaluateNPV() * AnalyticParameters.PeriodAsTimesPerYear / (1 + temp) / BasisPoint;
            }
            var tailValue = remainder * ContinuousRate;
            result[result.Length - 1] = -EvaluateNPV() * remainder / (1 + tailValue) / BasisPoint;
            return result;
        }

        protected Decimal EvaluateNPV2()
        {
            return EvaluateExpectedValue2() * GetPaymentDiscountFactor();
        }

        protected Decimal[] EvaluatedBucketedRates()
        {
            var len = AnalyticParameters.BucketedDiscountFactors.Length - 1;//FindYearFractionIndex();
            var bucketedRates = new decimal[len];
            if (len == 0)
            {
                bucketedRates = new[] { 0.0m };
            }
            else
            {
                for (var i = len; i > 0; --i)
                {
                    bucketedRates[i - 1] = GetRate(AnalyticParameters.BucketedDiscountFactors[i - 1],
                                                   AnalyticParameters.BucketedDiscountFactors[i],
                                                   AnalyticParameters.PeriodAsTimesPerYear);
                }
            }           
            return bucketedRates;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateAccrualFactor()
        {
            decimal result;
            if (AnalyticParameters.ExpectedAmount == null)
            {
                if (AnalyticParameters.DiscountType == DiscountType.None)
                {
                    result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * GetMultiplier() *
                             GetPaymentDiscountFactor() / BasisPoint;                   
                }
                else
                {
                    result = EvaluateNPV() - EvaluateNPV2();
                }
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * GetMultiplier() * 
                         GetPaymentDiscountFactor() / BasisPoint;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns>The break even rate</returns>
        protected  Decimal EvaluateBreakEvenRate()
        {
            return GetRate(GetStartDiscountFactor(), GetEndDiscountFactor(), AnalyticParameters.YearFraction);
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="startDiscountFactor">The start discount factor.</param>
        /// <param name="endDiscountFactor">The end discount factor.</param>
        /// <param name="yearFaction">The year faction.</param>
        /// <returns></returns>
        protected static Decimal GetRate(Decimal startDiscountFactor, Decimal endDiscountFactor, Decimal yearFaction)
        {
            return (startDiscountFactor / endDiscountFactor - COne) / yearFaction;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="dfStart">The discount factor.</param>
        /// <param name="dfEnd"></param>
        /// <returns></returns>
        protected static Decimal GetRate(DateTime startDate, DateTime endDate, Decimal dfStart, Decimal dfEnd)
        {
            var yearFraction = (endDate - startDate).Days / 365.0m;
            return (dfStart/dfEnd - 1) / yearFraction;
        }

        /// <summary>
        /// Evaluates the break even spread.
        /// </summary>
        /// <returns>The break even spread</returns>
        protected  Decimal EvaluateBreakEvenSpread()
        {
            return EvaluateBreakEvenRate() - AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateDeltaR()//TODO
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
            return result / BasisPoint;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return GetStartDiscountFactor() / (COne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected IDictionary<string, decimal> EvaluateDelta1PDH()
        {
            return LocalCurrencyDelta1PDH.ToDictionary(metric => metric.Key, metric => metric.Value * GetFxRate());
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected IDictionary<string, decimal> EvaluateLocalCurrencyDelta1PDH()
        {
            var result = new Dictionary<string, decimal>(); //{{"Base", EvaluateNPV()}};
            var baseValue = EvaluateNPV();
            if (AnalyticParameters.Delta1PDHCurves != null)
            {
                foreach (var curve in AnalyticParameters.Delta1PDHCurves)
                {
                    var rateCurve = curve as IRateCurve;
                    if (rateCurve != null)
                    {
                        var properties = rateCurve.GetPricingStructureId().Properties;
                        var assetId = properties.GetValue<string>("PerturbedAsset", false);
                        var df = rateCurve.GetDiscountFactor(AnalyticParameters.ValuationDate,
                                                             AnalyticParameters.PaymentDate);
                        var npv = (EvaluateNPV(df) - baseValue)/ AnalyticParameters.Delta1PDHPerturbation;
                        if (!result.ContainsKey(assetId))
                        {
                            result.Add(assetId, -npv);
                        }
                        else
                        {
                            result.Add(assetId + ".DiscountCurve", -npv);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, decimal> EvaluateLocalCurrencyDelta0PDH()
        {
            return null;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, decimal> EvaluateDelta0PDH()
        {
            return null;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        internal Decimal EvaluateExpectedFloatingValue()//TODO - What about the margin? What about BasisPoint
        {
            decimal result;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * EvaluateBreakEvenRate() * GetMultiplier();                
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier() / (COne + EvaluateBreakEvenRate() * AnalyticParameters.YearFraction);
            }
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateExpectedValue()
        {
            decimal result = 0;
            if (AnalyticParameters.ExpectedAmount != null)
            {
                result = (Decimal)AnalyticParameters.ExpectedAmount;
            }
            else
            {
                if (AnalyticParameters.DiscountType == DiscountType.None)
                {
                    result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                             (AnalyticParameters.Rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) * GetMultiplier();
                    
                }
                else
                {
                    var discountRate = GetDiscountRate();
                    if (discountRate != null)
                        result = AnalyticParameters.NotionalAmount * GetMultiplier() -
                                 AnalyticParameters.NotionalAmount * GetMultiplier() /
                                 (COne +
                                  ((decimal)discountRate + AnalyticParameters.Spread - AnalyticParameters.BaseRate) *
                                  AnalyticParameters.YearFraction);
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the expected value2.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateExpectedValue2()
        {
            decimal result = 0;
            if (AnalyticParameters.ExpectedAmount != null)
            {
                result = (Decimal)AnalyticParameters.ExpectedAmount;
            }
            else
            {
                if (AnalyticParameters.DiscountType == DiscountType.None)
                {
                    result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                             (AnalyticParameters.Rate + AnalyticParameters.Spread - AnalyticParameters.BaseRate - 0.0001m) * GetMultiplier();
                }
                else
                {
                    var discountRate = GetDiscountRate();
                    if (discountRate != null)
                        result = AnalyticParameters.NotionalAmount * GetMultiplier() -
                                 AnalyticParameters.NotionalAmount * GetMultiplier() /
                                 (COne +
                                  (((decimal)discountRate + AnalyticParameters.Spread) - AnalyticParameters.BaseRate - 0.0001m) *
                                  AnalyticParameters.YearFraction);
                }
            }
            return result;
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
            Func<double, double> f = BucketedDeltaTargetFunction;
            var df = new NumericalDerivative();
            var rate = AnalyticParameters.Rate;
            var dRate = Decimal.ToDouble(rate);
            var delta = (Decimal)df.EvaluateDerivative(f, dRate, 1);
            return delta / BasisPoint;
        }

        /// <summary>
        /// Evaluating NPV
        /// </summary>
        /// <returns>The floating npv</returns>
        protected Decimal EvaluateFloatingNPV()
        {
            return EvaluateExpectedFloatingValue() * GetPaymentDiscountFactor();
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
            return AnalyticParameters.Rate;
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

        public decimal EvaluateDiscountRate(DateTime valuationDate, DateTime startDate, DateTime endDate, decimal? rate, decimal yearFraction, IRateCurve forecastCurve)
        {
            decimal discountRate;
            if (rate != null)
            {
                discountRate = (decimal)rate;
            }
            else
            {
                discountRate = EvaluateForwardRate(valuationDate, startDate, endDate, yearFraction, forecastCurve); //TODO this needs to be changed to the forward rate.
            }
            return discountRate;
        }

        public decimal? EvaluateDiscountRate(DateTime valuationDate, DateTime startDate, DateTime endDate, DiscountType discountType, decimal? rate, decimal yearFraction, IRateCurve forecastCurve)
        {
            if (discountType == DiscountType.AFMA)
            {
                return rate;
            }
            if (discountType == DiscountType.ISDA)
            {
                var discountRate = EvaluateForwardRate(valuationDate, startDate, endDate, yearFraction, forecastCurve); //TODO this needs to be changed to the forward rate.
                return discountRate;
            }
            return null;
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

        public decimal GetStartDiscountFactor()
        {
            var startDiscountFactor = AnalyticParameters.StartDiscountFactor;
            if (startDiscountFactor != null)
            {
                return (decimal)startDiscountFactor;
            }
            return StartDiscountFactor;
        }

        public decimal GetEndDiscountFactor()
        {
            var endDiscountFactor = AnalyticParameters.EndDiscountFactor;
            if (endDiscountFactor != null)
            {
                return (decimal)endDiscountFactor;
            }
            return EndDiscountFactor;
        }

        public decimal? GetDiscountRate()
        {
            if(AnalyticParameters.DiscountType == DiscountType.None) return null;
            if (AnalyticParameters.DiscountType == DiscountType.AFMA)
            {
                var discountRate = AnalyticParameters.DiscountRate;
                if (discountRate != null)
                {
                    return discountRate;
                }
                if (DiscountRate != null)
                {
                    return DiscountRate; //EvaluateDiscountRate(DateTime valuationDate, DateTime startDate, DateTime endDate, decimal? rate, decimal yearFraction, IRateCurve forecastCurve)
                }
                return AnalyticParameters.Rate;
            }
            return GetRate(GetStartDiscountFactor(), GetEndDiscountFactor(), AnalyticParameters.YearFraction);
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