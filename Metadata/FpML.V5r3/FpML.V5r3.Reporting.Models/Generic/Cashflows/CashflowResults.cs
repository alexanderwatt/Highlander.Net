#region Usings

using System;
using System.Collections.Generic;
using Orion.Util.Helpers;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public class CashflowResults : ICashflowResults
    {
        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        public decimal NFV { get; protected set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal NPV { get; protected set; }

        /// <summary>
        /// Gets the risk npv.
        /// </summary>
        /// <value>The risk npv.</value>
        public IList<Pair<string, decimal>> RiskNPV { get; protected set; }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        public decimal SimpleCVA { get; protected set; }

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal HistoricalDelta1 { get; protected set; }

        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        public decimal LocalCurrencyBucketedDelta1 { get; protected set; }

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        public decimal[] LocalCurrencyBucketedDeltaVector { get; protected set; }

        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        public decimal[] LocalCurrencyBucketedDeltaVector2 { get; protected set; }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal LocalCurrencyDelta1 { get; protected set; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal LocalCurrencyExpectedValue { get; protected set; }

        /// <summary>
        /// Basically the expected value.
        /// </summary>
        public decimal LocalCurrencyCalculatedValue { get; protected set; }

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal LocalCurrencyHistoricalValue { get; protected set; }

        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        public decimal LocalCurrencyNFV { get; protected set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public decimal LocalCurrencyNPV { get; protected set; }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        public decimal LocalCurrencySimpleCVA { get; protected set; }

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal LocalCurrencyHistoricalDelta1 { get; protected set; }

        /// <summary>
        /// THe local currency gamma1
        /// </summary>
        public decimal LocalCurrencyGamma1 { get; protected set; }

        /// <summary>
        /// Gets the ExpectedValue.
        /// </summary>
        /// <value>The ExpectedValue.</value>
        public decimal ExpectedValue { get; protected set; }

        /// <summary>
        /// Gets the CalculatedValue.
        /// </summary>
        /// <value>The CalculatedValue.</value>
        public decimal CalculatedValue { get; protected set; }

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal HistoricalValue { get; protected set; }

        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        public decimal BucketedDelta1 { get; protected set; }

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        public decimal[] BucketedDeltaVector { get; protected set; }

        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        public decimal[] BucketedDeltaVector2 { get; protected set; }

        /// <summary>
        /// Gets the total delta in the base currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        public decimal LocalCurrencyAnalyticalDelta { get; protected set; }

        /// <summary>
        /// Gets the total delta in the reporting currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        public decimal AnalyticalDelta { get; protected set; }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal Delta1 { get; protected set; }

        /// <summary>
        /// Gets the total gamma in the base currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        public decimal LocalCurrencyAnalyticalGamma { get; protected set; }

        /// <summary>
        /// Gets the total gamma in the reporting currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        public decimal AnalyticalGamma { get; protected set; }

        /// <summary>
        /// Gets the gamma1.
        /// </summary>
        public decimal Gamma1 { get; protected set; }

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
    }
}