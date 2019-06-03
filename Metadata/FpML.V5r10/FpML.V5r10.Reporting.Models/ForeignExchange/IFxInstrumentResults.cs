#region Usings

using System;
using FpML.V5r10.Reporting.Models.Generic.Cashflows;

#endregion

namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    public enum FxInstrumentMetrics
    {
                //Base metrics, fx rate insensitive.
            DiscountFactorAtMaturity
            , ImpliedQuote
            , MarketQuote
            , BreakEvenRate

                //Base metrics, assumed to be in local currency. Do not require an fx rate.
            , Delta1
            , Delta0
            , DeltaR
            , FloatingNPV
            , AccrualFactor
            , HistoricalAccrualFactor
            , HistoricalDelta0
            , HistoricalDeltaR
            , ExpectedValue
            , CalculatedValue
            , HistoricalValue
            , NFV
            , NPV
            , SimpleCVA 
            , BucketedDelta1
            , BucketedDeltaVector
            , BucketedDeltaVector2
            , HistoricalDelta1

                //Base metrics explicitly in local currency. Do not require an fx rate.
            , LocalCurrencyDelta1
            , LocalCurrencyDelta0
            , LocalCurrencyDeltaR
            , LocalCurrencyFloatingNPV
            , LocalCurrencyAccrualFactor
            , LocalCurrencyHistoricalAccrualFactor
            , LocalCurrencyExpectedValue
            , LocalCurrencyCalculatedValue
            , LocalCurrencyHistoricalValue
            , LocalCurrencyNFV
            , LocalCurrencyNPV
            , LocalCurrencySimpleCVA 
            , LocalCurrencyBucketedDelta1
            , LocalCurrencyBucketedDeltaVector
            , LocalCurrencyBucketedDeltaVector2
            , LocalCurrencyHistoricalDelta1
            , BreakEvenStrike
            , PCE
            , PCETerm
    }

    public interface IFxInstrumentResults : ICashflowResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenRate { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal LocalCurrencyDeltaR { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal HistoricalDeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal LocalCurrencyHistoricalDeltaR { get; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        Decimal[] PCE { get; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        int[] PCETerm { get; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        Decimal BreakEvenStrike { get; }
    }
}
