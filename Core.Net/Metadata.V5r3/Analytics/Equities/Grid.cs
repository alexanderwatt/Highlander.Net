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
using System.Text;
using Highlander.Equities;
using ZeroCurve = Highlander.Reporting.Analytics.V5r3.Rates.ZeroCurve;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Equities
{
    /// <summary>
    /// 
    /// </summary>
    public class Grid
    {

        #region Attributes
        //diffusion parameters
        private double _domR;
        private double _forR;


        //grid structure variables
        private int _msteps;

        private double _lnfwd;
        private double _n1;
        private double _n2;
        private double _dx;
        private double _dt;
        private double _theta = 0.5;

        //array lists
        private List<double> _x;
        private List<double> _v;


        //array lists for matrix elements
        private List<double> _subDiagL;
        private List<double> _diagL;
        private List<double> _superDiagL;
        private List<double> _subDiagR;
        private List<double> _diagR;
        private List<double> _superDiagR;

        private List<double> _q;
        //private List<double> _r;


        //LU arrays
        private List<double> _l;
        private List<double> _d;
        private List<double> _u;


        #endregion

        #region Accessors

        public double Sig { get; set; }

        public int Steps { get; set; }

        public int NTsteps { get; set; }

        public double T { get; set; }

        public double Xu { get; set; }

        public double XL { get; set; }

        public double Spot { get; set; }

        public double Strike { get; set; }

        public string SPay { get; set; }

        public string SStyle { get; set; }

        public bool LbFlag { get; set; }

        public bool UbFlag { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// constructor
        /// </summary>
        public Grid()
        {
        }

        /// <summary>
        /// main pricer
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDiv"></param>
        /// <param name="greeks"></param>
        /// <param name="bGreeks"></param>
        /// <returns></returns>
        public double Pricer(ZeroCurve myZero, DivList myDiv, ref double[] greeks, bool bGreeks)
        {
            _x = new List<double>();
            _v = new List<double>();
            double tTemp = T;  //real time
            double tau = 0.0; //backward time
            double dtnom = T / (double) NTsteps;
            double dt = dtnom;
            double tempInt=0.0;
            //start the pricer
            CreateGrid();
            TerminalCondition();
            while( tau < T)
            {
                //set the increment
                double t1 = tTemp - dtnom;
                t1 = (t1 >= 0.0) ? t1 : 0.0;  //make sure t1 >= 0.0
                double divPay = 0.0;
                dt = CheckBetweenDiv(t1, tTemp, ref divPay, myDiv);
                //compute the real time and backward time tau
                tTemp -= dt;
                tau = T - tTemp;
                //compute the increment forward rate
                _domR = myZero.ForwardRate(tTemp, tTemp + dt);
                _forR = 0.0;
                //compute the forward rate from real time tTemp to expiry for the BC'c
                double domRbc = myZero.ForwardRate(tTemp, T);
                //compute discounted dividends for the bc's
                double DiscDiv = ComputeDiscDiv(tTemp, T, myZero, myDiv);
                //save the value at the spot for use in theta calcuation
                int nKeyInt = (int)((Math.Log(Spot) - XL) / _dx);
                double fracInt = (Math.Log(Spot) - _x[nKeyInt]) / (_x[nKeyInt + 1] - _x[nKeyInt]);
                tempInt = _v[nKeyInt] * (1.0 - fracInt) + _v[nKeyInt + 1] * fracInt;
                //get the fwd
                _lnfwd = Math.Log(GetATMfwd(myZero, myDiv, tTemp));
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
            int nKey = (int) ((Math.Log(Spot) - XL) / _dx);
            double frac = (Math.Log(Spot) - _x[nKey])/ (_x[nKey+1] - _x[nKey]);
            double temp = _v[nKey] * (1.0 - frac) + _v[nKey+1] * frac;
            if (bGreeks)
            {
                double[,] a = new double[4, 4];
                double[] b = new double[4];
                a[0,0] = 1.0;
                a[1,0] = 1.0;
                a[2,0] = 1.0;
                a[3,0] = 1.0;
                a[0, 1] = Math.Exp(_x[nKey-1]);
                a[1, 1] = Math.Exp(_x[nKey]);
                a[2, 1] = Math.Exp(_x[nKey +1]);
                a[3, 1] = Math.Exp(_x[nKey +2]);
                a[0, 2] = Math.Exp(_x[nKey - 1]) * Math.Exp(_x[nKey - 1]);
                a[1, 2] = Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]);
                a[2, 2] = Math.Exp(_x[nKey + 1])*Math.Exp(_x[nKey + 1]);
                a[3, 2] = Math.Exp(_x[nKey + 2]) * Math.Exp(_x[nKey + 1]);
                a[0, 3] = Math.Exp(_x[nKey - 1])*Math.Exp(_x[nKey - 1])*Math.Exp(_x[nKey - 1]);
                a[1, 3] = Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]) * Math.Exp(_x[nKey]);
                a[2, 3] = Math.Exp(_x[nKey + 1]) * Math.Exp(_x[nKey + 1]) * Math.Exp(_x[nKey +1]);
                a[3, 3] = Math.Exp(_x[nKey + 2] )* Math.Exp(_x[nKey + 2] )* Math.Exp(_x[nKey + 2]);
                b[0] = _v[nKey-1];
                b[1] = _v[nKey];
                b[2] = _v[nKey+1];
                b[3] = _v[nKey+2];
                int info = NewtonGauss(4, ref a, ref b);
                greeks[0] = b[1] + 2.0 * b[2] * Spot +3.0 * b[3] * Spot * Spot;
                greeks[1] =2.0*b[2] +6.0* b[3] * Spot ;
                greeks[2] = (tempInt - temp) / (365.0 * dt);
                /*
                nKey = (int)((Math.Log(_spot) * 1.001 - _xl) / _dx);
                frac = (Math.Log(_spot )* 1.001 - _x[nKey]) / (_x[nKey + 1] - _x[nKey]);
                double tempUp = _v[nKey] * (1.0 - frac) + _v[nKey + 1] * frac;
                nKey = (int)((Math.Log(_spot) * 0.999 - _xl) / _dx);
                frac = (Math.Log(_spot) * 0.999 - _x[nKey]) / (_x[nKey + 1] - _x[nKey]);
                double tempDn = _v[nKey] * (1.0 - frac) + _v[nKey + 1] * frac;
                greeks[0] = (tempUp - tempDn) / (0.002 *Math.Log(spot))/spot ;
                greeks[1] = (tempUp + tempDn - 2.0 * temp) / Math.Pow(0.001 * _spot * Math.Log(_spot), 2.0) - greeks[0]/_spot;
                greeks[2] = (tempInt - temp) / (365.0 * dt); 
                    */
            }
            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="myZero"></param>
        /// <param name="myDiv"></param>
        /// <returns></returns>
        public double ImpVol(double price, ZeroCurve myZero, DivList myDiv)
        {
            double[] greeks = new double[4];
            double temp = Pricer(myZero, myDiv, ref greeks, false);
            for (int idx = 0; idx < 20; idx++)
            {
                temp = Pricer(myZero, myDiv, ref greeks, false);
                if (Math.Abs(temp - price) < 0.0001) break;

                double sigold = Sig;
                double dsig = 0.01 * Sig;
                Sig += dsig;
                double tempUp = Pricer(myZero, myDiv, ref greeks, false);
                sigold -= (temp - price) * dsig / (tempUp - temp);
                Sig = sigold;
            }
            return Sig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDivList"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public double GetATMfwd(ZeroCurve myZero, DivList myDivList, double t)
        {
            double _spotstar = Spot;
            //compute the discounted dividends and take off spot
            if ((myDivList != null) && (myZero != null))
            {
                for (int idx = 0; idx < myDivList.DivPoints; idx++)
                {
                    if (0 < myDivList.GetT(idx) && myDivList.GetT(idx) <= t)
                    {
                        double d1 = myDivList.GetD(idx);
                        double r1 = myZero.LinInterp(myDivList.GetT(idx));
                        double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
                        _spotstar -= d1 * t1;
                    }
                }
            }
            //gross up to expiry to get atfwd
            double r = myZero.LinInterp(t);
            return _spotstar * Math.Exp(r * t);
        }

        #endregion

        #region Private Methods

        //Asset grid, creat for this range
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
                _subDiagL.Add(-_AL);
                _diagL.Add(1.0 - _BL);
                _superDiagL.Add(-_CL);
                _subDiagR.Add(_AR);
                _diagR.Add(1.0 + _BR);
                _superDiagR.Add(_CR); 
            }
        }

        //create new matrix rows
        private void MakeNewMatrixRows()
        {
            _subDiagL = new List<double>();
            _diagL = new List<double>();
            _superDiagL = new List<double>();
            _subDiagR = new List<double>();
            _diagR = new List<double>();
            _superDiagR = new List<double>();
        }

        //compute the q vector
        private void MakeQVec()
        {
            _q = new List<double> {0.0};
            for (int idx = 1; idx < _msteps; idx++)
                _q.Add( _subDiagR[idx] * _v[idx - 1] + _diagR[idx] * _v[idx] + _superDiagR[idx] * _v[idx + 1]);
            _q.Add(0.0);
        }

        //apply the BC
        private double MakeLowerBC(double tau, double S, double domRbc, double discDiv)
        {
            double temp = 0.0;
            if (LbFlag) return 0.0;
            switch (SPay.ToUpper())
            {
            case "C":
                break;
            case "P":
                temp = (SStyle.ToUpper().Equals("E")) ? Math.Exp(-domRbc * tau) * Strike - Math.Max(S - discDiv, 0.0) :
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
            _d.Add(_diagL[1]);
            _u.Add(0.0);
            _l.Add(0.0);
            _l.Add(0.0);
            for (int idx = 2; idx < _msteps; idx++)
            {
                _u.Add(_superDiagL[idx-1]);
                _l.Add(_subDiagL[idx]/_d[idx-1]);
                _d.Add(_diagL[idx] - _l[idx] * _superDiagL[idx - 1]);
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
                    temp = (_q[idx] - _superDiagL[idx] * _v[idx + 1] - _subDiagL[idx] * _v[idx - 1]) / _diagL[idx];

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
        private double CheckBetweenDiv(double t1, double t2, ref double discDiv, DivList myDiv)   //t1 & t2 are real times
        {
            double temp = t2-t1;
            for (int idx = 0; idx < myDiv.DivPoints; idx++)
            {
                if ((t1 <= myDiv.GetT(idx)) && (t2 > myDiv.GetT(idx)))
                {
                    temp = t2 - myDiv.GetT(idx);
                    discDiv = myDiv.GetD(idx);
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
                    _vn.Add(SStyle.ToUpper().Equals("A") ? Math.Max(temp, AmPay(idx)) : temp);
                }
                else // the new points are still below the grid
                {
                    temp = MakeLowerBC(tau, Math.Max(Math.Exp(_x[idx]) - divPay,0.0), domRbc, DiscDiv);
                    _vn.Add(SStyle.ToUpper().Equals("A") ? Math.Max(temp, AmPay(idx)) : temp);
                }
            }
            //for upper boundary add the upper
            //_vn.Add(MakeUpperBC(tau, Math.Exp(_x[_steps-1]) - divPay, domRbc, DiscDiv));
            _v = _vn;
        }

        //compute Sstar for the BC's
        private double ComputeDiscDiv(double tTemp, double T, ZeroCurve myZero, DivList mydiv)
        {
            double temp = 0.0;
            for (int idx = 0; idx < mydiv.DivPoints; idx++)
            {
                if ((tTemp < mydiv.GetT(idx)) && (mydiv.GetT(idx) < T))
                {
                    double fwdRate = myZero.ForwardRate(tTemp, mydiv.GetT(idx));
                    temp += mydiv.GetD(idx) * Math.Exp(-fwdRate * (mydiv.GetT(idx) - tTemp));
                }
            }
            return temp;
        }

        //coeffiecients ffor the diffusion equation
        private double a(double S, double T, double vol)
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
            int icol = 0, irow = 0, idx;
            int kdx, ldx;
            double dum;
            for (idx = 0; idx < n; idx++) { ipiv[idx] = 0; }
            for (idx = 0; idx < n; idx++)
            {
                var big = 0.0;
                int jdx;
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
                var pivinv = 1.0 / am[icol, icol];
                am[icol, icol] = 1.0;
                for (ldx = 0; ldx < n; ldx++) { am[icol, ldx] *= pivinv; }
                bm[icol] *= pivinv;
                int lldx;
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
    }
}
