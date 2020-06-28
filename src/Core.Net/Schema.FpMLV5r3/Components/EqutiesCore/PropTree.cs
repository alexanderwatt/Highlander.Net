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

namespace Highlander.Equities
{

    /// <summary>
    /// 
    /// </summary>
    public class PropTree : ITree
    {
        #region Private Members

        private double _spot0;
        private double[,] _spotMatrix;
        private double[] _up;
        private double[] _dn;
        private double[] _r;
        private double[] _div;
        private double[] _divTimes;
        private double[] _ratio;

        /// <summary>
        /// 
        /// </summary>
        public PropTree()
        {
            GridSteps = 0;
        }

          /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="vol"></param>
        /// <param name="spot"></param>
        /// <param name="steps"></param>
        /// <param name="flag"></param>
        public PropTree(double t, double vol, double spot, int steps, bool flag)
        {
          Tau = t;
          Sig = vol;
          Spot = spot;
          GridSteps = steps;
          FlatFlag = flag;
        }


        #endregion

        #region Accessors

    //set the step size
      /// <summary>
      /// 
      /// </summary>
      public int GridSteps { get; set; }


      //set the volatility
      /// <summary>
      /// 
      /// </summary>
      public double Sig { get; set; }


      //set the spot
      /// <summary>
      /// 
      /// </summary>
      public double Spot { get; set; }

      //set the time step size
    /// <summary>
    /// 
    /// </summary>
    public double Tau { get; set; }

      //get forward rate item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetR(int idx)
    {
        return idx < GridSteps ? _r[idx] : 0.0;
    }

    //set the forward rate
    /// <summary>
    /// 
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

    //set the div rate
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    /// <param name="t"></param>
    public void SetDiv(int idx, double value, double t)
    {
      _div[idx] = value;
      _divTimes[idx] = t;
    }

    //get div rate item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetDiv(int idx)
    {
      return _div[idx];
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

    //get div rate item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetDivTime(int idx)
    {
      return _divTimes[idx];
    }

    //get up item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetUp(int idx)
    {
        return idx < GridSteps ? _up[idx] : 0.0;
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
        return idx < GridSteps ? _dn[idx] : 0.0;
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
      public bool FlatFlag { get; set; } = true;

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
    /// 
    /// </summary>
    /// <param name="myZero"></param>
    /// <param name="myDivList"></param>
    public void MakeGrid(ZeroCurve myZero, DivList myDivList)
    {
        EmptyArrays();
        MakeArrays();
        MakeSpotZero(myZero, myDivList);
        MakeDivArray(myZero, myDivList);
        FillForwardRate(myZero);
        FillUpDown(Sig);
        decimal deltaT = Convert.ToDecimal(Tau)/Convert.ToDecimal(GridSteps);
        //make the spot grid      
      SetSpotMatrix(0, 0, _spot0) ;
      //now march forward in time
      for (int idx = 1; idx <= GridSteps; idx++)
      {
        for (int jdx = 0; jdx <= idx; jdx++)
        {
            var sv = _spot0 * Math.Pow(GetUp(idx - 1), jdx) * Math.Pow(GetDn(idx - 1), idx - jdx);
            //sv += (idx == GridSteps) ? 0.0 : get_div(idx);
           //get_div(idx);
          SetSpotMatrix(idx, jdx, sv);
        }
      }
        for (int idx=1;idx<=GridSteps;idx++)
        {
            for (int jdx=0; jdx<=idx;jdx++)
            {
                int kdx = myDivList.DivPoints-1;
                while (kdx>=0)
                {
                    decimal t1 = Convert.ToDecimal(myDivList.GetT(kdx));
                    if (t1/idx >  deltaT)                       
                       SetSpotMatrix(idx,jdx,GetSpotMatrix(idx,jdx)/(1-_ratio[kdx]));
                    kdx--;
                }
            }
        }
        SetSpotMatrix(0, 0, Spot);
    }

    /// <summary>
    /// Makes the grid.
    /// </summary>
    public void MakeGrid()
    {
        throw new NotImplementedException();
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
        _divTimes = new double[GridSteps];       
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
    private void FillForwardRate(ZeroCurve myZero)
    {
      double dt = Tau / GridSteps;
      if (myZero != null)
      {
        if (FlatFlag)
          FlatRate = myZero.LinInterp(Tau);
        for (int idx = 0; idx < GridSteps; idx++)
        {
            SetR(idx, FlatFlag ? FlatRate : myZero.ForwardRate(idx * dt, (idx + 1) * dt));
        }
      }
    }

    //create the up/down arrays
    private void FillUpDown(double sig)
    {
      if (_up != null)
      {
        double dt = Tau / GridSteps;
        for (int idx = 0; idx < GridSteps; idx++)
        {
          double a = (1.0 + Math.Exp((2 * GetR(idx) + sig * sig) * dt));
          double upVal = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * GetR(idx) * dt)))
              / (2.0 * Math.Exp(GetR(idx) * dt));
          SetUp(idx, upVal);
          SetDn(idx, 1.0 / upVal);
        }
      }
    }

    //public spotStar
    private void MakeSpotZero(ZeroCurve myZero, DivList myDivList)
    {
      _spot0 = Spot;
      _ratio = new double[myDivList.DivPoints];
      if (myZero != null)
      {
        for (int idx = 0; idx < myDivList.DivPoints; idx++)
        {
          if (myDivList.GetT(idx) <= Tau)
          {
            double d1 = myDivList.GetD(idx);
            double r1 = myZero.LinInterp(myDivList.GetT(idx));
            double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
            _ratio[idx]  = d1*t1/_spot0;
            if (_ratio[idx] > 1)
                throw new Exception ("Dividend greater than spot");
            _spot0 = _spot0 * (1-_ratio[idx]);
          }
        }
      }
    }

    //public spotStar
    private void MakeDivArray(ZeroCurve myZero, DivList myDivList)
    {
      double dt = Tau / GridSteps;
      if (myDivList != null && myZero != null)
      {
        for (int idx = 0; idx < GridSteps; idx++)
        {
          double temp = 0.0;
          for (int kdx = 0; kdx < myDivList.DivPoints; kdx++)
          {
              if ((myDivList.GetT(kdx) > idx * dt) & (myDivList.GetT(kdx) <= Tau) )
              {

                  temp += myDivList.GetD(kdx)*  Math.Exp(-myZero.ForwardRate(idx * dt, myDivList.GetT(kdx)) *
                                                         (myDivList.GetT(kdx) - idx * dt));
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

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
