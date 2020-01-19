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

#region Usings

using System;

#endregion

namespace FpML.V5r10.Reporting.Models.Generic.Cashflows
{
    public interface IFloatingCashflowParameters : ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        ///  <value>The start index.</value>
        Decimal? StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the floating index.
        /// </summary>
        ///  <value>The floating index.</value>
        Decimal? FloatingIndex { get; set; }

        /// <summary>
        /// Gets or sets the IsReset flag.
        /// </summary>
        ///  <value>The IsReset flag.</value>
        bool IsReset { get; set; }

        /// <summary>
        /// The ExpiryYearFraction.
        /// </summary>
        Decimal ExpiryYearFraction { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        ///  <value>The premium.</value>
        Decimal? Premium { get; set; }
    }
}