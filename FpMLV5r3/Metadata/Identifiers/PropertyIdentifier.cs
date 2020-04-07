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

#region Using directives

using Highlander.Constants;
using Highlander.Reporting.ModelFramework.V5r3.Identifiers;

#endregion

namespace Highlander.Reporting.Identifiers.V5r3
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
        public string PropertyType { get; set; }

        /// <summary>
        ///  An id for a bond.
        /// </summary>
        /// <param name="propertyType">The property Type. </param>
        /// <param name="city">The city.</param>
        /// <param name="postcode">The postcode</param>
        /// <param name="shortName">A short descriptive name</param>
        /// <param name="identifier">An identifier.</param>
        public PropertyIdentifier(PropertyType propertyType, string city, string postcode, string shortName, string identifier)
            : base(BuildUniqueId(propertyType, city, postcode, shortName, identifier))
        {
            PropertyType = propertyType.ToString();
            Id = BuildId(propertyType, city, postcode, shortName, identifier);
        }

        private static string BuildUniqueId(PropertyType propertyType, string city, string postcode, string shortName, string identifier)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Property + "." + propertyType+ "." + city + "." + postcode + "." + shortName + "." + identifier;
        }

        public static string BuildId(PropertyType propertyType, string city, string postcode, string shortName, string identifier)
        {
            return propertyType + "." + city + "." + postcode + "." + shortName + "." + identifier;
        }
    }
}