/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Highlander.Constants;
using Highlander.Utilities.NamedValues;

namespace Highlander.Reporting.Identifiers.V5r3
{
    /// <summary>
    /// AssetConfigurationId
    /// </summary>
    public class AssetConfigurationIdentifier : Identifier
    {
        /// <summary>
        /// AssetRef
        /// </summary>
        public string AssetRef { get; set; }

        /// <summary>
        /// SourceSystem
        /// </summary>
        public string SourceSystem { get; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// DataType
        /// </summary>
        public string DataType { get; }

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
            catch (Exception)
            {               
                throw new Exception("Invalid pricing structure property name.");
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