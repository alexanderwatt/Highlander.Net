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

using FpML.V5r10.Reporting.Models.ForeignExchange.FxLeg;

#endregion

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxOptionLeg
{
    public class FxOptionLegParameters : FxLegParameters, IFxOptionLegParameters
    {
        #region Implementation of IFxOptionLegParameters

        ///// <summary>
        ///// Gets or sets the metrics.
        ///// </summary>
        ///// <value>The metrics.</value>
        //public string[] Metrics { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency1.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //public decimal ExchangedCurrency1 { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency2.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //public decimal ExchangedCurrency2 { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency1 discount factor.</value>
        //public decimal Currency1DiscountFactor { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency2 discount factor.</value>
        //public decimal Currency2DiscountFactor { get; set; }

        /// <summary>
        /// The is call flag;
        /// </summary>
        public bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public decimal? Premium { get; set; }

        #endregion
    }
}