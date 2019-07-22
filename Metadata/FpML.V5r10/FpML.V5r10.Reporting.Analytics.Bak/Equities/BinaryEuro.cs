using System;
using Orion.Analytics.Options;

namespace Orion.Analytics.Equities
{
    public class BinaryEuro
    {
        public BinaryEuro(double spot, double strike, Boolean isCall, double tau, double vol, 
                           int[] rtdays, double[] rtamts, int[] divdays, double[] divamts, double skew)
        {
            _spot = spot;
            _strike = strike;
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
        private readonly int[] _ratedays;
        /// <summary>
        /// Continuously compounded zero curve rates
        /// </summary>
        private readonly double[] _rateamts;
        /// <summary>
        /// Dividend strip days
        /// </summary>
        private readonly int[] _divdays;
        /// <summary>
        /// Dividend strip divs
        /// </summary>
        private readonly double[] _divamts;
        /// <summary>
        /// Volatility of the option
        /// </summary>
        private readonly double _vol;        
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
        /// <param name="fwd">The FWD.</param>
        /// <param name="r">The r.</param>
        /// <param name="q">The q.</param>
        /// <returns></returns>
        private double FlatSkewPrice(double fwd, double r, double q)
        {                                                
            double[] p = OptionAnalytics.DigitalWithGreeks(_isCall,fwd,_strike,_vol,_tau);
            return p[0];            
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _ratedays, _rateamts);
            double r = EquityAnalytics.GetRateCCLin365(0,_tau,_ratedays,_rateamts);
            double q = EquityAnalytics.GetYieldCCLin365(_spot,0,_tau,_divdays,_divamts,_ratedays,_rateamts);
            double df = EquityAnalytics.GetDFCCLin365(0, _tau, _ratedays, _rateamts);
            double flatskewPrice = df*FlatSkewPrice(fwd,r,q);
            double[] res = OptionAnalytics.OptWithGreeks(true, fwd, _strike, _vol, _tau);
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
