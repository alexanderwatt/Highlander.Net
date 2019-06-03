using System;

namespace Orion.Models.ForeignExchange.FxSwap
{
    public enum FxSwapInstrumentMetrics
    {
        //NPV, 
        BaseCurrencyNPV
        , ForeignCurrencyNPV
    }

    public interface IFxSwapInstrumentResults
    {
        ///// <summary>
        ///// Gets the NPV in reporting currency.
        ///// </summary>
        ///// <value>The NPV.</value>
        //Decimal NPV { get; }

        /// <summary>
        /// Gets the base currency NPV.
        /// </summary>
        /// <value>The base xcurrency NPV.</value>
        Decimal BaseCurrencyNPV { get; }

        /// <summary>
        /// Gets the foreign currency NPV.
        /// </summary>
        /// <value>The foreign currency NPV.</value>
        Decimal ForeignCurrencyNPV { get; }

    }
}