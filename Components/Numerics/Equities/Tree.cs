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

#region Usings

using System;
using Highlander.Numerics.Rates;

#endregion

namespace Highlander.Numerics.Equities 
{
    public class Tree
    {
        #region Private Members

          public int GridSteps { get; set; }

          public double Tau { get; set; }

          public double Sig { get; set; }

          public double Spot { get; set; }

          public double SpotStart { get; set; }

          public double[,] SpotMatrix { get; set; }

          public double[] Up { get; set; }

          public double[] Dn { get; set; }

          public double[] Rate { get; set; }

          public double[] Dividends { get; set; }

          public double[] DividendTime { get; set; }

          public double FlatRate { get; set; }

          public bool FlatFlag = true;

        public Tree()
        {

        }

        public Tree(double t, double vol, double spot, int steps, bool flag)
        {
            Tau = t;
            Sig = vol;
            Spot = spot;
            GridSteps = steps;
            FlatFlag = flag;
        }

        #endregion

        #region Methods

        //get forward rate item
        public double GetR(int idx)
        {
            if (idx < GridSteps)
            {
                return Rate[idx];
            }
            return 0.0;
        }

        //set the forward rate
        public void SetRate(int idx, double value)
        {
          if (Rate != null)
          {
            if (idx < GridSteps)
            {
                Rate[idx] = value;
            }
          }
        }

        //set the div rate
        public void SetDividends(int idx, double value, double t)
        {
          Dividends[idx] = value;
          DividendTime[idx] = t;
        }

        //get div rate item
        public double GetDividends(int idx)
        {
          return Dividends[idx];
        }

        //get div rate item
        public double GetDividendTimes(int idx)
        {
          return DividendTime[idx];
        }

        //get up item
        public double GetUp(int idx)
        {
          if (idx < GridSteps)
          {
            return Up[idx];
          }
          else
          {
            return 0.0;
          }
        }

        //set the up itm
        public void SetUp(int idx, double value)
        {
          if (idx < GridSteps)
          {
            Up[idx] = value;
          }
        }

        //get dn item
        public double GetDn(int idx)
        {
          if (idx < GridSteps)
          {
            return Dn[idx];
          }
          else
          {
            return 0.0;
          }
        }

        //set the dn itm
        public void SetDn(int idx, double value)
        {
          if (idx < GridSteps)
          {
            Dn[idx] = value;
          }
        }

        #endregion

        #region Public Methods

        //get SpotMatrix item
        public double GetSpotMatrix(int idx, int jdx)
        {
          return SpotMatrix[idx, jdx];
        }

        //set SpotMatrix item
        public void SetSpotMatrix(int idx, int jdx, double value)
        {
          if ((idx <= GridSteps) && (jdx <= GridSteps))
          {
            SpotMatrix[idx, jdx] = value;
          }
        }


        //public MakeGrid
        public void MakeGrid(ZeroCurve myZero, DivList myDivList)
        {
          EmptyArrays();
          MakeArrays();
          MakeSpotStar(myZero, myDivList);
          MakeDivArray(myZero, myDivList);
          FillForwardRate(myZero);
          FillUpDown(Sig);

          //make the spot grid
          double sv = SpotStart;
          SetSpotMatrix(0, 0, sv+ GetDividends(0)) ;

          //now march forward in time
          for (int idx = 1; idx <= GridSteps; idx++)
          {
            for (int jdx = 0; jdx <= idx; jdx++)
            {
              sv = SpotStart * Math.Pow(GetUp(idx - 1), jdx) * Math.Pow(GetDn(idx - 1), idx - jdx);
              sv += (idx == GridSteps) ? 0.0 : GetDividends(idx);
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
          if (SpotMatrix == null)
          {
            SpotMatrix = new double[GridSteps + 1, GridSteps + 1];
            Up = new double[GridSteps];
            Dn = new double[GridSteps];
            Rate = new double[GridSteps];
            Dividends = new double[GridSteps];
            DividendTime = new double[GridSteps];
          }
        }

        //empty grid
        private void EmptyArrays()
        {
          SpotMatrix = null;
          Up = null;
          Dn = null;
          Dividends = null;
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
                SetRate(idx, FlatFlag ? FlatRate : myZero.ForwardRate(idx * dt, (idx + 1) * dt));
            }
          }
        }

        //create the up/down arrays
        private void FillUpDown(double sig)
        {
          if (Up != null)
          {
              double dt = Tau / GridSteps;
            for (int idx = 0; idx < GridSteps; idx++)
            {
              double a = (1.0 + Math.Exp((2 * GetR(idx) + sig * sig) * dt));
              double upval = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * GetR(idx) * dt)))
                  / (2.0 * Math.Exp(GetR(idx) * dt));
              SetUp(idx, upval);
              SetDn(idx, 1.0 / upval);
            }
          }
        }

        private void MakeSpotStar(ZeroCurve myZero, DivList myDivList)
        {
          SpotStart = Spot;
          if ((myDivList != null) && (myZero != null))
          {
              for (int idx = 0; idx < myDivList.Divpoints; idx++)
              {
                  if (myDivList.GetT(idx) <= Tau)
                  {
                      double d1 = myDivList.GetD(idx);
                      double r1 = myZero.LinInterp(myDivList.GetT(idx));
                      double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
                      SpotStart -= d1 * t1;
                  }
              }
          }
        }

        private void MakeDivArray(ZeroCurve myZero, DivList myDivList)
        {
            double dt = Tau / GridSteps;
          if ((myDivList != null) && (myZero != null))
          {
              for (int idx = 0; idx < GridSteps; idx++)
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
                  SetDividends(idx, temp, dt * idx);
              }
          }
          else  //missing either div or zero, load in 0 for _div on tree
          {
              for (int idx = 0; idx <= GridSteps; idx++)
              {
                  SetDividends(idx, 0.0, dt * idx);
              }
          }
        }

        #endregion
    }
}
