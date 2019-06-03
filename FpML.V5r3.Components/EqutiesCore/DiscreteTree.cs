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

namespace Orion.EquitiesCore
{

  /// <summary>
  /// 
  /// </summary>
  public class DiscreteTree : ITree
  {
    #region Private Members

    private double _spotstar;
    private double[,] _spotMatrix;
    private double[] _up;
    private double[] _dn;
    private double[] _r;
    private double[] _div;
    private double[] _divtime;

      /// <summary>
    /// 
    /// </summary>
    public DiscreteTree()
    {
        Gridsteps = 0;
    }

      /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <param name="vol"></param>
    /// <param name="spot"></param>
    /// <param name="steps"></param>
    /// <param name="flag"></param>
    public DiscreteTree(double t, double vol, double spot, int steps, bool flag)
    {
      Tau = t;
      Sig = vol;
      Spot = spot;
      Gridsteps = steps;
      FlatFlag = flag;
    }


    #endregion

    #region Accessors

    //set the step size
      /// <summary>
      /// 
      /// </summary>
      public int Gridsteps { get; set; }


      ///set the volatility
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
        if (idx < Gridsteps)
      {
        return _r[idx];
      }
        return 0.0;
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
        if (idx < Gridsteps)
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
      _divtime[idx] = t;
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
    public double GetDivtime(int idx)
    {
      return _divtime[idx];
    }

    //get up item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetUp(int idx)
    {
        return idx < Gridsteps ? _up[idx] : 0.0;
    }

    //set the up itm
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    public void SetUp(int idx, double value)
    {
      if (idx < Gridsteps)
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
        return idx < Gridsteps ? _dn[idx] : 0.0;
    }

    //set the dn itm
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    public void SetDn(int idx, double value)
    {
      if (idx < Gridsteps)
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
      if ((idx <= Gridsteps) && (jdx <= Gridsteps))
      {
        _spotMatrix[idx, jdx] = value;
      }
    }


    //public MakeGrid
    /// <summary>
    /// 
    /// </summary>
    /// <param name="myZero"></param>
    /// <param name="myDivList"></param>
    public void MakeGrid(ZeroCurve myZero, DivList myDivList)
    {
      EmptyArrays();
      MakeArrays();
      MakeSpotStar(myZero, myDivList);
      MakeDivArray(myZero, myDivList);
      FillForwardRate(myZero);
      FillUpDown(Sig);

      //make the spot grid
      double sv = _spotstar;
      SetSpotMatrix(0, 0, sv+GetDiv(0)) ;

      //now march foraward in time
      for (int idx = 1; idx <= Gridsteps; idx++)
      {
        for (int jdx = 0; jdx <= idx; jdx++)
        {
          sv = _spotstar * Math.Pow(GetUp(idx - 1), jdx) * Math.Pow(GetDn(idx - 1), idx - jdx);
          sv += idx == Gridsteps ? 0.0 : GetDiv(idx);
           //get_div(idx);
          SetSpotMatrix(idx, jdx, sv);
        }
      }
    }

    #endregion

    #region Private Methods

    //create grid
    private void MakeArrays()
    {
      if (_spotMatrix == null)
      {
        _spotMatrix = new double[Gridsteps + 1, Gridsteps + 1];
        _up = new double[Gridsteps];
        _dn = new double[Gridsteps];
        _r = new double[Gridsteps];
        _div = new double[Gridsteps];
        _divtime = new double[Gridsteps];
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

      double dt = Tau / Gridsteps;
      if (myZero != null)
      {
        if (FlatFlag)
          FlatRate = myZero.LinInterp(Tau);
        for (int idx = 0; idx < Gridsteps; idx++)
        {
            SetR(idx, FlatFlag ? FlatRate : myZero.ForwardRate(idx * dt, (idx + 1) * dt));
        }
      }
    }

    //create the up/down arrays
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sig"></param>
    private void FillUpDown(double sig)
    {
        if (_up == null) return;
        double dt = Tau / Gridsteps;
        for (int idx = 0; idx < Gridsteps; idx++)
        {
            double a = (1.0 + Math.Exp((2 * GetR(idx) + sig * sig) * dt));
            double upval = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * GetR(idx) * dt)))
                           / (2.0 * Math.Exp(GetR(idx) * dt));
            SetUp(idx, upval);
            SetDn(idx, 1.0 / upval);
        }
    }

    //public spotStar
    private void MakeSpotStar(ZeroCurve myZero, DivList myDivList)
    {
      _spotstar = Spot;

      if (myDivList != null && myZero != null)
      {
        for (int idx = 0; idx < myDivList.Divpoints; idx++)
        {
          if (myDivList.GetT(idx) <= Tau)
          {
            double d1 = myDivList.GetD(idx);
            double r1 = myZero.LinInterp(myDivList.GetT(idx));
            double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
            _spotstar -= d1 * t1;
          }
        }
      }
    }

    //public spotStar
    /// <summary>
    /// 
    /// </summary>
    /// <param name="myZero"></param>
    /// <param name="myDivList"></param>
    private void MakeDivArray(ZeroCurve myZero, DivList myDivList)
    {
      double dt = Tau / Gridsteps;
      if ((myDivList != null) && (myZero != null))
      {
        for (int idx = 0; idx < Gridsteps; idx++)
        {
          double temp = 0.0;
          for (int kdx = 0; kdx < myDivList.Divpoints; kdx++)
          {
            if ((myDivList.GetT(kdx) > idx * dt) && (myDivList.GetT(kdx) < Tau))
            {

              temp += myDivList.GetD(kdx) * Math.Exp(-myZero.ForwardRate(idx * dt, myDivList.GetT(kdx)) *
                 (myDivList.GetT(kdx) - idx * dt));
            }
          }
          SetDiv(idx, temp, dt * idx);
        }
      }
      else  //missing either div or zero, load in 0 for _div on tree
      {
        for (int idx = 0; idx <= Gridsteps; idx++)
        {
          SetDiv(idx, 0.0, dt * idx);
        }
      }
    }

    #endregion
  }
}
