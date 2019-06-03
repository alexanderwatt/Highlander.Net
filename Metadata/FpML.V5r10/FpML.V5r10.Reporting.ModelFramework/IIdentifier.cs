using Orion.Util.NamedValues;

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IIdentifier
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets the Unique Identifier.
        /// </summary>
        /// <value>The Unique Identifier.</value>
        string UniqueIdentifier { get; set; }

        /// <summary>
        /// Returns the properties relevant to this identifier.
        /// </summary>
        /// <returns></returns>
        NamedValueSet Properties { get; set; }
    }
}
