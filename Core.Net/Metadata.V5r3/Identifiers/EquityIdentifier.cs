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
    /// The RateCurveIdentifier.
    /// </summary>
    public class EquityIdentifier : Identifier, IEquityIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string MarketSector { get; set; }

        /// <summary>
        ///  An id for a bond.
        /// </summary>
        /// <param name="ticker">The bond ticker. </param>
        /// <param name="pricingSource">The pricing source is required for uniqueness. For example, AU for Australian shares.</param>
        public EquityIdentifier(string ticker, string pricingSource)
            : base(BuildUniqueId(ticker, pricingSource))
        {
            MarketSector = MarketSectorEnum.Equity.ToString();
            Id = BuildId(ticker, pricingSource);
        }

        private static string BuildUniqueId(string ticker, string pricingSource)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Equity + "." + ticker + "." + pricingSource;
        }

        public static string BuildId(string ticker, string pricingSource)
        {
            return MarketSectorEnum.Equity + "." + ticker + "." + pricingSource;
        }
    }
}