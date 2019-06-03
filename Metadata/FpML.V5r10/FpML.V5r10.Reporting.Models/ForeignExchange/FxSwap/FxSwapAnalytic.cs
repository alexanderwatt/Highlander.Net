using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap
{
    public class FxSwapAnalytic : ModelAnalyticBase<IFxSwapParameters, FxSwapInstrumentMetrics>, IFxSwapInstrumentResults
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal NPV => 0.0m;

        /// <summary>
        /// 
        /// </summary>
        public decimal BaseCurrencyNPV => 0.0m;

        /// <summary>
        /// 
        /// </summary>
        public decimal ForeignCurrencyNPV => 0.0m;
    }
}