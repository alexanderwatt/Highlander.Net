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
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Lattices
{
    /// <summary>
    /// Proportional dividend binomial tree. In this tree the dividends in escrow
    /// are represented as a proportion of the stock price at the node point. This 
    /// means at higher simulated prices on the higher nodes we have a larger dividend 
    /// than lower simulated prices on nodes lower down. 
    /// </summary>
    public class EquityPropDivTree : EquityBinomialTree
    {
            
        /// <summary>
        /// Initializes a new instance of the <see cref="EquityPropDivTree"/> class.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="t">The t.</param>
        /// <param name="vol">The vol.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        public EquityPropDivTree(double spot, double t, double vol, int steps, bool flag, int[] ratedays, double[] rateamts, int[] divdays, double[] divamts)
            : base(steps)
        {
            _tau = t;
            _sig = vol;
            _spot = spot;
            _gridsteps = steps;
            _flatFlag = flag;
            _ratedays = ratedays;
            _rateamts = rateamts;
            _divdays = divdays;
            _divamts = divamts;
            MakeGrid(ratedays, rateamts, divdays, divamts);
        }

        #region Private Members

        private readonly int _gridsteps;
        private readonly double _tau;
        private readonly double _sig;
        private readonly double _spot;
        private double _spot0;
        private double[,] _spotMatrix;
        private double[] _up;
        private double[] _dn;
        private double[] _r;
        private double[] _div;
        private double[] _divtime;
        private double[] _ratio;
        private readonly bool _flatFlag;
        private int daybasis = 365;
        private readonly int[] _ratedays;
        private readonly double[] _rateamts;
        private int[] _divdays;
        private double[] _divamts;

        #endregion
      

        //public MakeGrid
        private void MakeGrid(int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
            EmptyArrays();
            MakeArrays();
            MakeSpotZero(rtdays,rtamts, divdays,divamts);
            MakeDivArray(rtdays,rtamts, divdays,divamts);
            FillForwardRate(rtdays,rtamts);
            FillUpDown(_sig);
            double deltaT = _tau/_gridsteps;
            //make the spot grid      
          _spotMatrix[0, 0]= _spot0 ;
          //now march forward in time
            for (int idx = 1; idx <= _gridsteps; idx++)
            for (int jdx = 0; jdx <= idx; jdx++)
            {
                var sv = _spot0 * Math.Pow(_up[idx - 1], jdx) * Math.Pow(_dn[idx - 1], idx - jdx);
                //sv += (idx == Gridsteps) ? 0.0 : get_div(idx);
                //get_div(idx);
                _spotMatrix[idx, jdx] = sv;
            }
            for (int idx=1;idx<=_gridsteps;idx++)
            {
                for (int jdx=0; jdx<=idx;jdx++)
                {
                    int kdx = divdays.Length-1;
                    while (kdx>=0)
                    {
                        double dt0 = Convert.ToDouble(divdays[kdx])/daybasis;
                        if ( (dt0 >  idx*deltaT) & (dt0 <= _tau) )                          
                            _spotMatrix[idx,jdx] = _spotMatrix[idx,jdx]/(1-_ratio[kdx]);
                        kdx--;
                    }
                }
            }
            _spotMatrix[0, 0] = _spot;
        }

        #region Private Methods

        //create grid
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
        private void EmptyArrays()
        {
          _spotMatrix = null;
          _up = null;
          _dn = null;
          _div = null;
        }

        //fill grid forward rates
        private void FillForwardRate(int[] ratedays, double[] rateamts)
        {
            double dt = _tau / _gridsteps;
            int taudays = Convert.ToInt16(Math.Floor(_tau*daybasis));
            double flatRate=0.0;
            if (_flatFlag)
                flatRate = ForwardRate(0, _tau);
            for (int idx = 0; idx < _gridsteps; idx++)
            {
                if (_flatFlag)
                    _r[idx]= flatRate;
                else
                {                   
                    double fwdrate = ForwardRate(idx * dt , (idx + 1) * dt);
                    _r[idx] = fwdrate;
                }
            }          
        }

        //create the up/down arrays
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
              _dn[idx]= 1.0 / upval;
            }
          }
        }

        //public spotStar
        private void MakeSpotZero(int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
          _spot0 = _spot;
          _ratio = new double[divdays.Length];
          if (divdays.Length>0 && rtdays.Length>0)
          {
            for (int idx = 0; idx < divdays.Length; idx++)
            {
                double dt0 = Convert.ToDouble(divdays[idx]) / daybasis;
                if ( (dt0 <= _tau)  && (dt0 > 0) )
                {
                    double d1 = divamts[idx];
                    double r1 = ForwardRate(0.0,dt0);                
                    double t1 = Math.Exp(-r1 *dt0);
                    _ratio[idx]  = d1*t1/_spot0;
                    if (_ratio[idx] > 1)
                        throw new Exception ("Dividend greater than spot");
                    _spot0 = _spot0 * (1-_ratio[idx]);
                }
            }
          }
        }

        //public spotStar
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
                            double fwdrate = ForwardRate(idx * dt, dt0);
                            temp += divamts[kdx]*Math.Exp(- fwdrate *(dt0 - idx * dt));
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
              _div[idx]= 0.0;
              _divtime[idx]= dt * idx;
            }
          }      
        }

        private double ForwardRate(int days1, int days2)
        {
            int n = _ratedays.Length;
            double[] rtYrfrac = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                rtYrfrac[idx] = Convert.ToDouble(_ratedays[idx]) / daybasis;
            }
            double yearFrac1 = Convert.ToDouble(days1) / daybasis;
            double yearFrac2 = Convert.ToDouble(days2) / daybasis;
            var interpObj = LinearInterpolation.Interpolate(rtYrfrac, _rateamts);
            var fd1 = interpObj.ValueAt(yearFrac1, true);
            var fd2 = interpObj.ValueAt(yearFrac2, true);
            if (yearFrac2 == yearFrac1)
                return 0;
            return (fd2 * yearFrac2 - fd1 * yearFrac1) / (yearFrac2 - yearFrac1);
        }

        private double ForwardRate(double time1, double time2)
        {
            int n = _ratedays.Length;
            double[] rtYrfrac = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                rtYrfrac[idx] = Convert.ToDouble(_ratedays[idx]) / daybasis;
            }            
            var interpObj = LinearInterpolation.Interpolate(rtYrfrac, _rateamts);
            var fd1 = interpObj.ValueAt(time1, true);
            var fd2 = interpObj.ValueAt(time2, true);
            if (time1 == time2)
                return 0;
            return (fd2 * time2 - fd1 * time1) / (time2 - time1);
        }


    #endregion

        #region EquityBinomialTree implementation

        public override double Probability(int i, int index, int branch)
        {
            double r = Rate(i);
            double dt = _tau / Columns;
            double temp = (Math.Exp(r * dt) - _dn[i]) / (_up[i] - _dn[i]);
            return temp;
        }

        // index = number of moves 
        public override double Underlying(int i, int index)
        {
            //int j = (2 * index - i);
            //return x0 + j * dx;
            return _spotMatrix[i, index];
        }

        /// <summary>
        /// Dividends the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public override double Dividend(int index)
        {
            return _div[index];
        }

        //get forward rate item
        public override double Rate(int idx)
        {
            return idx < _gridsteps ? _r[idx] : 0.0;
        }

        /// <summary>
        /// Flat volatility on tree
        /// </summary>
        /// <returns></returns>
        public override double Volatility => _sig;

        /// <summary>
        /// Flat volatility on tree
        /// </summary>
        /// <returns></returns>
        public override double Time => _tau;

        #endregion 
    }
}
