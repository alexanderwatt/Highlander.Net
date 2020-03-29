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
using System.Collections.Generic;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Equities
{
    /// <summary>
    /// 
    /// </summary>
    public class CrankNicholson
    {
        #region Attributes
        //diffusion parameters
        private double _domR = 0.0;
        private double _forR = 0.0;


        //grid structure variables
        private int _msteps;
        private double _lnfwd;
        private double _n1;
        private double _n2;
        private double _dx;
        private double _dt;
        private double _theta = 0.5;

        //rate and divs curve in
        int[] ratedays;
        double[] rateamts;
        int[] divdays;
        double[] divamts;


        //array lists
        private List<double> _x;
        private List<double> _v;



        //array lists for matrix elements
        private List<double> _SubDiagL;
        private List<double> _DiagL;
        private List<double> _SuperDiagL;
        private List<double> _SubDiagR;
        private List<double> _DiagR;
        private List<double> _SuperDiagR;

        private List<double> _q;
        //private List<double> _r;


        //LU arrays
        private List<double> _l;
        private List<double> _d;
        private List<double> _u;


        #endregion

        #region Accessors

        /// <summary>
        /// 
        /// </summary>
        public double Sig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Steps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NTsteps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double T { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Xu { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double XL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Spot { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Strike { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SPay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SStyle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double StepSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool LbFlag { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool UbFlag { get; set; } = false;

        #endregion

        #region Public Methods
     
        /// <summary>
        /// Initializes a new instance of the <see cref="CrankNicholson"/> class.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="vol">The vol.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="tStepSize">Size of the t step.</param>
        /// <param name="far">The far.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        public CrankNicholson(bool isCall, bool isAmerican,  double spot, double strike, double yearFraction, double vol, int steps, double tStepSize, double far, int[] divdays,
                             double[] divamts, int[] ratedays, double[] rateamts)
        {
            this.Spot = spot;
            this.Strike = strike;
            T = yearFraction;
            Sig = vol;
            this.divdays = divdays;            
            this.divamts = divamts;
            PreValidateDivs();
            this.ratedays = ratedays;
            this.rateamts = rateamts;
            SStyle = isAmerican ? "A" : "E";
            SPay = isCall ? "C" : "P";
            XL = Math.Log(spot) - far * Sig * Math.Sqrt(yearFraction);
            Xu = Math.Log(spot) + far * Sig * Math.Sqrt(yearFraction);
            this.Steps = steps;
            NTsteps = Convert.ToInt32(T / tStepSize);
        }

        private void PreValidateDivs()
        {
            List<int> divdayslist = new List<int>();            
            List<double> divamtslist = new List<double>();            
            for (int idx = 0; idx < divdays.Length; idx++)
            {
                if (divdays[idx] > 0)
                {
                    divdayslist.Add(divdays[idx]);
                    divamtslist.Add(divamts[idx]);
                }
            }
            //replace existing members
            divdays = divdayslist.ToArray();
            divamts = divamtslist.ToArray();
        }

        public double[] GetPriceAndGreeks()
        {
            double[] greeks = new double[3];
            double[] res = new double[4];
            _x = new List<double>();
            _v = new List<double>();
            double tTemp = T;  //real time
            double tau = 0.0; //backward time
            double dtnom = T / (double)NTsteps;
            double dt = dtnom;
            double tempInt = 0.0;
            //start the pricer
            CreateGrid();
            TerminalCondition();
            while ((tau - T) < -0.001 * T)
            {
                //set the increment
                double t1 = tTemp - dtnom;
                t1 = (t1 >= 0.0) ? t1 : 0.0;  //make sure t1 >= 0.0
                double divPay = 0.0;
                dt = CheckBetweenDiv(t1, tTemp, ref divPay, divdays, divamts);
                //compute the real time and backward time tau
                tTemp -= dt;
                tau = T - tTemp;
                //compute the increment forward rate
                _domR = EquityAnalytics.GetRateCCLin365(tTemp, tTemp + dt, ratedays, rateamts);
                _forR = 0.0;
                //compute the forward rate from real time tTemp to expiry for the BC'c
                double domRbc = EquityAnalytics.GetRateCCLin365(tTemp, T, ratedays, rateamts);
                //compute discounted dividends for the bc's
                double DiscDiv = ComputeDiscDiv(tTemp, T, ratedays, rateamts, divdays, divamts);
                //save the value at the spot for use in theta calcuation
                int nKeyInt = (int)((Math.Log(Spot) - XL) / _dx);
                double fracInt = (Math.Log(Spot) - _x[nKeyInt]) / (_x[nKeyInt + 1] - _x[nKeyInt]);
                tempInt = _v[nKeyInt] * (1.0 - fracInt) + _v[nKeyInt + 1] * fracInt;
                //get the fwd
                _lnfwd = Math.Log(GetATMfwd(ratedays, rateamts, divdays, divamts, tTemp));
                //build the matrix
                OneStepSetUp(dt, tTemp);
                //compute the q vec
                MakeQVec();
                if (SStyle.ToUpper().Equals("E"))
                {
                    // set the exlicit BC
                    _v[0] = MakeLowerBC(tau, Math.Exp(_x[0]), domRbc, DiscDiv);
                    _v[Steps - 1] = MakeUpperBC(tau, Math.Exp(_x[Steps - 1]), domRbc, DiscDiv);
                    SORmethod();
                    //subract from q(1) and q(_steps-2) for explicit BC
                    //'_q[1] -= _SubDiagL[0] * _v[0];
                    //'_q[_msteps - 1] -= _SuperDiagL[_steps - 1] * _v[_steps - 1];

                    //this commented out info is used for the zero curvature condition
                    //_DiagL[1] += 2.0 * _SubDiagL[1];
                    //_SuperDiagL[1] -= _SubDiagL[1];
                    //_DiagL[_steps - 2] += 2.0 * _SuperDiagL[_steps - 2];
                    //_SubDiagL[_steps - 2] -= _SuperDiagL[_steps - 2];

                    //call LU decomp
                    //LUDecomp();
                    //Call the LU sOlver
                    //LUSolution();

                    //for zero curvature
                    //_v[0] = 2.0 * _v[1] - _v[2];
                    //_v[_steps - 1] = 2.0 * _v[_steps - 2] - _v[_steps - 3];
                }
                else
                {
                    _v[0] = MakeLowerBC(tau, Math.Exp(_x[0]), domRbc, DiscDiv);
                    _v[Steps - 1] = MakeUpperBC(tau, Math.Exp(_x[Steps - 1]), domRbc, DiscDiv);
                    SORmethod();
                }
                //after having incremented back,  apply a grid shift if needed
                if (divPay != 0.0)
                {
                    ApplyGridShift(tau, divPay, domRbc, DiscDiv);
                }
            }
            int nKey = (int)((Math.Log(Spot) - XL) / _dx);
            double frac = (Math.Log(Spot) - _x[nKey]) / (_x[nKey + 1] - _x[nKey]);
            double temp = _v[nKey] * (1.0 - frac) + _v[nKey + 1] * frac;
            double[,] a = new double[4, 4];
            double[] b = new double[4];
            a[0, 0] = 1.0;
            a[1, 0] = 1.0;
            a[2, 0] = 1.0;
            a[3, 0] = 1.0;
            a[0, 1] = Math.Exp(_x[nKey - 1]);
            a[1, 1] = Math.Exp(_x[nKey]);
            a[2, 1] = Math.Exp(_x[nKey + 1]);
            a[3, 1] = Math.Exp(_x[nKey + 2]);
            a[0, 2] = Math.Exp(_x[nKey - 1]) * Math.Exp(_x[nKey - 1]);
            a[1, 2] = Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]);
            a[2, 2] = Math.Exp(_x[nKey + 1]) * Math.Exp(_x[nKey + 1]);
            a[3, 2] = Math.Exp(_x[nKey + 2]) * Math.Exp(_x[nKey + 2]);
            a[0, 3] = Math.Exp(_x[nKey - 1]) * Math.Exp(_x[nKey - 1]) * Math.Exp(_x[nKey - 1]);
            a[1, 3] = Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]);
            a[2, 3] = Math.Exp(_x[nKey + 1]) * Math.Exp(_x[nKey + 1]) * Math.Exp(_x[nKey + 1]);
            a[3, 3] = Math.Exp(_x[nKey + 2]) * Math.Exp(_x[nKey + 2]) * Math.Exp(_x[nKey + 2]);
            b[0] = _v[nKey - 1];
            b[1] = _v[nKey];
            b[2] = _v[nKey + 1];
            b[3] = _v[nKey + 2];
            int info = NewtonGauss(4, ref a, ref b);
            greeks[0] = b[1] + 2.0 * b[2] * Spot + 3.0 * b[3] * Spot * Spot;
            greeks[1] = 2.0 * b[2] + 6.0 * b[3] * Spot;
            greeks[2] = (tempInt - temp) / (365.0 * dt);

            /*
            nKey = (int)((Math.Log(_spot) * 1.001 - _xl) / _dx);-
            frac = (Math.Log(_spot )* 1.001 - _x[nKey]) / (_x[nKey + 1] - _x[nKey]);
            double tempUp = _v[nKey] * (1.0 - frac) + _v[nKey + 1] * frac;

            nKey = (int)((Math.Log(_spot) * 0.999 - _xl) / _dx);
            frac = (Math.Log(_spot) * 0.999 - _x[nKey]) / (_x[nKey + 1] - _x[nKey]);
            double tempDn = _v[nKey] * (1.0 - frac) + _v[nKey + 1] * frac;

            greeks[0] = (tempUp - tempDn) / (0.002 *Math.Log(spot))/spot ;
            greeks[1] = (tempUp + tempDn - 2.0 * temp) / Math.Pow(0.001 * _spot * Math.Log(_spot), 2.0) - greeks[0]/_spot;
            greeks[2] = (tempInt - temp) / (365.0 * dt); 
             */
            res[0] = temp;
            res[1] = greeks[0];
            res[2] = greeks[1];
            res[3] = greeks[2];
            return res;
        }

        public double ImpVol(double price)
        {

          double[] greeks = new double[4];
          CrankNicholson clone = this.Clone();
          double[] temp0 = clone.GetPriceAndGreeks();
          double temp = temp0[0];
          for (int idx = 0; idx < 20; idx++)
          {
                temp0 = clone.GetPriceAndGreeks();
                temp = temp0[0];
                if (Math.Abs(temp - price) < 0.0001) break;
                double sigold = Sig;
                double dsig = 0.01 * Sig;
                Sig += dsig;
                clone.Sig = Sig;            
                double[] tempUp = clone.GetPriceAndGreeks();            
                sigold -= (temp - price) * dsig / (tempUp[0] - temp);
                Sig = sigold;
                clone.Sig = Sig;
          }
          return Sig;
        }

        /// <summary>
        /// Gets the AT MFWD.
        /// </summary>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public double GetATMfwd(int[] ratedays, double[] rateamts, int[] divdays, double[] divamts,  double t)
        {
            return EquityAnalytics.GetForwardCCLin365(Spot, t, divdays, divamts, ratedays, rateamts);         
        }
    
        #endregion

        #region Private Methods

        //Asset grid, create for this range
        private void CreateGrid()
        {
          _msteps = Steps - 1;   //number of upper index for the matrix manipulations
          _dx = (Xu - XL) / ((double)Steps - 1.0);
          //
          for (long idx = 0; idx < Steps; idx++)
          {
            _x.Add(XL + idx * _dx);
          }
        }

        //Payoff condition
        private void TerminalCondition()
        {
          for (int idx = 0; idx < Steps; idx++)
          {
              switch (SPay.ToUpper())
            {
              case "C":
                _v.Add(Math.Max(Math.Exp(_x[idx]) - Strike, 0.0));
                break;
              case "P":
                _v.Add(Math.Max(Strike - Math.Exp(_x[idx]), 0.0));
                break;
              case "A":
                _v.Add(((Math.Exp(_x[idx]) > Strike) ? 1.0 : 0.0));
                break;
              case "B":
                _v.Add(((Math.Exp(_x[idx]) > Strike) ? 0.0 : 1.0));
                break;
              case "T":
                _v.Add(1.0);
                break;
            }
          }
        }
        //for specified 
        private void OneStepSetUp(double dt, double T)
        {
          _dt = dt;
          _n1 = _dt / (_dx * _dx);
          _n2 = _dt / _dx;
          //create fresh matrix rows
          MakeNewMatrixRows();
          //scroll through rows
          for (int idx = 0; idx < Steps; idx++)
          {
            //set the Asset
            double S = Math.Exp(_x[idx]);
            double vol = Sig;
            //get the common functions
            double _a = a(S, T, vol);
            double _b = b(S, T, vol);
            double _c = c(S, T);
            //set up the common coefficients
            double _AR = (1.0 - _theta) * (_n1 * _a - 0.5 * _n2 * _b);
            double _BR = (1.0 - _theta) * (-2.0 * _n1 * _a + _dt * _c);
            double _CR = (1.0 - _theta) * (_n1 * _a + 0.5 * _n2 * _b);
            double _AL = _theta * _AR / (1.0 - _theta);
            double _BL = _theta * _BR / (1.0 - _theta);
            double _CL = _theta * _CR / (1.0 - _theta);
            //load the arrays
            _SubDiagL.Add(-_AL);
            _DiagL.Add(1.0 - _BL);
            _SuperDiagL.Add(-_CL);
            _SubDiagR.Add(_AR);
            _DiagR.Add(1.0 + _BR);
            _SuperDiagR.Add(_CR);
          }
        }
        
        //create new matrix rows
        private void MakeNewMatrixRows()
        {
          _SubDiagL = new List<double>();
          _DiagL = new List<double>();
          _SuperDiagL = new List<double>();
          _SubDiagR = new List<double>();
          _DiagR = new List<double>();
          _SuperDiagR = new List<double>();
        }

        //compute the q vector
        private void MakeQVec()
        {
            _q = new List<double> {0.0};
            for (int idx = 1; idx < _msteps; idx++)
                _q.Add( _SubDiagR[idx] * _v[idx - 1] + _DiagR[idx] * _v[idx] + _SuperDiagR[idx] * _v[idx + 1]);
            _q.Add(0.0);
        }

        //apply the BC
        private double MakeLowerBC(double tau, double S, double domRbc, double DiscDiv)
        {
          double temp = 0.0;
          if (LbFlag) return 0.0;
          switch (SPay.ToUpper())
          {
            case "C":
              break;
            case "P":
              temp = (SStyle.ToUpper().Equals("E")) ? Math.Exp(-domRbc * tau) * Strike - Math.Max(S - DiscDiv, 0.0) :
                  Strike - S;
              break;
            case "A":
              break;
            case "B":
              temp = Math.Exp(-domRbc * tau);
              break;
            case "T":
              break;
          }

          return temp;
        }

        private double MakeUpperBC(double tau, double S, double domRbc, double DiscDiv)
        {
          double temp = 0.0; 
          if (UbFlag) return 0.0;
          switch (SPay.ToUpper())
          {
            case "C":
              temp = (S - DiscDiv - Math.Exp(-domRbc * tau) * Strike);
              temp = (SStyle.ToUpper().Equals("A")) ? Math.Max(temp, S - Strike) : temp;
              break;
            case "P":
              break;
            case "A":
              temp = Math.Exp(-domRbc * tau);
              break;
            case "B":       
              break;
            case "T":
              break;
            default:
              break;
          }

          return temp;
        }


        //APPPLY L-U decomposition
        private void LUDecomp()
        {
          _l = new List<double>();
          _d = new List<double>();
          _u = new List<double>();
          //pad with zeros
          _d.Add(0.0);
          _d.Add(_DiagL[1]);
          _u.Add(0.0);
          _l.Add(0.0);
          _l.Add(0.0);
          for (int idx = 2; idx < _msteps; idx++)
          {
            _u.Add(_SuperDiagL[idx-1]);
            _l.Add(_SubDiagL[idx]/_d[idx-1]);
            _d.Add(_DiagL[idx] - _l[idx] * _SuperDiagL[idx - 1]);
          }
        }

        //Apply L-U solution
        private void LUSolution()
        {
          List<double> _w = new List<double>();
          _w.Add(0.0);
          _w.Add(_q[1]);
          for (int idx = 2; idx < _msteps; idx++)
           _w.Add(_q[idx] - _l[idx] * _w[idx - 1]);
          _v[_msteps - 1] = _w[_msteps - 1] / _d[_msteps - 1];
          for (int idx = _msteps - 2; idx > 0; idx--)
            _v[idx] = (_w[idx] - _u[idx] * _v[idx + 1]) / _d[idx];
        }

        //SOR method
        private void SORmethod()  
        {
          int noits = 0;
          double tol = 0.000001; // 0.0000001;
          double err = 1.0;
          //double[] temp = new double[_v.Count];
          double temp = 0.0;
          bool bAm = (SStyle.ToUpper().Equals("A")) ? true : false;
          while (Math.Sqrt(err) > tol)
          {
            err = 0.0;
            for (int idx = 1; idx < _msteps; idx++)
            {
               temp = (_q[idx] - _SuperDiagL[idx] * _v[idx + 1] - _SubDiagL[idx] * _v[idx - 1]) / _DiagL[idx];
               temp = bAm ?  Math.Max(temp, AmPay(idx)) : temp;
              //if (_sPay.ToUpper().Equals("P"))
              //  temp = bAm ?  Math.Max(temp, AmPay(idx)) : temp;
              err += (temp - _v[idx]) * (temp - _v[idx]);
              _v[idx] = temp;
            }
            noits += 1;
            //temp[0] = _v[0];
            //temp[_msteps] = _v[_msteps];
            //for( int idx = 0; idx != _steps; idx++)
            //  _v[idx] = temp[idx];
          }
        }

        private double AmPay(int idx)
        {
          return (SPay.ToUpper().Equals("C")) ? Math.Exp(_x[idx]) - Strike : Strike - Math.Exp(_x[idx]);
        }

        //determine if dt needs adjusted because a div occurs in proposed interval
        private static double CheckBetweenDiv(double t1, double t2, ref double discDiv, int[] divdays, double[] divamts)   //t1 & t2 are real times
        {
          double temp = t2-t1;
          int n = divdays.Length;
          for (int idx = 0; idx < n; idx++)
          {
              double dt0 = Convert.ToDouble(divdays[idx])/365.0;
                if ((t1 <= dt0) && (t2 > dt0))
                {
                    temp = t2 - dt0;
                    discDiv = divamts[idx];
                    break;
                }
          }
          return temp;
        }

        //Apply grid shift for disc divs
        private void ApplyGridShift(double tau, double divPay, double domRbc, double DiscDiv)
        {
          //set up the new list
          List<double> _vn = new List<double>();
          double temp;

          //for lower boundary, assign the lower bc using the immediate exercise
          temp = MakeLowerBC(tau, Math.Max(Math.Exp(_x[0]) - divPay,0.0), domRbc, DiscDiv);
          _vn.Add(SStyle.ToUpper().Equals("A") ? Math.Max(temp, AmPay(0)) : temp);
          for (int idx = 1; idx < Steps; idx++)
          {
            double xshift = Math.Log(Math.Exp(_x[idx]) - divPay);
            if (xshift >= XL)
            {
              int idNew = (int) ((xshift - XL) / _dx);
              double frac = (xshift - _x[idNew]) / (_x[idNew+1] - _x[idNew]);
              temp = (1.0 - frac) * _v[idNew] + frac * _v[idNew + 1];

              if (SStyle.ToUpper().Equals("A"))
               _vn.Add(Math.Max(temp, AmPay(idx))); 
              else
               _vn.Add(temp); 
            }
            else // the new points are still below the grid
            {
              temp = MakeLowerBC(tau, Math.Max(Math.Exp(_x[idx]) - divPay,0.0), domRbc, DiscDiv);
              if (SStyle.ToUpper().Equals("A"))
                _vn.Add(Math.Max(temp,AmPay(idx)));
              else
                _vn.Add(temp);
            }
          }
          //for upper boundary add the upper
          //_vn.Add(MakeUpperBC(tau, Math.Exp(_x[_steps-1]) - divPay, domRbc, DiscDiv));
          _v = _vn;
        }

        //compute Sstar for the BC's
        /// <summary>
        /// Computes the disc div.
        /// </summary>
        /// <param name="tTemp">The t temp.</param>
        /// <param name="T">The T.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <returns></returns>
        private static double ComputeDiscDiv(double tTemp, double T, int[] ratedays, double[] rateamts, int[] divdays, double[] divamts)
        {
            return EquityAnalytics.GetPVDivsCCLin365(tTemp, T, divdays, divamts, ratedays, rateamts);          
        }

        //coeffiecients ffor the diffusion equation
        private static double a(double S, double T, double vol)
        {
          return 0.5 * vol * vol;
        }

        private double b(double S, double T, double vol)
        {
          return _domR - _forR - 0.5 * vol * vol;
        }

        private double c(double S, double T)
        {
          return -_domR;
        }

        private static int NewtonGauss(int n, ref double[,] am, ref double[] bm)
        {
          int[] ipiv = new int[20];
          int[] indxr = new int[20];
          int[] indxc = new int[20];
          int icol = 0, irow = 0, idx, jdx, kdx, ldx, lldx;
          double big, pivinv, dum;
          for (idx = 0; idx < n; idx++) { ipiv[idx] = 0; }
          for (idx = 0; idx < n; idx++)
          {
            big = 0.0;
            for (jdx = 0; jdx < n; jdx++)
            {
              if (ipiv[jdx] != 1)
              {
                for (kdx = 0; kdx < n; kdx++)
                {
                  if (ipiv[kdx] == 0.0)
                  {
                    if (Math.Abs(am[jdx, kdx]) > big)
                    {
                      big = Math.Abs(am[jdx, kdx]); irow = jdx; icol = kdx;
                    }
                  }
                  else
                  {
                    if (ipiv[kdx] > 1)
                    {
                      return 90;
                    }
                  } //endif
                } // next k
              }
            } //next j
            ipiv[icol] += 1;
            if (irow != icol)
            {
              for (ldx = 0; ldx < n; ldx++)
              {
                dum = am[irow, ldx]; am[irow, ldx] = am[icol, ldx]; am[icol, ldx] = dum;
              }
              dum = bm[irow]; bm[irow] = bm[icol]; bm[icol] = dum;
            }
            indxr[idx] = irow;
            indxc[idx] = icol;
            if (am[icol, icol] == 0.0) { return 98; }
            pivinv = 1.0 / am[icol, icol];
            am[icol, icol] = 1.0;
            for (ldx = 0; ldx < n; ldx++) { am[icol, ldx] *= pivinv; }
            bm[icol] *= pivinv;
            for (lldx = 0; lldx < n; lldx++)
            {
              if (lldx != icol)
              {
                dum = am[lldx, icol];
                am[lldx, icol] = 0.0;
                for (ldx = 0; ldx < n; ldx++) { am[lldx, ldx] -= am[icol, ldx] * dum; }
                bm[lldx] -= bm[icol] * dum;
              }
            }
          }  // next idx
          for (ldx = (n - 1); ldx >= 0; ldx--)
          {
            if (indxr[ldx] != indxc[ldx])
            {
              for (kdx = 0; kdx < n; kdx++)
              {
                dum = am[kdx, indxr[ldx]]; am[kdx, indxr[ldx]] = am[kdx, indxc[ldx]]; am[kdx, indxc[ldx]] = dum;
              }
            }
          }  // next ldx
          return 2;
        }

    
        #endregion

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        private CrankNicholson Clone()
        {
            return (CrankNicholson)MemberwiseClone();
            //return new CrankNicolson(spot,strike,T,sig,steps,tStepSize,far,divdays,divamts,ratedays,rateamts);
        }
    }
}
