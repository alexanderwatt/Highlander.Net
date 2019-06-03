

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap
{
    public class FxSwapInstrumentResults : IFxSwapInstrumentResults
    {
        #region Implementation of IFxLegInstrumentResults

        /// <summary>
        /// Gets the NPV in reporting currency.
        /// </summary>
        /// <value>The NPV.</value>
        public decimal NPV { get; private set; }

        /// <summary>
        /// Gets the base currency NPV.
        /// </summary>
        /// <value>The base xcurrency NPV.</value>
        public decimal BaseCurrencyNPV { get; private set; }

        /// <summary>
        /// Gets the foreign currency NPV.
        /// </summary>
        /// <value>The foreign currency NPV.</value>
        public decimal ForeignCurrencyNPV { get; private set; }

        #endregion
    }
}