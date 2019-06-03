using System;
using Orion.Constants;

namespace Orion.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IPricingStructureIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets the PricingStructureType.
        /// </summary>
        /// <value>The PricingStructureType.</value>
        PricingStructureTypeEnum PricingStructureType { get; }

        /// <summary>
        /// Gets the CurveName.
        /// </summary>
        /// <value>The CurveName.</value>
        string CurveName { get; }

        /// <summary>
        /// Gets the BuildDateTime.
        /// </summary>
        /// <value>The BuildDateTime.</value>
        DateTime BuildDateTime { get; }

        /// <summary>
        /// The base date of the pricing structure
        /// </summary>
        DateTime BaseDate { get; }
    }
}