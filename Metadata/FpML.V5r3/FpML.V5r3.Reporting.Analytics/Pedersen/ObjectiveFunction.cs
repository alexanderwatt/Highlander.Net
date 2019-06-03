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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Double;

#endregion

namespace Orion.Analytics.Pedersen
{
    public class ObjectiveFunction
    {
        #region declarations

        public double Fval { get; set; }

        private readonly Parameters _param;
        private readonly Economy _economy;
        private Recycle _previous;

        public bool ExpForm { get; set; }

        private double _hfac;
        private double _vfac;
        private double _sfac;
        private double _cfac;

        public double HWeight { get; set; }

        public double VWeight { get; set; }

        public double CWeight { get; set; }

        public double SWeight { get; set; }

        private Thread[] _threads;
        public int NThreads { get; set; }

        private MyCounter _mc;
        public bool Finished { get; set; }

        private bool _evalgrad;

        private GeneralVector _grad;
        private GeneralVector _xgrad;

        private int _itercount;

        #endregion

        private bool _hit;

        public ObjectiveFunction(Economy eco, Recycle prev, Parameters p)
        {
            _param = p;
            _economy = eco;
            _previous = prev;
        }

        public void Initialise()
        {
            _itercount = 0;
            Finished = false;
            _evalgrad = false;
            _mc = new MyCounter();
            _xgrad = new GeneralVector();
            _hfac = 1.0 / (_param.NEXPIRY * (_param.Ntenor - 1));
            _vfac = 1.0 / ((_param.NEXPIRY - 1) * _param.Ntenor);
            _sfac = 1.0 / _param.Nswpn;
            _cfac = 1.0 / _param.Ncplt;
        }

        public void StartOtherThreads()
        {
            _threads = new Thread[NThreads - 1];
            for (int i = 0; i < NThreads - 1; i++)
            {
                _threads[i] = new Thread(ThreadGrad);
                _threads[i].Start();
            }
        }

        #region objective
        public double ObjFun(Vector x)
        {
            GeneralMatrix xmat = VecToMat(x);
            PopulateXi(xmat);
            double temp = Bound(x);
            if (temp > 0 && !ExpForm)
            {
                return temp;
            }
            if (_param.Ncplt > 0)
            {
                temp += CWeight * QOFCplt();
            } 
            if (_param.Nswpn > 0)
            {
                temp += SWeight * QOFSwpn();
            }
            temp += HWeight * SmoothH(xmat) + VWeight * SmoothV(xmat);
            Fval = temp;
            _itercount++;
            Orion.Pedersen.WriteRange(1, 1, $"{_itercount}, {temp:f9}, {_hit}");
            return temp;
        }

        private double ObjFun(Vector x, int r)
        {
            GeneralMatrix xmat = VecToMat(x);
            GeneralMatrix gm = SetXi(r, xmat[r, Range.All]);
            double temp = Bound(x);
            if (temp > 0 && !ExpForm)
            {
                return temp;
            }
            if (_param.Ncplt > 0)
            {
                temp += CWeight * QOFCplt(gm, r);
            }
            if (_param.Nswpn > 0)
            {
                temp += SWeight * QOFSwpn(gm, r);
            }
            temp += HWeight * SmoothH(xmat) + VWeight * SmoothV(xmat);
            return temp;
        }

        public Vector ObjGrad(Vector x, Vector f)
        {
            _mc = new MyCounter();
            _grad = new GeneralVector(x.Length);
            _xgrad = new GeneralVector(x.Length);
            if (f == null)
                f = new GeneralVector(x.Length);
            x.CopyTo(_xgrad);

            _evalgrad = true;

            EvaluateGrad();

            _grad.CopyTo(f);

            return f;
        }

        private void EvaluateGrad()
        {
            while (true)
            {
                int mytask;
                lock (_mc)
                {
                    if (_mc.Taskcount < _xgrad.Length)
                    {
                        mytask = _mc.Taskcount;
                        _mc.Taskcount++;
                    }
                    else
                    {
                        break;
                    }
                }
                var gv = new GeneralVector(_xgrad.Length);
                _xgrad.CopyTo(gv);

                int r = mytask / _param.Ntenor;
                gv[mytask] = _xgrad[mytask] + 1e-9;
                _grad[mytask] = 1e9 * (ObjFun(gv, r) - Fval);
            }
            _evalgrad = false;
        }

        private void ThreadGrad()
        {
            while (!Finished)
            {
                if (_evalgrad)
                {
                    EvaluateGrad();
                }
                Thread.Sleep(5);
            }
        }

