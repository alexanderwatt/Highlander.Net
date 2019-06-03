#region Usings

using System;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public enum FloatingCashflowMetrics
    {
                //Base metrics, fx rate insensitive.
            IndexAtMaturity
            , ImpliedQuote
            , MarketQuote
            , BreakEvenIndex
            , BreakEvenStrike
                //Base metrics, assumed to be in local currency. Do not require an fx rate.
            , Delta1
            , Delta0
            , HistoricalDelta0
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
            , LocalCurrencyFloatingNPV
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
            , PCE
            , PCETerm
    }

    public interface IFloatingCashflowResults : ICashflowResults
    {
        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta0.</value>
        Decimal Delta0 { get; }

        /// <summary>
        /// Gets the LocalCurrencyDelta0.
        /// </summary>
        /// <value>The LocalCurrencyDelta0.</value>
        Decimal LocalCurrencyDelta0 { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenIndex { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenStrike { get; }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        Decimal IndexAtMaturity { get; }

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
    }
}
