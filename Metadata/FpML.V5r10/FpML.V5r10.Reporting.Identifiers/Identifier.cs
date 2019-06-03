#region Using directives

using FpML.V5r10.Reporting.ModelFramework;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class Identifier : IIdentifier
    {
        /// <summary>
        /// Properties
        /// </summary>
        public NamedValueSet Properties { get; set; }

        #region Implementation of IIdentifier

        /// <summary>
        /// Gets the NameSpace.
        /// </summary>
        /// <value>The NameSpace.</value>
        public string NameSpace { get; set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the Unique Identifier.
        /// </summary>
        /// <value>The Unique Identifier.</value>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// REturns the properties relevant to this identifier.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetProperties()
        {
            return Properties;
        }

        #endregion

        ///<summary>
        /// An id.
        ///</summary>
        public Identifier()
        {}

        ///<summary>
        /// An id.
        ///</summary>
        ///<param name="id">The Id.</param>
        public Identifier(string id)
        {
            UniqueIdentifier = id;
            Id = id;
            var nvs = new NamedValueSet();
            nvs.Set("Identifier", Id);
            nvs.Set("UniqueIdentifier", Id);
        }

        ///<summary>
        /// An id.
        ///</summary>
        ///<param name="properties">The properties.</param>
        public Identifier(NamedValueSet properties)
        {
            Properties = properties;
        }
    }
}