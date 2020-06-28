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

#region Usings

using System;
using Highlander.Equities;
using ZeroCurve = Highlander.Reporting.Analytics.V5r3.Rates.ZeroCurve;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Equities 
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

        public double[] R { get; set; }

        public double[] DIV { get; set; }

        public double[] Divtime { get; set; }

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

        /// <summary>
        /// get forward rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetR(int idx)
        {
            return idx < GridSteps ? R[idx] : 0.0;
        }

        /// <summary>
        /// set the forward rate
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetR(int idx, double value)
        {
            if (R != null)
            {
                if (idx < GridSteps)
                {
                    R[idx] = value;
                }
            }
        }

        /// <summary>
        /// set the div rate
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        public void SetDIV(int idx, double value, double t)
        {
            DIV[idx] = value;
            Divtime[idx] = t;
        }

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDIV(int idx)
        {
            return DIV[idx];
        }

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDivtime(int idx)
        {
            return Divtime[idx];
        }

        /// <summary>
        /// get up item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetUp(int idx)
        {
            return idx < GridSteps ? Up[idx] : 0.0;
        }

        /// <summary>
        /// set the up itm
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetUp(int idx, double value)
        {
            if (idx < GridSteps)
            {
                Up[idx] = value;
            }
        }

        /// <summary>
        /// get dn item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetDn(int idx)
        {
            return idx < GridSteps ? Dn[idx] : 0.0;
        }

        /// <summary>
        /// set the dn itm
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        public void SetDn(int idx, double value)
        {
          if (idx < GridSteps)
          {
            Dn[idx] = value;
          }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// get SpotMatrix item
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <returns></returns>
        public double GetSpotMatrix(int idx, int jdx)
        {
            return SpotMatrix[idx, jdx];
        }

        /// <summary>
        /// set SpotMatrix item
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <param name="value"></param>
        public void SetSpotMatrix(int idx, int jdx, double value)
        {
            if ((idx <= GridSteps) && (jdx <= GridSteps))
            {
                SpotMatrix[idx, jdx] = value;
            }
        }


        /// <summary>
        /// public MakeGrid
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
            double sv = SpotStart;
            SetSpotMatrix(0, 0, sv+GetDIV(0)) ;
            //now march forward in time
            for (int idx = 1; idx <= GridSteps; idx++)
            {
                for (int jdx = 0; jdx <= idx; jdx++)
                {
                    sv = SpotStart * Math.Pow(GetUp(idx - 1), jdx) * Math.Pow(GetDn(idx - 1), idx - jdx);
                    sv += (idx == GridSteps) ? 0.0 : GetDIV(idx);
                    //get_div(idx);
                    SetSpotMatrix(idx, jdx, sv);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// create grid
        /// </summary>
        private void MakeArrays()
        {
            if (SpotMatrix == null)
            {
                SpotMatrix = new double[GridSteps + 1, GridSteps + 1];
                Up = new double[GridSteps];
                Dn = new double[GridSteps];
                R = new double[GridSteps];
                DIV = new double[GridSteps];
                Divtime = new double[GridSteps];
            }
        }

        /// <summary>
        /// empty grid
        /// </summary>
        private void EmptyArrays()
        {
            SpotMatrix = null;
            Up = null;
            Dn = null;
            DIV = null;
        }

        /// <summary>
        /// fill grid forward rates
        /// </summary>
        /// <param name="myZero"></param>
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

        /// <summary>
        /// create the up/down arrays
        /// </summary>
        /// <param name="sig"></param>
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

        /// <summary>
        /// public spotStart
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDivList"></param>
        private void MakeSpotStar(ZeroCurve myZero, DivList myDivList)
        {
            SpotStart = Spot;
            if ((myDivList != null) && (myZero != null))
            {
                for (int idx = 0; idx < myDivList.DivPoints; idx++)
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

        /// <summary>
        /// public spotStar
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDivList"></param>
        private void MakeDivArray(ZeroCurve myZero, DivList myDivList)
        {
            double dt = Tau / GridSteps;
            if ((myDivList != null) && (myZero != null))
            {
                for (int idx = 0; idx < GridSteps; idx++)
                {
                    double temp = 0.0;
                    for (int kdx = 0; kdx < myDivList.DivPoints; kdx++)
                    {
                        if ((myDivList.GetT(kdx) > idx * dt) && (myDivList.GetT(kdx) < Tau))
                        {

                            temp += myDivList.GetD(kdx) * Math.Exp(-myZero.ForwardRate(idx * dt, myDivList.GetT(kdx)) *
                                                                   (myDivList.GetT(kdx) - idx * dt));
                        }
                    }
                    SetDIV(idx, temp, dt * idx);
                }
            }
            else  //missing either div or zero, load in 0 for _div on tree
            {
                for (int idx = 0; idx <= GridSteps; idx++)
                {
                    SetDIV(idx, 0.0, dt * idx);
                }
            }
        }

        #endregion
    }
}
