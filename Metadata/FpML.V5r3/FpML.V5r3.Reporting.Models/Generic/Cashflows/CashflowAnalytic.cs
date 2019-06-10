/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.ModelFramework;
using Orion.Models.Rates;
using Orion.ModelFramework.PricingStructures;
using Orion.Constants;
using Orion.Util.Helpers;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public class CashflowAnalytic : ModelAnalyticBase<ICashflowParameters, InstrumentMetrics>, IRateInstrumentResults
    {
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
        /// Gets the Bucketed Delta
        /// </summary>
        /// <value>The Bucketed Delta</value>
        public Decimal LocalCurrencyBucketedDelta1 => EvaluateBucketedDelta1();

        /// <summary>
        /// Gets the vector of Bucketed Delta
        /// </summary>
        /// <value>The vector of Bucketed Delta</value>
        public Decimal[] LocalCurrencyBucketedDeltaVector => EvaluateBucketDelta12();

        public Decimal[] LocalCurrencyBucketedDeltaVector2 => EvaluateBucketDelta12();

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
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => EvaluateMarketQuote();

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDelta0 => 0.0m;

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
        /// Gets the expected value.
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
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR => LocalCurrencyDeltaR * GetFxRate();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        public decimal FloatingNPV => LocalCurrencyFloatingNPV * GetFxRate();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor => LocalCurrencyAccrualFactor * GetFxRate();

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        public decimal HistoricalAccrualFactor => LocalCurrencyHistoricalAccrualFactor * GetFxRate();

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate => EvaluateBreakEvenRate();

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
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDeltaR => 0.0m;

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
        /// Gets the AccrualFactor.
        /// </summary>
        /// <value>The AccrualFactor.</value>
        public decimal LocalCurrencyAccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the Delta0.
        /// </summary>
        /// <value>The Delta0.</value>
        public decimal LocalCurrencyDelta0 => EvaluateDelta0();

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal LocalCurrencyDelta1 => EvaluateDelta1();

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDeltaR => EvaluateDeltaR();

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
        /// Gets the BES.
        /// </summary>
        /// <value>The BES.</value>
        public decimal BreakEvenStrike => 0.0m;//TODO THis is unfinished.

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE => new[] {0.0m};//TODO THis is unfinished.

        /// <summary>
        /// Gets the PCE Term.
        /// </summary>
        /// <value>The PCE Term.</value>
        public Decimal[] PCETerm => new[] {0.0m};//TODO THis is unfinished.

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
        /// Gets the calculated value.
        /// </summary>
        /// <value>The calculated value.</value>
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
        public IList<Pair<string, decimal>> RiskNPV => new List<Pair<string, decimal>> {new Pair<string, decimal>(AnalyticParameters.Currency, NPV)};

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        public decimal SimpleCVA => 0.0m;//TODO THis is unfinished.

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal HistoricalDelta1 => LocalCurrencyHistoricalDelta1 * GetFxRate();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        /// <summary>
        /// Gets the reporting currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> Delta1PDH => EvaluateDelta1PDH();

        /// <summary>
        /// Gets the reporting currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> Delta0PDH => null;

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta1PDH => EvaluateLocalCurrencyDelta1PDH();

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        public IDictionary<string, Decimal> LocalCurrencyDelta0PDH => null;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        public CashflowAnalytic()
        {
            ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">The fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        public CashflowAnalytic(DateTime valuationDate, DateTime paymentDate, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            PaymentDiscountFactor = EvaluateDiscountFactor(valuationDate, paymentDate, discountCurve);
            var paymentDiscountFactor = PaymentDiscountFactor;
            var days = (paymentDate - valuationDate).Days;
            if (days == 0)
            {
                days = 1;
                paymentDiscountFactor = EvaluateDiscountFactor(valuationDate, paymentDate.AddDays(1), discountCurve);
            }
            var time = days / 365.25;
            ContinuousRate = -(Decimal)(Math.Log((double)paymentDiscountFactor) / time);
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
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateExpectedValue()
        {
            return AnalyticParameters.NotionalAmount * GetMultiplier();
        }

        /// <summary>
        /// Evaluates the vector of bucketed delta
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal[] EvaluateBucketedDeltaVector()
        {
            Decimal[] temp = { 0.0m };
            return temp;
        }

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns>The break even rate</returns>
        protected virtual Decimal EvaluateBreakEvenRate()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {     
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaR()
        {
            return 0.0m;
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
        /// Evaluates the delta wrt the cash flow.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * ContinuousRate;
            return EvaluateNPV() * AnalyticParameters.CurveYearFraction / (1 + temp) / BasisPoint;
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
        /// Evaluates the gamma1 wrt the cash flow.
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
        /// Evaluates the rate.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateMarketQuote()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the break even spread.
        /// </summary>
        /// <returns>The break even spread</returns>
        protected virtual Decimal EvaluateBreakEvenSpread()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return GetPaymentDiscountFactor();
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected IDictionary<string, decimal> EvaluateDelta1PDH()
        {
            return LocalCurrencyDelta1PDH.ToDictionary(metric => metric.Key, metric => metric.Value*GetFxRate());
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
                    if (curve is IRateCurve rateCurve)
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
            return result ;
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
            var time = (AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear);
            var cycles = Convert.ToInt32(Math.Abs(Math.Floor(time)));
            var remainder = AnalyticParameters.CurveYearFraction - cycles * AnalyticParameters.PeriodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = EvaluateNPV() * AnalyticParameters.PeriodAsTimesPerYear / (1 + temp) / BasisPoint;
            }
            var tailValue = remainder * ContinuousRate;
            result[result.Length - 1] = EvaluateNPV() * remainder / (1 + tailValue) / BasisPoint;
            return result;
        }

        /// <summary>
        /// Evaluating Bucketed Delta
        /// </summary>
        /// <returns>The bucketed delta</returns>
        protected virtual Decimal EvaluateBucketedDelta1()
        {
            return EvaluateDelta1();
        }

        /// <summary>
        /// Evaluating the floating NPV.
        /// </summary>
        /// <returns>The NPV</returns>
        protected virtual Decimal EvaluateFloatingNPV()
        {
            return 0m;
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

        #endregion Methods
    }
}