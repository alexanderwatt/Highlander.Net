
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework
{

    /// <summary>
    /// Base Analytic Model Interace (i.e. Type B Model)
    /// P - Denotes the parameters Interface
    /// M - Denotes the enum interface for the input to the calculation
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <typeparam name="EnumM">The type of the num M.</typeparam>
    public interface IModelAnalytic<P, EnumM>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; }

        /// <summary>
        /// Gets or sets the analytics parameters.
        /// </summary>
        /// <value>The analytics parameters.</value>
        P AnalyticParameters { get; set; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        List<EnumM> Metrics { get; }

        /// <summary>
        /// Sets the metrics.
        /// </summary>
        void SetMetrics();

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        R Calculate<R, C>(EnumM[] metrics);

        /// <summary>
        /// Calculates the specified analytic parameters.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="analyticParameters">The analytic parameters.</param>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        R Calculate<R, C>(P analyticParameters, EnumM[] metrics);
     }
}
