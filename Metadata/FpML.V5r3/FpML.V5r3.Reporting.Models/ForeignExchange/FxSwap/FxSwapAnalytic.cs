using System;
using Orion.ModelFramework;

namespace Orion.Models.ForeignExchange.FxSwap
{
    public class FxSwapAnalytic : ModelAnalyticBase<IFxSwapParameters, FxSwapInstrumentMetrics>, IFxSwapInstrumentResults
    {
        public decimal NPV => 0.0m;

        public decimal BaseCurrencyNPV => 0.0m;

        public decimal ForeignCurrencyNPV => 0.0m;
    }
}