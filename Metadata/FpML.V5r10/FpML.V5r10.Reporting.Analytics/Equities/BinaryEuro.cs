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
using Highlander.Numerics.Options;


namespace FpML.V5r10.Reporting.Analytics.Equities
{
    public class BinaryEuro
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spot">The spot price</param>
        /// <param name="strike">The strike</param>
        /// <param name="isCall">Is the option a call?</param>
        /// <param name="tau">The time to expiry</param>
        /// <param name="volatility">The lognormal volatility</param>
        /// <param name="rateDays">The rate days array</param>
        /// <param name="rateAmounts">The rate amounts array</param>
        /// <param name="dividendDays">The dividend day array</param>
        /// <param name="dividendAmounts">THe dividend amount array</param>
        /// <param name="skew"></param>
        public BinaryEuro(double spot, double strike, Boolean isCall, double tau, double volatility,
                           int[] rateDays, double[] rateAmounts, int[] dividendDays, double[] dividendAmounts, double skew)
        {
            _spot = spot;
            _strike = strike;
            _isCall = isCall;
            _tau = tau;
            _volatility = volatility;
            _rateDays = rateDays;
            _rateAmounts = rateAmounts;
            _dividendDays = dividendDays;
            _dividendAmounts = dividendAmounts;
            _skew = skew;
        }

        #region Private Members

        /// <summary>
        /// Spot price
        /// </summary>
        private readonly double _spot;

        /// <summary>
        /// Pay off style Call or Put
        /// </summary>
        private readonly bool _isCall;

        /// <summary>
        /// Strike
        /// </summary>
        private readonly double _strike;

        /// <summary>
        /// Continuously compounded zero curve days
        /// </summary>
        private readonly int[] _rateDays;

        /// <summary>
        /// Continuously compounded zero curve rates
        /// </summary>
        private readonly double[] _rateAmounts;

        /// <summary>
        /// Dividend strip days
        /// </summary>
        private readonly int[] _dividendDays;

        /// <summary>
        /// Dividend strip divs
        /// </summary>
        private readonly double[] _dividendAmounts;

        /// <summary>
        /// Volatility of the option
        /// </summary>
        private readonly double _volatility;

        /// <summary>
        /// Time to expiry
        /// </summary>
        private readonly double _tau;

        /// <summary>
        /// Skew
        /// </summary>
        private readonly double _skew;

        #endregion

        /// <summary>
        /// Flats the skew price.
        /// </summary>
        /// <param name="forward">The FWD.</param>
        /// <param name="r">The r.</param>
        /// <param name="q">The q.</param>
        /// <returns></returns>
        private double FlatSkewPrice(double forward, double r, double q)
        {
            double[] p = OptionAnalytics.DigitalWithGreeks(_isCall, forward, _strike, _volatility, _tau);
            return p[0];
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _dividendDays, _dividendAmounts, _rateDays, _rateAmounts);
            double r = EquityAnalytics.GetRateCCLin365(0, _tau, _rateDays, _rateAmounts);
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _dividendDays, _dividendAmounts, _rateDays, _rateAmounts);
            double df = EquityAnalytics.GetDFCCLin365(0, _tau, _rateDays, _rateAmounts);
            double flatSkewPrice = df * FlatSkewPrice(fwd, r, q);
            double[] res = OptionAnalytics.OptWithGreeks(true, fwd, _strike, _volatility, _tau);
            double callVega = res[3];
            double price;
            if (_isCall)
                price = flatSkewPrice - _skew * df * callVega;
            else
                price = flatSkewPrice + _skew * df * callVega;
            return price;
        }
    }
}
