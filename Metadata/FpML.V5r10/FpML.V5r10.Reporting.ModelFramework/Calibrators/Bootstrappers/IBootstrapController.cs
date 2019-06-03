
namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// A bootstrap controller interface. 
    /// TD - Denotes a generic type for bootstrap data
    /// TR - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="TD"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public interface IBootstrapController<TD, out TR> //: IController<D,R>
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
    }
}