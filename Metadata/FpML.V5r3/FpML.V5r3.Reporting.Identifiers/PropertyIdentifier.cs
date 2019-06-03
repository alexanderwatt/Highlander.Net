#region Using directives

using Orion.Constants;
using Orion.ModelFramework.Identifiers;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The PropertyIdentifier.
    /// </summary>
    public class PropertyIdentifier : Identifier, IPropertyIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string PropertyType{ get; set; }

        /// <summary>
        ///  An id for a bond.
        /// </summary>
        /// <param name="propertyType">The property Type. </param>
        /// <param name="city">The city.</param>
        /// <param name="suburb">The suburb</param>
        /// <param name="streetName">THe street</param>
        /// <param name="streetIdentifier">THe street number or name of property.</param>
        public PropertyIdentifier(string propertyType, string city, string suburb, string streetName, string streetIdentifier)
            : base(BuildUniqueId(propertyType, city, suburb, streetName, streetIdentifier))
        {
            PropertyType = propertyType;
            Id = BuildId(propertyType, city, suburb, streetName, streetIdentifier);
        }

        private static string BuildUniqueId(string propertyType, string city, string suburb, string streetName, string streetIdentifier)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Property + "." + propertyType + "." + city + "." + suburb + "." + streetName + "." + streetIdentifier;
        }

        public static string BuildId(string propertyType, string city, string suburb, string streetName, string streetIdentifier)
        {
            return propertyType + "." + city + "." + suburb + "." + streetName + "." + streetIdentifier;
        }
    }
}