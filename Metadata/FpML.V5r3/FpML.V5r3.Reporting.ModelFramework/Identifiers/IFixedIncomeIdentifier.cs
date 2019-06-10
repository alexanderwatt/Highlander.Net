/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Orion.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IFixedIncomeIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The market sector.
        ///</summary>
        string MarketSector { get; set; }
    }

    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IEquityIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The market sector.
        ///</summary>
        string MarketSector { get; set; }
    }

    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IPropertyIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The property type.
        ///</summary>
        string PropertyType { get; set; }
    }
}