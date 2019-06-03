using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework
{
    /// <summary>
    ///  D - Denotes a generic type for model data
    ///  R - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public interface IController<D, R>
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
        R Calculate(D modelData);
    }
}
