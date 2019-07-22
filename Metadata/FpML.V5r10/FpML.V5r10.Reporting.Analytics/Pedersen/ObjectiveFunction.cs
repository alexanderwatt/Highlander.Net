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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Highlander.Numerics.LinearAlgebra;
using Highlander.Numerics.Pedersen;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = Highlander.Numerics.LinearAlgebra.Matrix;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Pedersen
{
    public class ObjectiveFunction
    {
        #region Declarations

        public double FunctionValue { get; set; }

        private readonly Parameters _param;
        private readonly Economy _economy;
        private Recycle _previous;

        public bool ExpForm { get; set; }

        private double _hFactor;
        private double _vFactor;
        private double _sFactor;
        private double _cFactor;

        public double HWeight { get; set; }

        public double VWeight { get; set; }

        public double CWeight { get; set; }

        public double SWeight { get; set; }

        private Thread[] _threads;
        public int NThreads { get; set; }

        private MyCounter _mc;
        public bool Finished { get; set; }

        private bool _evaluateGradient;

        private Vector _gradient;
        private Vector _xGradient;

        private int _iterationCount;

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
            _iterationCount = 0;
            Finished = false;
            _evaluateGradient = false;
            _mc = new MyCounter();
            _hFactor = 1.0 / (_param.NumberOfExpiries * (_param.NumberOfTenors - 1));
            _vFactor = 1.0 / ((_param.NumberOfExpiries - 1) * _param.NumberOfTenors);
            _sFactor = 1.0 / _param.NumberOfSwaptions;
            _cFactor = 1.0 / _param.NumberOfCaplets;
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

        #region Objective Function

        public double ObjFun(Vector<double> x)
        {
            var xMatrix = VecToMat(x);
            PopulateXi(xMatrix);
            double temp = Bound(x);
            if (temp > 0 && !ExpForm)
            {
                return temp;
            }
            if (_param.NumberOfCaplets > 0)
            {
                temp += CWeight * QOFCplt();
            } 
            if (_param.NumberOfSwaptions > 0)
            {
                temp += SWeight * QOFSwpn();
            }
            temp += HWeight * SmoothH(xMatrix) + VWeight * SmoothV(xMatrix);
            FunctionValue = temp;
            _iterationCount++;
            Pedersen.WriteRange(1, 1, $"{_iterationCount}, {temp:f9}, {_hit}");
            return temp;
        }

        private double ObjFun(Vector<double> x, int r)
        {
            var xMatrix = VecToMat(x);
            var gm = SetXi(r, (DenseVector)xMatrix.Row(r));
            double temp = Bound(x);
            if (temp > 0 && !ExpForm)
            {
                return temp;
            }
            if (_param.NumberOfCaplets > 0)
            {
                temp += CWeight * QOFCplt(gm, r);
            }
            if (_param.NumberOfSwaptions > 0)
            {
                temp += SWeight * QOFSwpn(gm, r);
            }
            temp += HWeight * SmoothH(xMatrix) + VWeight * SmoothV(xMatrix);
            return temp;
        }

        public Vector<double> ObjGrad(Vector<double> x)//, Vector f
        {
            _mc = new MyCounter();
            _gradient = new DenseVector(x.Count);
            _xGradient = new DenseVector(x.Count);
            //if (f == null)
            var f = new DenseVector(x.Count);
            x.CopyTo(_xGradient);
            _evaluateGradient = true;
            EvaluateGrad();
            _gradient.CopyTo(f);
            return f;
        }

        private void EvaluateGrad()
        {
            while (true)
            {
                int myTask;
                lock (_mc)
                {
                    if (_mc.TaskCount < _xGradient.Count)
                    {
                        myTask = _mc.TaskCount;
                        _mc.TaskCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                var gv = new DenseVector(_xGradient.Count);
                _xGradient.CopyTo(gv);
                int r = myTask / _param.NumberOfTenors;
                gv[myTask] = _xGradient[myTask] + 1e-9;
                _gradient[myTask] = 1e9 * (ObjFun(gv, r) - FunctionValue);
            }
            _evaluateGradient = false;
        }

        private void ThreadGrad()
        {
            while (!Finished)
            {
                if (_evaluateGradient)
                {
                    EvaluateGrad();
                }
                Thread.Sleep(5);
            }
        }

        #region Objective Components

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
                    double temp = System.Math.Abs(t - centre);

                    if (temp > range)
                    {
                        _hit = true;
                        temp = (System.Math.Exp(temp) - 1);
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

        private double SmoothH(Matrix<double> xMatrix)
        {
            double total = 0;
            for (int i = 0; i < _param.NumberOfExpiries; i++)
            {
                for (int j = 0; j < _param.NumberOfTenors - 1; j++)
                {
                    double temp = System.Math.Log(xMatrix[i, j] / xMatrix[i, j + 1]);
                    total += temp * temp;
                }
            }
            total = total * _hFactor;
            return total;
        }

        private double SmoothV(Matrix<double> xMatrix)
        {
            double total = 0;
            for (int i = 0; i < _param.NumberOfExpiries - 1; i++)
            {
                for (int j = 0; j < _param.NumberOfTenors; j++)
                {
                    double temp = System.Math.Log(xMatrix[i, j] / xMatrix[i + 1, j]);
                    total += temp * temp;
                }
            }
            total = total * _vFactor;
            return total;
        }
        private double QOFSwpn()
        {
            double total = 0;
            foreach (int t in _param.SwaptionExpiries)
            {
                if (t > _param.UnderlyingExpiry - 1)
                {
                    break;
                }
                foreach (var t1 in _param.SwaptionTenors)
                {
                    if (t + t1 > _param.UnderlyingTenor - 1)
                    {
                        break;
                    }
                    double tempImpliedVolatility = _economy.ReturnImpliedVolatility(t, t1);
                    if (!(tempImpliedVolatility > 0)) continue;
                    double temp = System.Math.Log(_economy.FindImpliedVolatility(t, t1) /
                                           tempImpliedVolatility);
                    total += temp * temp;
                }
            }
            total = total * _sFactor;
            return total;
        }

        private double QOFSwpn(DenseMatrix gm, int change)
        {
            double total = 0;
            foreach (int t in _param.SwaptionExpiries)
            {
                if (t > _param.UnderlyingExpiry - 1)
                {
                    break;
                }
                foreach (var t1 in _param.SwaptionTenors)
                {
                    if (t + t1 > _param.UnderlyingTenor - 1)
                    {
                        break;
                    }
                    double tempImpliedVolatility = _economy.ReturnImpliedVolatility(t, t1);
                    if (tempImpliedVolatility > 0)
                    {
                        double temp = System.Math.Log(_economy.FindImpliedVolatility(t, t1, gm, change) /
                                               tempImpliedVolatility);
                        total += temp * temp;
                    }
                }
            }
            total = total * _sFactor;
            return total;
        }

        private double QOFCplt()
        {
            double total = 0;
            for (int i = 0; i < _param.CapletTenors; i++)
            {
                double tempImpliedVolatility = _economy.ReturnImpliedVolatility(i);
                if (tempImpliedVolatility > 0)
                {
                    double temp = System.Math.Log(_economy.FindImpliedVolatility(i, 0) / tempImpliedVolatility);
                    total += temp * temp;
                }
            }
            total = total * _cFactor;
            return total;
        }

        private double QOFCplt(DenseMatrix gm, int change)
        {
            double total = 0;
            for (int i = 0; i < _param.CapletTenors; i++)
            {
                double tempImpliedVolatility = _economy.ReturnImpliedVolatility(i);
                if (tempImpliedVolatility > 0)
                {
                    double temp = System.Math.Log(_economy.FindImpliedVolatility(i, 0, gm, change) / tempImpliedVolatility);
                    total += temp * temp;
                }
            }
            total = total * _cFactor;
            return total;
        }

        #endregion

        #endregion

        private void PopulateXi(Matrix<double> xMatrix)
        {
            for (int i = 0; i < _param.NumberOfExpiries; i++)
            {
                var gm = SetXi(i, (DenseVector)xMatrix.Row(i));
                for (int j = _param.Expiry[i]; j < _param.Expiry[i + 1]; j++)
                {
                    for (int k = 0; k < _param.UnderlyingTenor - j; k++)
                    {
                        for (int l = 0; l < _param.NumberOfFactors; l++)
                        {
                            _economy.Xi[j][k, l] = gm[k, l];
                        }
                    }
                    //economy.Xi[j] = (GeneralMatrix)gm.Clone();
                }
            }
        }

        private DenseMatrix SetXi(int exp, Vector x)
        {
            var cor = _economy.Correlation;
            var cov = new Matrix(_param.NumberOfTenors, _param.NumberOfTenors);
            for (int i = 0; i < _param.NumberOfTenors; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cov[i, j] = cor[i, j] * x[i] * x[j];
                }
            }
            var eigenValueDecomposition = new EigenvalueDecomposition(cov);
            var baseVector = eigenValueDecomposition.RealEigenvalues.ToList();
            var eval = baseVector.GetRange(_param.NumberOfTenors - _param.NumberOfFactors, _param.NumberOfTenors - 1 + _param.NumberOfFactors);//Select the sub-vector starting at the index i and with length y 
            var baseMatrix = eigenValueDecomposition.EigenVectors;
            var eigenVectors = baseMatrix.GetMatrix(0, baseMatrix.RowCount - 1, _param.NumberOfTenors - _param.NumberOfFactors, _param.NumberOfTenors - 1);
            var xiSection = new DenseMatrix(_param.UnderlyingTenor, _param.NumberOfFactors);
            for (int j = 0; j < _param.NumberOfFactors; j++)
            {
                eval[j] = System.Math.Sqrt(eval[j]);
                for (int i = 0; i < _param.NumberOfTenors; i++)
                {
                    var temp = eval[j] * eigenVectors[i, j];
                    for (int n = _param.Tenor[i]; n < System.Math.Min(_param.Tenor[i + 1], _param.UnderlyingTenor - _param.Expiry[exp]); n++)
                    {
                        xiSection[n, j] = temp;
                    }
                }
            }
            return xiSection;
        }

        public void OutputResult(Vector<double> x)
        {
            PopulateXi(VecToMat(x));
            _economy.OutputResult();
        }

        public Matrix<double> VecToMat(Vector<double> x)
        {
            var tempMatrix = new DenseMatrix(_param.NumberOfExpiries, _param.NumberOfTenors);
            if (ExpForm)
            {
                for (int i = 0; i < _param.NumberOfExpiries; i++)
                {
                    for (int j = 0; j < _param.NumberOfTenors; j++)
                    {
                        tempMatrix[i, j] = System.Math.Exp(x[i * _param.NumberOfTenors + j] * 20);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _param.NumberOfExpiries; i++)
                {
                    for (int j = 0; j < _param.NumberOfTenors; j++)
                    {
                        tempMatrix[i, j] = x[i * _param.NumberOfTenors + j];
                    }
                }
            }
            return tempMatrix;
        }
    }

    /*
     * For thread locking
     */
    class MyCounter
    {
        public int TaskCount { get; set; }
    }
}