        #region objective components
        private double Bound(IEnumerable<double> x)
        {
            _hit = false;
            double a = 0;
            if (ExpForm)
            {
                const double centre = -0.2;
                const double range = 0.4;
                foreach (double t in x)
                {
                    double temp = Math.Abs(t - centre);

                    if (temp > range)
                    {
                        _hit = true;
                        temp = (Math.Exp(temp) - 1);
                        a += 100 * temp * temp;
                    }
                }
            }
            else
            {
                a += x.Where(t => t <= 0).Sum(t => 200*(0.01 - t)*(0.01 - t));
            }
            return a;
        }
        private double SmoothH(GeneralMatrix xmat)
        {
            double total = 0;
            for (int i = 0; i < _param.NEXPIRY; i++)
            {
                for (int j = 0; j < _param.Ntenor - 1; j++)
                {
                    double temp = Math.Log(xmat[i, j] / xmat[i, j + 1]);
                    total += temp * temp;
                }
            }
            total = total * _hfac;
            return total;
        }
        private double SmoothV(GeneralMatrix xmat)
        {
            double total = 0;
            for (int i = 0; i < _param.NEXPIRY - 1; i++)
            {
                for (int j = 0; j < _param.Ntenor; j++)
                {
                    double temp = Math.Log(xmat[i, j] / xmat[i + 1, j]);
                    total += temp * temp;
                }
            }
            total = total * _vfac;
            return total;
        }
        private double QOFSwpn()
        {
            double total = 0;
            foreach (int t in _param.SwpnExp)
            {
                if (t > _param.Uexpiry - 1)
                {
                    break;
                }
                for (int j = 0; j < _param.SwpnTen.Length; j++)
                {
                    if (t + _param.SwpnTen[j] > _param.Utenor - 1)
                    {
                        break;
                    }
                    double tempivol = _economy.ReturnIvol(t, _param.SwpnTen[j]);
                    if (!(tempivol > 0)) continue;
                    double temp = Math.Log(_economy.FindIvol(t, _param.SwpnTen[j]) /
                                           tempivol);

                    total += temp * temp;
                }
            }
            total = total * _sfac;
            return total;
        }
        private double QOFSwpn(GeneralMatrix gm, int change)
        {
            double total = 0;
            foreach (int t in _param.SwpnExp)
            {
                if (t > _param.Uexpiry - 1)
                {
                    break;
                }
                for (int j = 0; j < _param.SwpnTen.Length; j++)
                {
                    if (t + _param.SwpnTen[j] > _param.Utenor - 1)
                    {
                        break;
                    }
                    double tempivol = _economy.ReturnIvol(t, _param.SwpnTen[j]);
                    if (tempivol > 0)
                    {
                        double temp = Math.Log(_economy.FindIvol(t, _param.SwpnTen[j], gm, change) /
                                               tempivol);
                        total += temp * temp;
                    }
                }
            }
            total = total * _sfac;
            return total;
        }
        private double QOFCplt()
        {
            double total = 0;
            for (int i = 0; i < _param.Tcplt; i++)
            {
                double tempivol = _economy.ReturnIvol(i);
                if (tempivol > 0)
                {
                    double temp = Math.Log(_economy.FindIvol(i, 0) / tempivol);
                    total += temp * temp;
                }
            }
            total = total * _cfac;
            return total;
        }
        private double QOFCplt(GeneralMatrix gm, int change)
        {
            double total = 0;
            for (int i = 0; i < _param.Tcplt; i++)
            {
                double tempivol = _economy.ReturnIvol(i);
                if (tempivol > 0)
                {
                    double temp = Math.Log(_economy.FindIvol(i, 0, gm, change) / tempivol);
                    total += temp * temp;
                }
            }
            total = total * _cfac;
            return total;
        }
        #endregion

        #endregion

        private void PopulateXi(GeneralMatrix xmat)
        {
            for (int i = 0; i < _param.NEXPIRY; i++)
            {
                GeneralMatrix gm = SetXi(i, xmat[i, Range.All]);
                for (int j = _param.Expiry[i]; j < _param.Expiry[i + 1]; j++)
                {
                    for (int k = 0; k < _param.Utenor - j; k++)
                    {
                        for (int l = 0; l < _param.NFAC; l++)
                        {
                            _economy.Xi[j][k, l] = gm[k, l];
                        }
                    }
                    //economy.Xi[j] = (GeneralMatrix)gm.Clone();
                }
            }
        }

        private GeneralMatrix SetXi(int exp, Vector x)
        {
            SymmetricMatrix cor = _economy.Correlation;
            var cov = new SymmetricMatrix(_param.Ntenor);
            for (int i = 0; i < _param.Ntenor; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cov[i, j] = cor[i, j] * x[i] * x[j];
                }
            }

            var eign = new SymmetricEigenvalueDecomposition(cov, true);


            //for (int i=1;i<10;i++)
            //{
            //    if (eign.Eigenvalues[i-1]>eign.Eigenvalues[i])
            //    {
            //        Console.WriteLine(eign.Eigenvalues);
            //    }
            //}
            var eval = (GeneralVector)eign.Eigenvalues[new Range(_param.Ntenor - _param.NFAC, _param.Ntenor - 1)];
            var evec = (GeneralMatrix)eign.Eigenvectors[Range.All, new Range(_param.Ntenor - _param.NFAC, _param.Ntenor - 1)];
            var xisection = new GeneralMatrix(_param.Utenor, _param.NFAC);
            for (int j = 0; j < _param.NFAC; j++)
            {
                eval[j] = Math.Sqrt(eval[j]);
                for (int i = 0; i < _param.Ntenor; i++)
                {
                    double temp = eval[j] * evec[i, j];
                    for (int n = _param.Tenor[i]; n < Math.Min(_param.Tenor[i + 1], _param.Utenor - _param.Expiry[exp]); n++)
                    {
                        xisection[n, j] = temp;
                    }
                }
            }
            return xisection;
        }

        public void OutputResult(Vector x)
        {
            GeneralMatrix xmat = VecToMat(x);
            PopulateXi(xmat);
            _economy.OutputResult();
        }

        public GeneralMatrix VecToMat(Vector x)
        {
            var tempmat = new GeneralMatrix(_param.NEXPIRY, _param.Ntenor);
            if (ExpForm)
            {
                for (int i = 0; i < _param.NEXPIRY; i++)
                {
                    for (int j = 0; j < _param.Ntenor; j++)
                    {
                        tempmat[i, j] = Math.Exp(x[i * _param.Ntenor + j] * 20);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _param.NEXPIRY; i++)
                {
                    for (int j = 0; j < _param.Ntenor; j++)
                    {
                        tempmat[i, j] = x[i * _param.Ntenor + j];
                    }
                }
            }
            return tempmat;
        }
    }

    /*
     * For thread locking
     */
    class MyCounter
    {
        public int Taskcount { get; set; }
    }
}
