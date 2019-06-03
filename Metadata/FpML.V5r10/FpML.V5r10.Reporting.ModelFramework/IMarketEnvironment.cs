
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface IMarketEnvironment
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        object GetProperty(string name);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetProperties();

        /// <summary>
        /// Gets the fp ML data.
        /// </summary>
        /// <returns></returns>
        Market GetFpMLData();

        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IPricingStructure GetPricingStructure(string name);

        /// <summary>
        /// Gets the pricing structures.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, IPricingStructure> GetPricingStructures();


        /// <summary>
        /// Gets the chidren.
        /// </summary>
        /// <returns></returns>
        IList<IMarketEnvironment> GetChildren();

        ///<summary>
        /// Returns the market.
        ///</summary>
        ///<returns></returns>
        Market GetMarket();
    }
}
