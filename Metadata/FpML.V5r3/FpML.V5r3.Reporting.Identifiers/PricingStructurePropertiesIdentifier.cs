using System;
using Orion.ModelFramework;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace Orion.Identifiers
{
    /// <summary>
    /// PricingStructurePropertiesId
    /// </summary>
    public class PricingStructurePropertiesIdentifier : IIdentifier
    {
        /// <summary>
        /// Properties
        /// </summary>
        public NamedValueSet Properties { get; set; }

        /// <summary>
        /// PricingStructureType
        /// </summary>
        public PricingStructureTypeEnum PricingStructureType { get; set; }

        /// <summary>
        /// GetProperties
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetProperties()
        {
            return Properties;
        }

        /// <summary>
        /// UniqueIdentifier
        /// </summary>
        public String UniqueIdentifier { get; set; }

        /// <summary>
        /// SourceSystem
        /// </summary>
        public String SourceSystem { get; }

        /// <summary>
        /// Id
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        public String Domain { get; }

        /// <summary>
        /// DataType
        /// </summary>
        public String DataType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IIdentifier"/> class.
        /// </summary>
        /// <param name="properties">The properties. they must include:
        /// PricingStructureType, CurveName, BuildDateTime and Algorithm.</param>
        public PricingStructurePropertiesIdentifier(NamedValueSet properties)
        {
            Properties = properties;
            try
            {
                DataType = "Properties.PricingStructures";
                SourceSystem = PropertyHelper.ExtractSourceSystem(properties);
                Domain = SourceSystem + '.' + DataType;
                Id = PropertyHelper.ExtractPropertyIdentifier(properties);
                PricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
                UniqueIdentifier = BuildUniqueId();
                PropertyHelper.Update(Properties, CurveProp.UniqueIdentifier, UniqueIdentifier);
                PropertyHelper.Update(Properties, "Domain", Domain);
            }
            catch (Exception)
            {               
                throw new Exception("Invalid pricingstrucutre property name.");
            }

        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string BuildUniqueId()
        {
            return $"{Domain}.{PricingStructureType}";
        }
    }
}