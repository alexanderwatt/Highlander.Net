using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    ///  TD - Denotes a generic type for model data
    ///  TR - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="TD"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public interface IModelController<TD, out TR> //: IController<TD,TR>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; set; }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        TR Calculate(TD modelData);

        /// <summary>
        /// Gets the model data.
        /// </summary>
        /// <value>The model data.</value>
        TD ModelData { get; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        IList<string> Metrics { get; set; }
    }
}
