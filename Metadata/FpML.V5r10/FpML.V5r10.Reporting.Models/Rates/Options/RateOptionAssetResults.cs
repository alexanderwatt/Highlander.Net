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

using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.Models.Rates.Options
{
    public class RateOptionAssetResults : IRateOptionAssetResults
    {
        #region Implementation of IRateOptionAssetResults

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal ImpliedStrike { get; set; }

        /// <summary>
        /// Gets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        public decimal FlatVolatility { get; set; }

        /// <summary>
        /// Gets the forward rates have been calculated from the discount ffactors provided.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double> ForwardRates { get; set; }

        /// <summary>
        /// Gets the npvs.
        /// </summary>
        /// <value>The pvs.</value>
        public List<double> NPV { get; set; }

        /// <summary>
        /// Gets the implied quotes.
        /// </summary>
        /// <value>The implied quotes.</value>
        public List<double> ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public List<double> ExpectedValue { get; set; }

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public List<double> RawValue { get; set; }

        /// <summary>
        /// Gets the accrual factors.
        /// </summary>
        /// <value>The accrual factors.</value>
        public List<double> AccrualFactor { get; set; }

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        public Decimal DeltaR { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public List<double> Delta0 { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public List<double> Delta1 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        public List<double> Gamma0 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public List<double> Gamma1 { get; set; }

        /// <summary>
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        public List<double> Vega0 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        public List<double> Theta0 { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public List<double> VolatilityAtExpiry { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public List<double> MarketQuote { get; set; }

        #endregion
    }
}