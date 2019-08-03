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

using System;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Lattices
{
    /// <summary>
    /// Discrete dividend binomial tree. In this tree the dividends are assumed to be flat
    /// at all node points.  
    /// </summary>
    public class EquityDiscreteDivTree : EquityBinomialTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EquityDiscreteDivTree"/> class.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="t">The t.</param>
        /// <param name="vol">The vol.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        /// <param name="ratedays">The rate days.</param>
        /// <param name="rateamts">The rate amounts.</param>
        /// <param name="divdays">The dividend days.</param>
        /// <param name="divamts">The dividend amounts.</param>
        public EquityDiscreteDivTree(double spot, double t, double vol, int steps, bool flag, int[] ratedays, double[] rateamts, int[] divdays, double[] divamts)
            : base(steps)
        {
            _tau = t;
            _sig = vol;
            _spot = spot;
            _gridsteps = steps;
            _flatFlag = flag;
            _ratedays=ratedays;
            _rateamts=rateamts;
            _divdays=divdays;
            _divamts = divamts;
            MakeGrid(ratedays, rateamts, divdays, divamts);
        }

        #region Private Members

        /// <summary>
        /// Grid steps
        /// </summary>
        private readonly int _gridsteps;

        /// <summary>
        /// Time to expiry
        /// </summary>
        private readonly double _tau;

        /// <summary>
        /// Sigma
        /// </summary>
        private readonly double _sig;

        /// <summary>
        /// Spot
        /// </summary>
        private readonly double _spot;

        /// <summary>
        /// Spot* - spot less present value of dividends in escrow
        /// </summary>
        private double _spotstar;

        /// <summary>
        /// Asset tree
        /// </summary>
        private double[,] _spotMatrix;

        /// <summary>
        /// Vector of up factors at each nodepoint
        /// </summary>
        private double[] _up;

        /// <summary>
        /// Vector of dn factors at each nodepoint
        /// </summary>
        private double[] _dn;

        /// <summary>
        /// Vector of forward rates at each nodepoint
        /// </summary>
        private double[] _r;

        /// <summary>
        /// Div amounts at each node point
        /// </summary>
        private double[] _div;

        /// <summary>
        /// Div time progressed at each node point
        /// </summary>
        private double[] _divtime;

        /// <summary>
        /// Rate curve - days
        /// </summary>
        private readonly int[] _ratedays;

        /// <summary>
        /// Rate curve - continuously compounded rate amounts
        /// </summary>
        private readonly double[] _rateamts;

        /// <summary>
        /// Dividend strip - days
        /// </summary>
        private int[] _divdays;

        /// <summary>
        /// Dividend strip - dividend amounts
        /// </summary>
        private double[] _divamts;

        /// <summary>
        /// Flat interest rate used across tree if applicable
        /// </summary>
        private double _flatRate;

        /// <summary>
        /// Use flat forward rate at every node point on tree
        /// </summary>
        private readonly bool _flatFlag = true;

        /// <summary>
        /// Day basis
        /// </summary>
        private int daybasis = 365;

        #endregion

        #region Private Methods        


        //public MakeGrid
        /// <summary>
        /// Makes the grid.
        /// </summary>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        private void MakeGrid( int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
            EmptyArrays();
            MakeArrays();
            MakeSpotStar(rtdays, rtamts, divdays, divamts);
            MakeDivArray(rtdays, rtamts, divdays, divamts);
            FillForwardRate(rtdays, rtamts);
            FillUpDown(_sig);
            //make the spot grid
            double sv = _spotstar;
            _spotMatrix[0, 0]= sv + Dividend(0);
            //now march forward in time
            for (int idx = 1; idx <= _gridsteps; idx++)
            {
                for (int jdx = 0; jdx <= idx; jdx++)
                {
                    sv = _spotstar * Math.Pow(_up[idx - 1], jdx) * Math.Pow(_dn[idx - 1], idx - jdx);
                    sv += (idx == _gridsteps) ? 0.0 : Dividend(idx);
                    //get_div(idx);
                    _spotMatrix[idx, jdx]= sv;
                }
            }
        }          

        //create grid
        /// <summary>
        /// Makes the arrays.
        /// </summary>
        private void MakeArrays()
        {
            if (_spotMatrix == null)
            {
                _spotMatrix = new double[_gridsteps + 1, _gridsteps + 1];
                _up = new double[_gridsteps];
                _dn = new double[_gridsteps];
                _r = new double[_gridsteps];
                _div = new double[_gridsteps];
                _divtime = new double[_gridsteps];
            }
        }

        //empty grid
        /// <summary>
        /// Empties the arrays.
        /// </summary>
        private void EmptyArrays()
        {
            _spotMatrix = null;
            _up = null;
            _dn = null;
            _div = null;
        }

        //fill grid forward rates
        /// <summary>
        /// Fills the forward rate.
        /// </summary>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        private void FillForwardRate(int[] rtdays, double[] rtamts)
        {
           double dt = _tau / _gridsteps;         
           if (_flatFlag)
               _flatRate = ForwardRate(0,_tau);
           for (int idx = 0; idx < _gridsteps; idx++)
           {
               if (_flatFlag)
                   _r[idx]= _flatRate;
               else
               {                             
                   double fwdrate = ForwardRate(idx*dt,(idx+1)*dt);
                   _r[idx]= fwdrate;
               }
           }          
        }

        //create the up/down arrays
        /// <summary>
        /// Fills up down.
        /// </summary>
        /// <param name="sig">The sig.</param>
        private void FillUpDown(double sig)
        {
            if (_up != null)
            {
                double dt = _tau / _gridsteps;
                for (int idx = 0; idx < _gridsteps; idx++)
                {
                    double a = (1.0 + Math.Exp((2 * Rate(idx) + sig * sig) * dt));
                    double upval = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * Rate(idx) * dt)))
                        / (2.0 * Math.Exp(Rate(idx) * dt));
                    _up[idx]= upval;
                    _dn[idx] = 1.0 / upval;
                }
            }
        }

        //public spotStar
        /// <summary>
        /// Makes the spot star.
        /// </summary>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        private void MakeSpotStar(int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
            _spotstar = _spot;
            for (int idx = 0; idx <divdays.Length; idx++)
            {
                double dt0 = Convert.ToDouble(divdays[idx]) / daybasis;
                if ( (dt0>0) & (dt0 <= _tau) )
                {
                    double d1 = divamts[idx];                  
                    double r1 = ForwardRate(0,dt0);
                    double t1 = Math.Exp(-r1 * dt0);
                    _spotstar -= d1 * t1;
                }
            }
           
        }

        //public spotStar
        /// <summary>
        /// Makes the div array.
        /// </summary>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        private void MakeDivArray(int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
            double dt = _tau / _gridsteps;
            if (divdays.Length>0 && rtdays.Length>0)
            {
                for (int idx = 0; idx < _gridsteps; idx++)
                {
                    double temp = 0.0;
                    for (int kdx = 0; kdx < divdays.Length; kdx++)
                    {
                        double dt0 = Convert.ToDouble(divdays[kdx]) / daybasis;
                        if ((dt0 > idx * dt) && (dt0 <= _tau))
                        {
                            double fwdrate = ForwardRate(idx*dt, dt0);
                            temp += divamts[kdx] * Math.Exp(-fwdrate * (dt0 - idx * dt));
                        }
                    }
                    _div[idx] = temp;
                    _divtime[idx]= dt * idx;
                }
            }                      
            else  //missing either div or zero, load in 0 for _div on tree
            {
                for (int idx = 0; idx <= _gridsteps; idx++)
                {
                    _div[idx] = 0.0;
                    _divtime[idx] = dt * idx;                    
                }
            }
        }

        /// <summary>
        /// Forwards the rate.
        /// </summary>
        /// <param name="days1">The days1.</param>
        /// <param name="days2">The days2.</param>
        /// <returns></returns>
        private double ForwardRate(int days1, int days2)
        {
            int n = _ratedays.Length;
            double[] rt_yrfrac = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                rt_yrfrac[idx] = System.Convert.ToDouble(_ratedays[idx]) / daybasis;
            }
            double yearFrac1 = System.Convert.ToDouble(days1) / daybasis;
            double yearFrac2 = System.Convert.ToDouble(days2) / daybasis;
            var interpObj = LinearInterpolation.Interpolate(rt_yrfrac, _rateamts);
            var fd1 = interpObj.ValueAt(yearFrac1, true);
            var fd2 = interpObj.ValueAt(yearFrac2, true);
            if (yearFrac2 == yearFrac1)
                return 0;
            return (fd2 * yearFrac2 - fd1 * yearFrac1) / (yearFrac2 - yearFrac1);
        }

        /// <summary>
        /// Forwards the rate.
        /// </summary>
        /// <param name="time1">The time1.</param>
        /// <param name="time2">The time2.</param>
        /// <returns></returns>
        private double ForwardRate(double time1, double time2)
        {
            int n = _ratedays.Length;
            double[] rt_yrfrac = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                rt_yrfrac[idx] = System.Convert.ToDouble(_ratedays[idx]) / daybasis;
            }
            var interpObj = LinearInterpolation.Interpolate(rt_yrfrac, _rateamts);
            var fd1 = interpObj.ValueAt(time1, true);
            var fd2 = interpObj.ValueAt(time2, true);
            if (time1 == time2)
                return 0;
            return (fd2 * time2 - fd1 * time1) / (time2 - time1);
        }


        #endregion

        #region EquityBinomialTree implementation

        /// <summary>
        /// Probability of an up-movement i.e from node (index,i) to (index+1,i+1)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public override double Probability(int i, int index, int branch)
        {
            double r = Rate(i);
            double dt = System.Convert.ToDouble(_tau) / Columns;
            double temp = (Math.Exp(r * dt) - _dn[i]) / (_up[i] - _dn[i]);
            return temp;
        }

        ///
        /// <summary>
        /// Return underlying value at
        /// </summary>
        /// <param name="i"> i = # time points progressed </param>
        /// <param name="index">index = # moves up </param>
        /// <returns></returns>
        public override double Underlying(int i, int index)
        {           
            return _spotMatrix[i, index];
        }

         
        /// <summary>
        /// PV of dividends at idx time point
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public override double Dividend(int idx)
        {
            return _div[idx];
        }

      
        /// <summary>
        /// Forward rate at idx to (idx+1)
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public override double Rate(int idx)
        {
            if (idx < _gridsteps)
            {
                return _r[idx];
            }
            return 0.0;
        }

        /// <summary>
        /// Volatility of underlying
        /// </summary>
        /// <value></value>   
        /// <returns></returns>
        public override double Volatility => _sig;


        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        public override double Time => _tau;

        #endregion
    }
}
