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
        public PropertyIdentifier(string propertyType, string city, string shortName, string postcode, string identifier)
            : base(BuildUniqueId(propertyType, city, shortName, postcode, identifier))
        {
            PropertyType = propertyType;
            Id = BuildId(propertyType, city, shortName, postcode, identifier);
        }

        private static string BuildUniqueId(string propertyType, string city, string shortName, string postcode, string identifier)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Property + "." + BuildId(propertyType, city, postcode, shortName, identifier);
        }

        public static string BuildId(string propertyType, string city, string shortName, string postcode, string identifier)
        {
            string result = null;
            if (!string.IsNullOrEmpty(propertyType))
            {
                result = propertyType;
            }
            if (!string.IsNullOrEmpty(city))
            {
                result = result + "." + city;
            }
            if (!string.IsNullOrEmpty(shortName))
            {
                result = result + "." + shortName;
            }
            if (!string.IsNullOrEmpty(postcode))
            {
                result = result + "." + postcode;
            }
            if (!string.IsNullOrEmpty(identifier))
            {
                result = result + "." + identifier;
            }
            return result;
        }
    }
}