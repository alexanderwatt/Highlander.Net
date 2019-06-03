using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Base Model Controller class from which all controllers/models should be extended
    /// </summary>
    [Serializable]
    public abstract class ModelControllerBase<TD, TR> : IModelController<TD, TR>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>The model parameters.</value>
        public TD ModelData { get; protected set; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public abstract IList<string> Metrics { get; set; }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public abstract TR Calculate(TD modelData);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="analyticResults">The analytic results.</param>
        /// <returns></returns>
        protected abstract TR GetValue<T>(T analyticResults);

        ///<summary>
        ///</summary>
        protected ModelControllerBase()
        {
            Id = string.Empty;
        }
    }
}