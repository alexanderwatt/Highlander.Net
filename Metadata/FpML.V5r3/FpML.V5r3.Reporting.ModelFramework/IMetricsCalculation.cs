using System;

namespace Orion.ModelFramework
{
    /// <summary>
    /// Base Interface for retrieving the input and output from a controllers calculation
    /// </summary>
    /// <typeparam name="IAP">The type of the AP.</typeparam>
    /// <typeparam name="IR">The type of the R.</typeparam>
    public interface IMetricsCalculation<IAP, IR>
    {
        /// <summary>
        /// Gets or sets a value indicating whether [calculation perfomed indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [calculation perfomed indicator]; otherwise, <c>false</c>.
        /// </value>
        Boolean CalculationPerfomedIndicator { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        IAP AnalyticModelParameters { get; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        IR CalculationResults { get; }
    }
}
