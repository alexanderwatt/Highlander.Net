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

using System;
using Orion.Analytics.Options;



namespace Orion.Analytics.Equities
{
    public class BinaryEuro
    {
        public BinaryEuro(double spot, double strike, Boolean isCall, double tau, double vol, 
                           int[] rtdays, double[] rtamts, int[] divdays, double[] divamts, double skew)
        {
            _Spot = spot;
            _Strike = strike;
            _isCall = isCall;
            _tau = tau;
            _vol = vol;            
            _ratedays = rtdays;
            _rateamts = rtamts;
            _divdays = divdays;
            _divamts = divamts;
            _skew = skew;                 
        }

        #region Private Members

        /// <summary>
        /// Spot price
        /// </summary>
        private double _Spot;     
        /// <summary>
        /// Pay off style Call or Put
        /// </summary>
        private bool _isCall;
        /// <summary>
        /// Strike
        /// </summary>
        private double _Strike;                
        /// <summary>
        /// Continuously compounded zero curve days
        /// </summary>
        private int[] _ratedays;
        /// <summary>
        /// Continuously compounded zero curve rates
        /// </summary>
        private double[] _rateamts;
        /// <summary>
        /// Dividend strip days
        /// </summary>
        private int[] _divdays;
        /// <summary>
        /// Dividend strip divs
        /// </summary>
        private double[] _divamts;
        /// <summary>
        /// Volatility of the option
        /// </summary>
        private double _vol;        
        /// <summary>
        /// Time to expiry
        /// </summary>
        private double _tau;        
        /// <summary>
        /// Skew
        /// </summary>
        private double _skew;

        #endregion

        /// <summary>
        /// Flats the skew price.
        /// </summary>
        /// <param name="fwd">The FWD.</param>
        /// <param name="r">The r.</param>
        /// <param name="q">The q.</param>
        /// <returns></returns>
        private double FlatSkewPrice(double fwd, double r, double q)
        {                                                
            double[] p = OptionAnalytics.DigitalWithGreeks(_isCall,fwd,_Strike,_vol,_tau);
            return p[0];            
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            double fwd = EquityAnalytics.GetForwardCCLin365(_Spot, _tau, _divdays, _divamts, _ratedays, _rateamts);
            double r = EquityAnalytics.GetRateCCLin365(0,_tau,_ratedays,_rateamts);
            double q = EquityAnalytics.GetYieldCCLin365(_Spot,0,_tau,_divdays,_divamts,_ratedays,_rateamts);
            double df = EquityAnalytics.GetDFCCLin365(0, _tau, _ratedays, _rateamts);
            double flatskewPrice = df*FlatSkewPrice(fwd,r,q);
            double[] res = OptionAnalytics.OptWithGreeks(true, fwd, _Strike, _vol, _tau);
            double callvega = res[3];
            double price;
            if (_isCall)
                price = flatskewPrice - _skew * df*callvega ;
            else
                price = flatskewPrice + _skew * df* callvega ;
            return price;
        }
    }
}
