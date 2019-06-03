#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using Orion.Analytics.Rates;

#endregion

namespace Orion.Analytics.Equities
{
    /// <summary>
    /// 
    /// </summary>
    public class Grid
    {

        #region Attributes
        //diffusion parameters
        private double _sig;
        private double _domR = 0.0;
        private double _forR = 0.0;


        //grid structure variables
        private int _steps;
        private int _msteps;
        private string _sPay;
        private string _sStyle;
        private double _strike;
        private double _spot;
        private double _lnfwd;
        private double _n1;
        private double _n2;
        private double _dx;
        private double _dt;
        private double _xl;
        private double _xu;
        private double _theta = 0.5;
        private double _T;
        private int _nTsteps;
        private bool _lbFlag = false;
        private bool _ubFlag = false;

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
        private List<double> _r;


        //LU arrays
        private List<double> _l;
        private List<double> _d;
        private List<double> _u;


        #endregion

        #region Accessors

        public double sig
        {
            get { return _sig; }
            set { _sig = value; }
        }

        public int steps
        {
            get { return _steps; }
            set { _steps = value; }
        }

        public int nTsteps
        {
            get { return _nTsteps; }
            set { _nTsteps = value; }
        }

        public double T
        {
            get { return _T; }
            set { _T = value; }
        }

        public double xu
        {
            get { return _xu; }
            set { _xu = value; }
        }

        public double xl
        {
            get { return _xl; }
            set { _xl = value; }
        }

        public double spot
        {
            get { return _spot; }
            set { _spot = value; }
        }

        public double strike
        {
            get { return _strike; }
            set { _strike = value; }
        }

        public string sPay
        {
            get { return _sPay; }
            set { _sPay = value; }
        }

        public string sStyle
        {
            get { return _sStyle; }
            set { _sStyle = value; }
        }

        public bool lbFlag
        {
            get { return _lbFlag; }
            set { _lbFlag = value; }
        }

        public bool ubFlag
        {
            get { return _ubFlag; }
            set { _ubFlag = value; }
        }

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

                double tTemp = _T;  //real time
                double tau = 0.0; //backward time

                double dtnom = _T / (double) _nTsteps;
                double dt = dtnom;
                double tempInt=0.0;

                //start the pricer

                CreateGrid();
                TerminalCondition();

                while( tau < _T)
                {
                //set the increment
                double t1 = tTemp - dtnom;
                t1 = (t1 >= 0.0) ? t1 : 0.0;  //make sure t1 >= 0.0
                double divPay = 0.0;
                dt = CheckBetweenDiv(t1, tTemp, ref divPay, myDiv);


                //compute the real time and backward time tau
                tTemp -= dt;
                tau = _T - tTemp;    
       
                //compute the increment forward rate
                _domR = myZero.forwardRate(tTemp, tTemp + dt);
                _forR = 0.0;

                //compute the forward rate from real time tTemp to expiry for the BC'c
                double domRbc = myZero.forwardRate(tTemp, _T);

                //compute discounted dividends for the bc's
                double DiscDiv = ComputeDiscDiv(tTemp, _T, myZero, myDiv);

                //save the value at the spot for use in theta calcuation
                int nKeyInt = (int)((Math.Log(_spot) - _xl) / _dx);
                double fracInt = (Math.Log(_spot) - _x[nKeyInt]) / (_x[nKeyInt + 1] - _x[nKeyInt]);
                tempInt = _v[nKeyInt] * (1.0 - fracInt) + _v[nKeyInt + 1] * fracInt;


                //get the fwd
                _lnfwd = Math.Log(GetATMfwd(myZero, myDiv, tTemp));

                //build the matrix
                OneStepSetUp(dt, tTemp);

                //compute the q vec
                MakeQVec();

        
                if (_sStyle.ToUpper().Equals("E"))
                {
                    // set the exlicit BC
                    _v[0] = MakeLowerBC(tau, Math.Exp(_x[0]), domRbc, DiscDiv);
                    _v[_steps - 1] = MakeUpperBC(tau, Math.Exp(_x[_steps - 1]), domRbc, DiscDiv);
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
                    _v[_steps - 1] = MakeUpperBC(tau, Math.Exp(_x[_steps - 1]), domRbc, DiscDiv);       
                    SORmethod();
                }

                //after having incremented back,  apply a grid shift if needed
                if (divPay != 0.0)
                {
                    ApplyGridShift(tau, divPay, domRbc, DiscDiv);
                }

                }

                int nKey = (int) ((Math.Log(_spot) - _xl) / _dx);
                double frac = (Math.Log(_spot) - _x[nKey])/ (_x[nKey+1] - _x[nKey]);
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

                greeks[0] = b[1] + 2.0 * b[2] * _spot +3.0 * b[3] * _spot * _spot;
                greeks[1] =2.0*b[2] +6.0* b[3] * _spot ;
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

                double sigold = sig;
                double dsig = 0.01 * sig;
                sig += dsig;
                double tempUp = Pricer(myZero, myDiv, ref greeks, false);
                sigold -= (temp - price) * dsig / (tempUp - temp);
                sig = sigold;
                }
                return sig;
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
                double _spotstar = _spot;
                //compute the discounted dividends and take off spot
                if ((myDivList != null) && (myZero != null))
                {
                for (int idx = 0; idx < myDivList.Divpoints; idx++)
                {
                    if (0 < myDivList.GetT(idx) && myDivList.GetT(idx) <= t)
                    {
                    double d1 = myDivList.GetD(idx);
                    double r1 = myZero.linInterp(myDivList.GetT(idx));
                    double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
                    _spotstar -= d1 * t1;
                    }
                }
                }

