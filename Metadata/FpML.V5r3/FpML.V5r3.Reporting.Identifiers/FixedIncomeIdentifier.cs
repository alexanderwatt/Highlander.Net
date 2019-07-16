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

using System;
using Orion.Constants;
using Orion.ModelFramework.Identifiers;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class FixedIncomeIdentifier : Identifier, IFixedIncomeIdentifier
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

        ///<summary>
        /// An id for a bond.
        ///</summary>
        ///<param name="coupon">The coupon. Prefixed by F if floating and V if variable. </param>
        ///<param name="marketSector">The market sector. This is the Bloomberg designation. </param>
        ///<param name="ticker">The bond ticker. </param>
        ///<param name="maturityDate">The maturity date as a string and formatted as MM/DD/YY </param>
        ///<param name="couponType">The coupon type: fixed, float or struct. </param>
        public FixedIncomeIdentifier(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
            : base(BuildUniqueId(ticker, coupon, marketSector, maturityDate, couponType))
        {
            MarketSector = marketSector;
            Id = BuildId(ticker, coupon, marketSector, maturityDate, couponType);
        }

        private static string BuildUniqueId(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
        {
            var coup = coupon.Replace('.', ',');
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.FixedIncome + "." + marketSector + "." + ticker + "." + couponType + "." + coup + "." + maturityDate.ToString("d");
        }

        public static string BuildId(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
        {
            var coup = coupon.Replace('.', ',');
            return marketSector + "." + ticker + "." + couponType + "." + coup + "." + maturityDate.ToString("d");
        }
    }
}