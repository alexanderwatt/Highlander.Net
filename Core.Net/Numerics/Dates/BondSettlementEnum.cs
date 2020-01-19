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

#region Using Directives

using System;

#endregion

namespace Highlander.Numerics.Dates
{
    /// <summary>
    /// Settlement/value date delay convention
    /// </summary>
    public enum BondSettlementEnum
    {
        /// <summary>no settlement delay</summary>
        SC_dealDate = 1,
        /// <summary>one business day</summary>
        SC_1bd,
        /// <summary>"two business days"</summary>
        SC_2bd,
        /// <summary>one business day plus one calendar day</summary>
        SC_1b1cd,
        /// <summary>three business days</summary>
        SC_3bd,
        /// <summary>three calendar days</summary>
        SC_3d,
        /// <summary>three business days plus one calendar day</summary>
        SC_3b1cd,
        /// <summary>four business days</summary>
        SC_4bd,
        /// <summary>five business days</summary>
        SC_5bd,
        /// <summary>seven calendar days</summary>
        SC_7d,
        /// <summary>seven calendar days plus one business day</summary>
        SC_7c1bd,
        /// <summary>three calendar days</summary>
        SC_3cd,
        /// <summary>seven business days</summary>
        SC_7bd,
        /// <summary>3 business days, but 6 b.d. lockout period before coupons</summary>
        SC_3bd_6bdLO,
        /// <summary>4 business days, but 6 b.d. lockout period before coupons</summary>
        SC_4bd_6bdLO//,
        // /// <summary>Canadian settlement: 2 b.d. for less than 3y to maturity, otherwise 3 b.d.</summary>
        // SC_Canada,
        // /// <summary>Austrian: next Monday but one</summary>
        //SC_Austria,
        // /// <summary>Australian: next b.d. if less than 5y to maturity, otherwise 7 c.d.</summary>
        // SC_Australia,
        // /// <summary>Australian: next b.d. if less than 5y to maturity, otherwise 7 c.d.</summary>
        // SC_SouthAfrica
    }
}