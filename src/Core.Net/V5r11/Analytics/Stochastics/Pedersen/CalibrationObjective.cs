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
using System.Diagnostics;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Highlander.Reporting.Analytics.V5r3.Processes;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Matrix = Highlander.Reporting.Analytics.V5r3.LinearAlgebra.Matrix;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Stochastics.Pedersen
{
    class CalibrationObjective
    {
        #region Private Fields

        private readonly CalibrationTargets _targets;
        private readonly PedersenTimeGrid _timeGrid;
        private readonly InterestRateVolatilities _volatilities;
        private readonly double _hFactor;
        private readonly double _vFactor;
        private readonly double _sFactor;
        private readonly double _cFactor;
        private readonly CalibrationSettings _parameters;
        private double _lastObjFunVal;
        private Thread[] _threads;
        private TaskCounter _taskCounter;
        private bool _gradPhase;

        public bool Finished { get; set; }

        private Vector<double> _currentInput;
        private Vector<double> _grad;
        private int _iterationCounter;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targets">Calibration targets containing swaption and caplet data</param>
        /// <param name="timeGrid">Time discretisation</param>
        /// <param name="volatilities">Volatility to be calibrated</param>
        /// <param name="parameters">Calibration parameter settings</param>
        public CalibrationObjective(CalibrationTargets targets, PedersenTimeGrid timeGrid, 
                                    InterestRateVolatilities volatilities, CalibrationSettings parameters)
        {
            Finished = false;
            _gradPhase = false;
            _iterationCounter = 0;
            _taskCounter = new TaskCounter();
            //_currentInput = new DenseVector(0);
            _targets = targets;
            _timeGrid = timeGrid;
            _volatilities = volatilities;
            _parameters = parameters;
            _hFactor = 1.0 / (_timeGrid.ExpiryCount * (_timeGrid.TenorCount - 1));
            _vFactor = 1.0 / ((_timeGrid.ExpiryCount - 1) * _timeGrid.TenorCount);
            _sFactor = 1.0 / _targets.SwaptionCount;
            _cFactor = 1.0 / _targets.CapletCount;
        }

        #endregion

        #region Objective Function and Gradient + Threads

        /// <summary>
        /// Main objective function
        /// </summary>
        /// <param name="inputVolVec"></param>
        /// <returns></returns>
        public double ObjFun(Vector<double> inputVolVec)
        {
            var inputVolMat = VecToMat(inputVolVec);
            _volatilities.Populate(inputVolMat);
            double result = Bound(inputVolVec);
            if (result > 0 && !_parameters.ExponentialForm)
            {
                return result;
            }
            if (_targets.CapletCount > 0)
            {
                result += _parameters.CapletWeight * QOFCaplet();
            }
            if (_targets.SwaptionCount > 0)
            {
                result += _parameters.SwaptionWeight * QOFSwaption();
            }
            result += _parameters.HSmoothWeight * SmoothH(inputVolMat) + _parameters.VSmoothWeight * SmoothV(inputVolMat);
            _lastObjFunVal = result;
            _iterationCounter++;
            Trace.WriteLine($"Objective Function call #{_iterationCounter}, Output {_lastObjFunVal:f9}");
            return result;
        }

        /// <summary>
        /// Modified objective functions, used for calculating gradient function.
        /// </summary>
        /// <param name="inputVolVec"></param>
        /// <param name="replacedRow"></param>
        /// <returns></returns>
        private double ObjFun(Vector inputVolVec, int replacedRow)
        {
            var inputVolMat = VecToMat(inputVolVec);
            var newRow = _volatilities.SingleRowOfVolatilities(replacedRow, inputVolMat.RowD(replacedRow));
            double result = Bound(inputVolVec);
            if (result > 0 && !_parameters.ExponentialForm)
            {
                return result;
            }
            if (_targets.CapletCount > 0)
            {
                result += _parameters.CapletWeight * QOFCaplet(newRow, replacedRow);
            }
            if (_targets.SwaptionCount > 0)
            {
                result += _parameters.SwaptionWeight * QOFSwaption(newRow, replacedRow);
            }
            result += _parameters.HSmoothWeight * SmoothH(inputVolMat) + _parameters.VSmoothWeight * SmoothV(inputVolMat);
            return result;
        }

        /// <summary>
        /// Gradient Function
        /// </summary>
        /// <param name="inputVolVec"></param>
        /// <returns></returns>
        public Vector<double> ObjGrad(Vector<double> inputVolVec)
        {
            _taskCounter = new TaskCounter();
            _grad = new DenseVector(inputVolVec.Count);
            var result = new DenseVector(inputVolVec.Count);
            _currentInput = new DenseVector(inputVolVec.Count);
            inputVolVec.CopyTo(_currentInput);
            _gradPhase = true;
            EvaluateGrad();
            _grad.CopyTo(result);
            return result;
        }

        /// <summary>
        /// Actual gradient evaluation occurs here.
        /// </summary>
        private void EvaluateGrad()
        {
            while (true)
            {
                int thisTask;
                lock (_taskCounter)
                {
                    if (_taskCounter.TaskCount < _currentInput.Count)
                    {
                        thisTask = _taskCounter.TaskCount;
                        _taskCounter.TaskCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                var perturbedInput = new DenseVector(_currentInput.Count);
                _currentInput.CopyTo(perturbedInput);
                int replacedRow = thisTask / _timeGrid.TenorCount;
                perturbedInput[thisTask] = _currentInput[thisTask] + 1e-9;
                _grad[thisTask] = 1e9 * (ObjFun(perturbedInput, replacedRow) - _lastObjFunVal);
            }
            _gradPhase = false;
        }

        /// <summary>
        /// Starting auxiliary threads for gradient calculations.
        /// </summary>
        public void StartOtherThreads()
        {
            _threads = new Thread[_parameters.ThreadCount - 1];
            for (int i = 0; i < _parameters.ThreadCount - 1; i++)
            {
                _threads[i] = new Thread(ThreadGrad);
                _threads[i].Start();
            }
        }

        /// <summary>
        /// Thread start function for extra threads
        /// </summary>
        private void ThreadGrad()
        {
            while (!Finished)
            {
                if (_gradPhase)
                {
                    EvaluateGrad();
                }
                Thread.Sleep(5);
            }
        }

        #region Objective components

        /// <summary>
        /// Used to detect extremely large or small inputs. To steer optimisation routine away from those regions and prevent overflow.
        /// </summary>
        /// <param name="inputVolVec"></param>
        /// <returns></returns>
        private double Bound(Vector<double> inputVolVec)
        {
            double a = 0;
            if (_parameters.ExponentialForm)
            {
                const double centre = -0.2;
                const double range = 0.4;
                foreach (var volatility in inputVolVec)
                {
                    double temp = Math.Abs(volatility - centre);

                    if (temp > range)
                    {
                        temp = (Math.Exp(temp) - 1);
                        a += 100 * temp * temp;
                    }
                }
            }
            else
            {
                foreach (var volatility in inputVolVec)
                {
                    if (volatility <= 0)
                    {
                        a += 200 * (0.01 - volatility) * (0.01 - volatility);
                    }
                }
            }
            return a;
        }

        /// <summary>
        /// Horizontal smoothing.
        /// </summary>
        /// <param name="inputVolMat"></param>
        /// <returns></returns>
        private double SmoothH(Matrix inputVolMat)
        {
            double total = 0;
            for (int i = 0; i < _timeGrid.ExpiryCount; i++)
            {
                for (int j = 0; j < _timeGrid.TenorCount - 1; j++)
                {
                    double temp = Math.Log(inputVolMat[i, j] / inputVolMat[i, j + 1]);
                    total += temp * temp;
                }
            }
            total = total * _hFactor;
            return total;
        }

        /// <summary>
        /// Vertical smoothing.
        /// </summary>
        /// <param name="inputVolMat"></param>
        /// <returns></returns>
        private double SmoothV(Matrix inputVolMat)
        {
            double total = 0;
            for (int i = 0; i < _timeGrid.ExpiryCount - 1; i++)
            {
                for (int j = 0; j < _timeGrid.TenorCount; j++)
                {
                    double temp = Math.Log(inputVolMat[i, j] / inputVolMat[i + 1, j]);
                    total += temp * temp;
                }
            }
            total = total * _vFactor;
            return total;
        }

        /// <summary>
        /// Quality of fit for swaptions.
        /// </summary>
        /// <returns></returns>
        private double QOFSwaption()
        {
            double total = 0;
            for (int i = 0; i < _targets.SwaptionCount; i++)
            {
                if (_targets.SwaptionImpliedVol[i] > 0)
                {
                    double temp = Math.Log(_volatilities.FindImpliedVolatility(_targets.SwaptionExpiry[i], _targets.SwaptionTenor[i])
                                           / _targets.SwaptionImpliedVol[i]);
                    total += temp * temp;
                }
            }
            total = total * _sFactor;
            return total;
        }

        /// <summary>
        /// Quality of fit for swaptions, used when the volatility is perturbed during gradient calculations.
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="replacedRow"></param>
        /// <returns></returns>
        private double QOFSwaption(Matrix newRow, int replacedRow)
        {
            double total = 0;
            for (int i = 0; i < _targets.SwaptionCount; i++)
            {
                if (_targets.SwaptionImpliedVol[i] > 0)
                {
                    double temp = Math.Log(_volatilities.FindImpliedVolatility(_targets.SwaptionExpiry[i], _targets.SwaptionTenor[i], newRow, replacedRow)
                                           / _targets.SwaptionImpliedVol[i]);
                    total += temp * temp;
                }
            }
            total = total * _sFactor;
            return total;
        }

        /// <summary>
        /// Quality of fit for caplets.
        /// </summary>
        /// <returns></returns>
        private double QOFCaplet()
        {
            double total = 0;
            for (int i = 0; i < _targets.CapletCount; i++)
            {
                if (_targets.CapletImpliedVol[i] > 0)
                {
                    double temp = Math.Log(_volatilities.FindImpliedVolatility(_targets.CapletExpiry[i], 1)
                                           / _targets.CapletImpliedVol[i]);
                    total += temp * temp;
                }
            }
            total = total * _cFactor;
            return total;
        }

        /// <summary>
        /// Quality of fit for caplets, used when the volatility is perturbed during gradient calculations.
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="replacedRow"></param>
        /// <returns></returns>
        private double QOFCaplet(Matrix newRow, int replacedRow)
        {
            double total = 0;
            for (int i = 0; i < _targets.CapletCount; i++)
            {
                if (_targets.CapletImpliedVol[i] > 0)
                {
                    double temp = Math.Log(_volatilities.FindImpliedVolatility(_targets.CapletExpiry[i], 1, newRow, replacedRow)
                                           / _targets.CapletImpliedVol[i]);
                    total += temp * temp;
                }
            }
            total = total * _cFactor;
            return total;
        }

        #endregion

        #endregion

        #region Volatility Access/Modification

        /// <summary>
        /// Converts the input vector into a matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Matrix VecToMat(Vector<double> x)
        {
            var tempMatrix = new Matrix(_timeGrid.ExpiryCount, _timeGrid.TenorCount);
            if (_parameters.ExponentialForm)
            {
                for (int i = 0; i < _timeGrid.ExpiryCount; i++)
                {
                    for (int j = 0; j < _timeGrid.TenorCount; j++)
                    {
                        tempMatrix[i, j] = Math.Exp(x[i * _timeGrid.TenorCount + j] * 20);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _timeGrid.ExpiryCount; i++)
                {
                    for (int j = 0; j < _timeGrid.TenorCount; j++)
                    {
                        tempMatrix[i, j] = x[i * _timeGrid.TenorCount + j];
                    }
                }
            }
            return tempMatrix;
        }

        /// <summary>
        /// Populates the volatility from an input vector
        /// </summary>
        /// <param name="inputVolVec"></param>
        public void PopulateVol(Vector<double> inputVolVec)
        {
            var inputVolMat = VecToMat(inputVolVec);
            _volatilities.Populate(inputVolMat);
        }

        /// <summary>
        /// Returns the InterestRateVolatilities object post-calibration.
        /// </summary>
        /// <param name="inputVolVec"></param>
        /// <returns></returns>
        public InterestRateVolatilities ReturnVol(Vector<double> inputVolVec)
        {
            return _volatilities.FixAndReturnVol();
        }

        /// <summary>
        /// For debugging
        /// </summary>
        public string AverageAbsVol()
        {
            string returnString = "";
            double sum = 0;
            double temp;
            if (_targets.SwaptionCount > 0)
            {
                for (int i = 0; i < _targets.SwaptionCount; i++)
                {
                    if (_targets.SwaptionImpliedVol[i] > 0)
                    {
                        temp = _volatilities.FindImpliedVolatility(_targets.SwaptionExpiry[i], _targets.SwaptionTenor[i]);
                        sum += Math.Abs(temp - _targets.SwaptionImpliedVol[i]);
                    }
                }
                temp = sum / _targets.SwaptionCount;
                returnString = $"Average Swaption Absolute Error: {temp}; ";
                Trace.WriteLine($"Average Swaption Absolute Error: {temp}");
            }
            else
            {
                Trace.WriteLine(String.Format("Average Swaption Absolute Error: N/A"));
            }
            sum = 0;
            if (_targets.CapletCount > 0)
            {
                for (int i = 0; i < _targets.CapletCount; i++)
                {
                    if (_targets.CapletImpliedVol[i] > 0)
                    {
                        temp = _volatilities.FindImpliedVolatility(_targets.CapletExpiry[i], 1);
                        sum += Math.Abs(temp - _targets.CapletImpliedVol[i]);
                    }
                }
                temp = sum / _targets.CapletCount;
                returnString = returnString + $"Average Caplet Absolute Error: {temp}";
                Trace.WriteLine($"Average Caplet Absolute Error: {temp}");
            }
            else
            {
                Trace.WriteLine(String.Format("Average Caplet Absolute Error: N/A"));
            }

            return returnString;
        }
        #endregion
    }

    /// <summary>
    /// Class for task counting during the multi-threaded gradient calculation 
    /// </summary>
    class TaskCounter
    {
        public int TaskCount { get; set; }
    }
}