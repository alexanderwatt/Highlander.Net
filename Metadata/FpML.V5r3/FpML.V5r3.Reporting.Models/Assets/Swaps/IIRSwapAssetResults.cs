using System.Collections.Generic;

namespace Orion.Models.Assets.Swaps
{
    public interface IIRSwapAssetResults : IRateAssetResults
    {
        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double[]> Leg1CashFlowDetails { get; }

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double[]> Leg1RiskDetails { get; }

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double[]> Leg2CashFlowDetails { get; }

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double[]> Leg2RiskDetails { get; }
    }
}