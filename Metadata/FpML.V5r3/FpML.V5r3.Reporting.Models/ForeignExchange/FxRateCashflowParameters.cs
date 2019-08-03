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

#region Usings

using System;
using Orion.Models.Generic.Cashflows;

#endregion

namespace Orion.Models.ForeignExchange
{
    public class FxRateCashflowParameters : FloatingCashflowParameters, IFxRateCashflowParameters
    {
        /// <summary>
        /// Gets or sets the first currency.
        /// </summary>
        ///  <value>The first currency.</value>
        public String Currency1 { get; set; }

        /// <summary>
        /// Gets or sets the second currency.
        /// </summary>
        ///  <value>The second currency.</value>
        public String Currency2 { get; set; }

        /// <summary>
        /// Gets or sets the IsCurrency1Base flag.
        /// </summary>
        ///  <value>The IsCurrency1Base flag.</value>
        public bool IsCurrency1Base { get; set; }
    }
}