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

using Orion.ModelFramework;
using Orion.Util.NamedValues;

#endregion

namespace Orion.Identifiers
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