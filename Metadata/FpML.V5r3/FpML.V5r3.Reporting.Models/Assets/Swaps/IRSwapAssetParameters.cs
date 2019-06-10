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

using System.Collections.Generic;

namespace Orion.Models.Assets.Swaps
{
    public class IRSwapAssetParameters : IIRSwapAssetParameters
    {
        public string[] Metrics { get; set; }

        #region Implementation of IIRSwapAssetParameters

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public double StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public double Rate { get; set; }

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        public decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flags.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public List<bool> IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the notionals.
        /// </summary>
        /// <value>The notional.</value>
        public List<double> Leg1Notionals { get; set; }

        /// <summary>
        /// Gets or sets the notionals.
        /// </summary>
        /// <value>The notional.</value>
        public List<double> Leg2Notionals { get; set; }

        /// <summary>
        /// Gets or sets the upfront payment.
        /// </summary>
        /// <value>The upfront payment.</value>
        public double Leg1UpFrontPayment { get; set; }

        /// <summary>
        /// Gets or sets the upfront payment.
        /// </summary>
        /// <value>The upfront payment.</value>
        public double Leg2UpFrontPayment { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        public List<double> Leg1PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the forecast factors.
        /// </summary>
        /// <value>The forecast factors.</value>
        public List<double> Leg1ForecastDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double> Leg1ForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double> Leg2ForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        public List<double> Leg2PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the forecast factors.
        /// </summary>
        /// <value>The forecast factors.</value>
        public List<double> Leg2ForecastDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        public List<double> Leg1YearFractions { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        public List<double> Leg2YearFractions { get; set; }

        /// <summary>
        /// Gets or sets the spreads.
        /// </summary>
        /// <value>The spreads.</value>
        public List<double> Leg1Spreads { get; set; }

        /// <summary>
        /// Gets or sets the spreads.
        /// </summary>
        /// <value>The spreads.</value>
        public List<double> Leg2Spreads { get; set; }

        #endregion
    }
}