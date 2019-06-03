using System.Collections.Generic;

namespace Orion.Models.Assets.Swaps
{
    public class IRSwapAssetResults : RateAssetResults, IIRSwapAssetResults
    {
        #region Implementation of IIRSwapAssetResults

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1CashFlowDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1RiskDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2CashFlowDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2RiskDetails => throw new System.NotImplementedException();

        #endregion
    }
}