/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// AssetConfigurationId
    /// </summary>
    public class AssetConfigurationIdentifier : Identifier
    {
        /// <summary>
        /// AssetRef
        /// </summary>
        public String AssetRef { get; set; }

        /// <summary>
        /// SourceSystem
        /// </summary>
        public String SourceSystem { get; }

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
        public AssetConfigurationIdentifier(NamedValueSet properties)
            : base(properties)
        {
            try
            {
                DataType = "Configuration.Asset";
                SourceSystem = PropertyHelper.ExtractSourceSystem(properties);
                Domain = SourceSystem + '.' + DataType;
                Id = PropertyHelper.ExtractPropertyIdentifier(properties);
                AssetRef = PropertyHelper.ExtractAssetRef(properties);
                UniqueIdentifier = BuildUniqueId();
                PropertyHelper.Update(Properties, CurveProp.UniqueIdentifier, UniqueIdentifier);
                PropertyHelper.Update(Properties, "Domain", Domain);
            }
            catch (System.Exception)
            {               
                throw new System.Exception("Invalid pricingstrucutre property name.");
            }

        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string BuildUniqueId()
        {
            return $"{Domain}.{AssetRef}";
        }
    }
}