                //gross up to expiry to get atfwd
                double r = myZero.linInterp(t);
                return _spotstar * Math.Exp(r * t);

            }

    

        #endregion

        #region Private Methods

        //Asset grid, creat for this range
        private void CreateGrid()
        {
            _msteps = _steps - 1;   //number of upper index for the matrix manipulations
            _dx = (_xu - _xl) / ((double)_steps - 1.0);
            //
            for (long idx = 0; idx < _steps; idx++)
            {
            _x.Add(_xl + idx * _dx);
            }

            return;
        }


        //Payoff condition
        private void TerminalCondition()
        {
            for (int idx = 0; idx < _steps; idx++)
            {
        
            switch (_sPay.ToUpper())
            {
                case "C":
                _v.Add(Math.Max(Math.Exp(_x[idx]) - _strike, 0.0));
                break;
                case "P":
                _v.Add(Math.Max(_strike - Math.Exp(_x[idx]), 0.0));
                break;
                case "A":
                _v.Add(((Math.Exp(_x[idx]) > _strike) ? 1.0 : 0.0));
                break;
                case "B":
                _v.Add(((Math.Exp(_x[idx]) > _strike) ? 0.0 : 1.0));
                break;
                case "T":
                _v.Add(1.0);
                break;
                default:
                break;
            }
            }
            return;
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
            for (int idx = 0; idx < _steps; idx++)
            {
            //set the Asset
            double S = Math.Exp(_x[idx]);

            double vol = _sig;

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

            return;
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
            return;
        }


        //compute the q vector
        private void MakeQVec()
        {
            _q = new List<double>();
            _q.Add(0.0);
            for (int idx = 1; idx < _msteps; idx++)
            _q.Add( _SubDiagR[idx] * _v[idx - 1] + _DiagR[idx] * _v[idx] + _SuperDiagR[idx] * _v[idx + 1]);
            _q.Add(0.0);
            return;
        }

        //apply the BC
        private double MakeLowerBC(double tau, double S, double domRbc, double DiscDiv)
        {
            double temp = 0.0;
            if (_lbFlag) return 0.0;
            switch (_sPay.ToUpper())
            {
            case "C":
                break;
            case "P":
                temp = (_sStyle.ToUpper().Equals("E")) ? Math.Exp(-domRbc * tau) * _strike - Math.Max(S - DiscDiv, 0.0) :
                    _strike - S;
                break;
            case "A":
                break;
            case "B":
                temp = Math.Exp(-domRbc * tau);
                break;
            case "T":
                break;
            default:
                break;
            }

            return temp;
        }

        private double MakeUpperBC(double tau, double S, double domRbc, double DiscDiv)
        {
            double temp = 0.0; 
            if (_ubFlag) return 0.0;
            switch (_sPay.ToUpper())
            {
            case "C":
                temp = (S - DiscDiv - Math.Exp(-domRbc * tau) * _strike);
                temp = (_sStyle.ToUpper().Equals("A")) ? Math.Max(temp, S - _strike) : temp;
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
            return;
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


            return;
        }


        //SOR method
        private void SORmethod()  
        {
            int noits = 0;
            double tol = 0.000001; // 0.0000001;
            double err = 1.0;
            //double[] temp = new double[_v.Count];
            double temp = 0.0;
            bool bAm = (_sStyle.ToUpper().Equals("A")) ? true : false;

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
            return (_sPay.ToUpper().Equals("C")) ? Math.Exp(_x[idx]) - _strike : _strike - Math.Exp(_x[idx]);
        }


        //determine if dt needs adjusted because a div occurs in proposed interval
        private double CheckBetweenDiv(double t1, double t2, ref double discDiv, DivList myDiv)   //t1 & t2 are real times
        {
            double temp = t2-t1;
            for (int idx = 0; idx < myDiv.Divpoints; idx++)
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
            if (_sStyle.ToUpper().Equals("A"))
            _vn.Add(Math.Max(temp,AmPay(0)));
            else
            _vn.Add(temp);

            for (int idx = 1; idx < _steps; idx++)
            {
            double xshift = Math.Log(Math.Exp(_x[idx]) - divPay);
            if (xshift >= _xl)
            {
                int idNew = (int) ((xshift - _xl) / _dx);
                double frac = (xshift - _x[idNew]) / (_x[idNew+1] - _x[idNew]);

                temp = (1.0 - frac) * _v[idNew] + frac * _v[idNew + 1];

                if (_sStyle.ToUpper().Equals("A"))
                _vn.Add(Math.Max(temp, AmPay(idx))); 
                else
                _vn.Add(temp); 
            }
            else // the new points are still below the grid
            {
                temp = MakeLowerBC(tau, Math.Max(Math.Exp(_x[idx]) - divPay,0.0), domRbc, DiscDiv);
                if (_sStyle.ToUpper().Equals("A"))
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
        private double ComputeDiscDiv(double tTemp, double T, ZeroCurve myZero, DivList mydiv)
        {
            double temp = 0.0;
            for (int idx = 0; idx < mydiv.Divpoints; idx++)
            {
            if ((tTemp < mydiv.GetT(idx)) && (mydiv.GetT(idx) < T))
            {
                double fwdRate = myZero.forwardRate(tTemp, mydiv.GetT(idx));
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

    }
}
