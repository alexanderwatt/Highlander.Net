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

#region Using directives

using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using Orion.Constants;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
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