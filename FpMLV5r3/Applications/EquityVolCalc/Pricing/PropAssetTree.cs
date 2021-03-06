﻿/*
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
using System.Collections.Generic;
using System.Linq;
using Highlander.Equities;
using Highlander.Equity.Calculator.V5r3.Helpers;

namespace Highlander.Equity.Calculator.V5r3.Pricing
{
    public class PropAssetTree : ITree 
    {
        #region Private Members

        readonly DateTime _today;
        private double _spot0;
        private double[,] _spotMatrix;
        private double[] _up;
        private double[] _dn;
        private double[] _r;
        private double[] _div;
        private readonly List<Dividend> _rawDivs;
        readonly RateCurve _rawZero;
        private double[] _divtime;
        private double[] _ratio;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="vol"></param>
        /// <param name="spot"></param>
        /// <param name="steps"></param>
        /// <param name="flag"></param>
        /// <param name="today"></param>
        /// <param name="zeroCurve"></param>
        /// <param name="divs"></param>
        public PropAssetTree(double t, double vol, double spot, int steps, bool flag, DateTime today, RateCurve zeroCurve, IEnumerable<Dividend> divs)
        {
          Tau = t;
          Sig = vol;
          Spot = spot;
          GridSteps = steps;
          FlatFlag = flag;
          _rawZero = zeroCurve;
          _rawDivs = PreProcessDivs(today, divs);
          _today = today;
          EmptyArrays();
          MakeArrays();
          MakeSpotZero();
          MakeDivArray();
          FillForwardRate();                    
        }

        #endregion

        #region Accessors

        /// <summary>
        /// set the step size
        /// </summary>
        public int GridSteps { get; set; }

        /// <summary>
        /// set the volatility
        /// </summary>
        public double Sig { get; set; }

        /// <summary>
        /// set the spot
        /// </summary>
        public double Spot { get; set; }

        /// <summary>
        /// set the time step size
        /// </summary>
        public double Tau { get; set; }

        /// <summary>
        /// get forward rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetR(int idx)
        {
            if (idx < GridSteps)
            {
                return _r[idx];
            }
            return 0.0;
        }

        /// <summary>
        /// set the forward rate
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetR(int idx, double value)
        {
          if (_r != null)
          {
            if (idx < GridSteps)
            {
              _r[idx] = value;
            }
          }
        }

        /// <summary>
        /// set the div rate
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        public void SetDiv(int idx, double value, double t)
        {
          _div[idx] = value;
          _divtime[idx] = t;
        }

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDiv(int idx)
        {
          return _div[idx];
        }

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDivTime(int idx)
        {
            return _divtime[idx];
        }

        //get div rate item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetRatio(int idx)
        {
          return _ratio[idx];
        }

        /// <summary>
        /// get up item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetUp(int idx)
        {
            if (idx < GridSteps)
            {
                return _up[idx];
            }
            return 0.0;
        }

        //set the up itm
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetUp(int idx, double value)
        {
          if (idx < GridSteps)
          {
            _up[idx] = value;
          }
        }

        //get dn item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDn(int idx)
        {
            if (idx < GridSteps)
            {
                return _dn[idx];
            }
            return 0.0;
        }

        //set the dn itm
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetDn(int idx, double value)
        {
          if (idx < GridSteps)
          {
            _dn[idx] = value;
          }
        }

        /// <summary>
        /// 
        /// </summary>
        public double FlatRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool FlatFlag { get; set; }

        #endregion

        #region Public Methods

        //get SpotMatrix item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <returns></returns>
        public double GetSpotMatrix(int idx, int jdx)
        {
          return _spotMatrix[idx, jdx];
        }

        /// <summary>
        /// Makes the grid.
        /// </summary>
        /// <param name="myZero">My zero.</param>
        /// <param name="myDivList">My div list.</param>
        public void MakeGrid(ZeroCurve myZero, DivList myDivList)
        {
            throw new NotImplementedException();
        }

        //set SpotMatrix item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <param name="value"></param>
        public void SetSpotMatrix(int idx, int jdx, double value)
        {
          if ((idx <= GridSteps) && (jdx <= GridSteps))
          {
            _spotMatrix[idx, jdx] = value;
          }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        //public MakeGrid
        /// <summary>
        /// 
        /// </summary>
        public void MakeGrid()
        {
            FillUpDown(Sig);       
            double deltaT = Tau/GridSteps;
            //make the spot grid      
          SetSpotMatrix(0, 0, _spot0) ;
          //now march forward in time
          for (int idx = 1; idx <= GridSteps; idx++)
          {
            for (int jdx = 0; jdx <= idx; jdx++)
            {
                double sv = _spot0 * Math.Pow(GetUp(idx - 1), jdx) * Math.Pow(GetDn(idx - 1), idx - jdx);
                //sv += (idx == Gridsteps) ? 0.0 : get_div(idx);
               //get_div(idx);
              SetSpotMatrix(idx, jdx, sv);
            }
          }
            for (int idx=1;idx<=GridSteps;idx++)
            {
                for (int jdx=0; jdx<=idx;jdx++)
                {
                    int kdx = _rawDivs.Count-1;
                    while (kdx>=0)
                    {
                        double t1 = _rawDivs[kdx].ExDate.Subtract(_today).Days/365.0;
                        if (t1 <= idx * deltaT)
                           break;
                        SetSpotMatrix(idx,jdx,GetSpotMatrix(idx,jdx)/(1-_ratio[kdx]));
                        kdx--;
                    }
                }
            }
            SetSpotMatrix(0, 0, Spot);
        }

        #endregion

        #region Private Methods

        //create grid
        private void MakeArrays()
        {
          if (_spotMatrix == null)
          {
            _spotMatrix = new double[GridSteps + 1, GridSteps + 1];
            _up = new double[GridSteps];
            _dn = new double[GridSteps];
            _r = new double[GridSteps];
            _div = new double[GridSteps];
            _divtime = new double[GridSteps];       
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
        private void FillForwardRate()
        {
            int days = Convert.ToInt32(Tau*365);
          if (_rawZero != null)
          {
                FlatRate = -1/Tau*Math.Log(Convert.ToDouble(_rawZero.GetDf(days)));
                for (int idx = 0; idx < GridSteps; idx++)
                {          
                    SetR(idx, FlatRate);          
                }
          }
        }

        //create the up/down arrays
        private void FillUpDown(double sigma)
        {
          if (_up != null)
          {
            double dt = Tau / GridSteps;
            for (int idx = 0; idx < GridSteps; idx++)
            {
              double a = (1.0 + Math.Exp((2 * GetR(idx) + sigma * sigma) * dt));
              double upval = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * GetR(idx) * dt)))
                  / (2.0 * Math.Exp(GetR(idx) * dt));
              SetUp(idx, upval);
              SetDn(idx, 1.0 / upval);
            }
          }
        }

        /// <summary>
        /// Pres the process divs.
        /// </summary>
        /// <param name="today"></param>
        /// <param name="rawDivList">The raw div list.</param>
        /// <returns></returns>
        private List<Dividend> PreProcessDivs(DateTime today, IEnumerable<Dividend> rawDivList)
        {
            // Preprocess divs           
            DateTime expiry = today.AddDays(365.0*Tau);
            List<Dividend> divs = rawDivList.Where(div => div.ExDate > today && div.ExDate <= expiry).ToList();
            DividendHelper.Sort(divs);
            return divs;
        }

        //public spotStar
        private void MakeSpotZero()
        {         
          _spot0 = Spot;
          _ratio = new double[_rawDivs.Count];
          if ((_rawDivs != null) && (_rawZero != null))
          {
                int idx = 0;
                foreach (Dividend div in _rawDivs)
                {
                    int days = div.ExDate.Subtract(_today).Days;
                    double t0 = days / 365.0;
                    if (t0 <= Tau & t0>0)
                    {
                        double d1 = Convert.ToDouble(div.Amount);
                        double t1 =  Convert.ToDouble(_rawZero.GetDf(days));                  
                        _ratio[idx] = d1*t1/_spot0;
                        if (_ratio[idx] > 1)
                            throw new Exception ("Dividend greater than spot");
                        _spot0 = _spot0 * (1-_ratio[idx]);                        
                    }
                    idx++;                   
                }
          }         

        }

        //public spotStar
        private void MakeDivArray()
        {          
          double dt = Tau / GridSteps;
          if ((_rawDivs != null) && (_rawZero != null))
          {
            for (int idx = 0; idx < GridSteps; idx++)
            {
              double temp = 0.0;
              foreach (Dividend div in _rawDivs)
              {
                  double days = div.ExDate.Subtract(_today).Days;
                  double t0 = days / 365.0;

                    if ((t0 > idx * dt) && (t0 <= Tau))
                    {
                        int days0  = Convert.ToInt32(days - 365*idx*dt);
                        temp += Convert.ToDouble(div.Amount * _rawZero.GetDf(days0));                     
                    }
              }
              SetDiv(idx, temp, dt * idx);
            }
          }
          else  //missing either div or zero, load in 0 for _div on tree
          {
            for (int idx = 0; idx <= GridSteps; idx++)
            {
              SetDiv(idx, 0.0, dt * idx);
            }
          }
          
        }

        #endregion
    }
}