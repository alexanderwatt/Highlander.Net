#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Options;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Options
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class RateOptionAssetAnalytic : ModelAnalyticBase<IRateOptionAssetParameters, RateOptionMetrics>, IRateOptionAssetResults
    {
        #region IRateOptionAssetResults Members

        public List<double[]> Greeks => EvaluateGreeks();

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public List<double> NPV => EvaluateNPV();

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal ImpliedStrike => EvaluateBreakEvenRate();

        /// <summary>
        /// Gets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        public decimal FlatVolatility => EvaluateFlatVolatility();

        /// <summary>
        /// Gets the forward rates have been calculated from the discount factors provided.
        /// </summary>
        /// <value>The NPV.</value>
        public List<double> ForwardRates => EvaluateRates();

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public List<double> RawValue => EvaluateRawValues();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public List<double> AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        public Decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Gets the second derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        public List<double> Theta0 => ExtractRisk(4);

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public List<double> VolatilityAtExpiry => new List<double> { (double)AnalyticParameters.FlatVolatility };

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public List<double> MarketQuote => new List<double> { (double)AnalyticParameters.FlatVolatility };

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public List<double> ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public List<double> ExpectedValue => EvaluateExpectedValues();

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public List<double> Delta0 => ExtractRisk(1);

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public List<double> Delta1 => new List<double> { 0.0 };

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        public List<double> Gamma0 => ExtractRisk(2);

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public List<double> Gamma1 => new List<double> { 0.0 };

        /// <summary>
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        public List<double> Vega0 => ExtractRisk(3);

        #endregion

        /// <summary>
        /// Evaluates the rates.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateBreakEvenRate()
        {
            return (Decimal)AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the rates.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateFlatVolatility()
        {
            return AnalyticParameters.FlatVolatility;
        }

        /// <summary>
        /// Evaluates the rates.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateRates()
        {
            var rates = new List<double>();
            var dfs = AnalyticParameters.ForecastDiscountFactors;
            var fractions = AnalyticParameters.YearFractions;
            var resetLength = AnalyticParameters.ForwardRates?.Count ?? 0;
            if (ValidateOptionInputs(dfs, fractions))
            {
                for (var index = 0; index < dfs.Count-1; index++)
                {
                    if (index < resetLength)
                    {
                        if (AnalyticParameters.ForwardRates != null)
                        {
                            var rate = AnalyticParameters.ForwardRates[index];
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
        protected List<double> EvaluateAccrualFactor()
        {
            var accrualFactor = new List<double>();
            var fractions = AnalyticParameters.YearFractions;
            if (ValidateOptionInputs(fractions))
            {
                var index = 0;
                foreach (var factor in fractions)
                {
                    accrualFactor.Add(factor * AnalyticParameters.PaymentDiscountFactors[index + 1] * AnalyticParameters.Notionals[index] / 100);
                    index++;
                }
            }
            return new List<double> {accrualFactor.Sum()};
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateRawValues()
        {
            var rawValues = new List<double>();
            var rates = EvaluateRates();
            if (rates.Count == AnalyticParameters.Strikes.Count && rates.Count == AnalyticParameters.Volatilities.Count && rates.Count == AnalyticParameters.TimesToExpiry.Count)
            {
                var index = 0;
                foreach (var rate in rates)
                {
                    var value = OptionAnalytics.Opt(!AnalyticParameters.IsPut, rate, AnalyticParameters.Strikes[index], 
                        AnalyticParameters.Volatilities[index], AnalyticParameters.TimesToExpiry[index]);
                    rawValues.Add(value);
                    index++;
                }
            }
            return rawValues;
        }

        /// <summary>
        /// Evaluates the greeks.
        /// </summary>
        /// <returns>The option greeks.</returns>
        protected List<double[]> EvaluateGreeks()
        {
            var greekValues = new List<double[]>();
            var rates = EvaluateRates();
            if (ValidateOptionInputs(rates))
            {
                var index = 0;
                foreach (var rate in rates)
                {
                    var value = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsPut, rate, AnalyticParameters.Strikes[index],
                        AnalyticParameters.Volatilities[index], AnalyticParameters.TimesToExpiry[index]);
                    greekValues.Add(value);
                    index++;
                }
            }
            return greekValues;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateNPV()//We assume in arrears payment initially.
        {
            var expectedValues = EvaluateExpectedValues();
            var npvs = new List<double>();
            if (ValidateOptionInputs(expectedValues))
            {
                var index = 0;
                foreach (var expectedValue in expectedValues)
                {
                    npvs.Add(expectedValue * AnalyticParameters.PaymentDiscountFactors[index + 1]);
                    index++;
                }
            }
            return npvs;//  new List<double> { npvs.Sum() }
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateImpliedQuote()
        {
            //TODO Temporary only. This needs to be corrected.
            return new List<double> {0.0};
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateDeltaR()
        {
            var result = ExtractRisk(1);
            var multiplier = 1.0m;
            if (!AnalyticParameters.IsPut)
            {
                multiplier = -1.0m;
            }
            return (Decimal)result.Sum() * multiplier;
        }

        /// <summary>
        /// Evaluates the expected values.
        /// </summary>
        /// <returns></returns>
        protected List<double> EvaluateExpectedValues()
        {
            var rawValues = EvaluateRawValues();            
            var expectedValues = new List<double>();
            if (ValidateOptionInputs(rawValues))
            {
                var index = 0;
                foreach (var rawValue in rawValues)
                {
                    expectedValues.Add(rawValue * AnalyticParameters.Notionals[index] * AnalyticParameters.YearFractions[index]);
                    index++;
                }
            }
            return expectedValues;
        }

        private static Boolean ValidateOptionInputs(ICollection<double> dfs, ICollection<double> other)
        {
            bool result = dfs.Count - 1 <= other.Count;
            return result;
        }

        private Boolean ValidateOptionInputs(ICollection<double> rates)
        {
            var result = rates.Count == AnalyticParameters.Strikes.Count && rates.Count == AnalyticParameters.Volatilities.Count
                && rates.Count == AnalyticParameters.TimesToExpiry.Count && rates.Count == AnalyticParameters.Notionals.Count
                && rates.Count == AnalyticParameters.YearFractions.Count;
            return result;
        }

        private List<double> ExtractRisk(int type)
        {
            var result = new List<double>();
            var greeks = Greeks;
            var rows = greeks.Count;
            for(int i = 0; i <  rows; i++ )
            {
                result.Add(greeks[i][type]);
            }
            return new List<double> { result.Sum() };
        }
    }
}