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

using Highlander.Reporting.Analytics.V5r3.Processes;
using Highlander.Reporting.Analytics.V5r3.Rates;
using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Matrix = Highlander.Reporting.Analytics.V5r3.LinearAlgebra.Matrix;

namespace Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities
{
    /// <summary>
    /// Class that creates, computes and stores multi-factored forward rate volatilities, with homogeneous forward correlations.
    /// Given appropriate swaption weights, also computes all caplet and swaption implied volatilities of a particular strike.
    /// </summary>
    public class InterestRateVolatilities
    {
        #region Private Fields

        //intermediary values to reduce repeated calculations
        private readonly double[][] _storedImpliedVol;
        private readonly double[][] _storedImpliedVolSq;
        private readonly double[][][] _storedImpliedVolTerm;
        private readonly SwaptionWeights _weights;
        private readonly Matrix[] _volatility;

        public Matrix[] Volatility => (Matrix[])_volatility.Clone();

        //whether the vol is finalised
        private bool _volFixed;
        private readonly DenseMatrix _correlation;
        private readonly PedersenTimeGrid _timeGrid;
        private readonly int _factors;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeGrid">Time discretisation used.</param>
        /// <param name="factors">Number of factors in the model.</param>
        /// <param name="weights">Precomputed swaption weights.</param>
        /// <param name="correlation">A homogeneous forward correlation matrix.</param>
        public InterestRateVolatilities(PedersenTimeGrid timeGrid, int factors, SwaptionWeights weights, DenseMatrix correlation)
        {
            _volFixed = false;
            _timeGrid = timeGrid;
            _factors = factors;
            _weights = weights;
            _correlation = correlation;
            _volatility = new Matrix[_timeGrid.MaxExpiry];
            for (int i = 0; i < _timeGrid.MaxExpiry; i++)
            {
                _volatility[i] = new Matrix(_timeGrid.MaxTenor, _factors);
            }
            _storedImpliedVol = new double[_timeGrid.MaxExpiry][];
            _storedImpliedVolSq = new double[_timeGrid.MaxExpiry][];
            _storedImpliedVolTerm = new double[_timeGrid.MaxExpiry][][];
            for (int i = 0; i < _timeGrid.MaxExpiry; i++)
            {
                _storedImpliedVol[i] = new double[_timeGrid.MaxTenor - i];
                _storedImpliedVolSq[i] = new double[_timeGrid.MaxTenor - i];
                _storedImpliedVolTerm[i] = new double[_timeGrid.MaxTenor - i][];
                for (int j = 0; j < _timeGrid.MaxTenor - i; j++)
                {
                    _storedImpliedVolTerm[i][j] = new double[i + 1];
                }
            }
        }

        #endregion

        #region Volatility Modification Method

        /// <summary>
        /// Computes the multi-factored volatilities given a particular input volatility grid, using principal component analysis.
        /// (See Pedersen, M. B. (1998) Calibrating Libor market models. Working paper, Financial Research Department, SimCorp.)
        /// </summary>
        /// <param name="inputVolMat"></param>
        public void Populate(Matrix inputVolMat)
        {
            if (!_volFixed)
            {
                for (int i = 0; i < _timeGrid.ExpiryCount; i++)
                {
                    var currentRow = SingleRowOfVolatilities(i, inputVolMat.RowD(i));
                    for (int j = _timeGrid.ExpiryGrid[i]; j < _timeGrid.ExpiryGrid[i + 1]; j++)
                    {
                        for (int k = 0; k < _timeGrid.MaxTenor - j; k++)
                        {
                            for (int l = 0; l < _factors; l++)
                            {
                                _volatility[j][k, l] = currentRow[k, l];
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Cannot alter finalised volatility.");
            }
        }

        /// <summary>
        /// Scale particular volatilities, used under the cascade algorithm
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="ten"></param>
        /// <param name="scaleFactor"></param>
        public void Scale(int exp, int ten, double scaleFactor)
        {
            if (!_volFixed)
            {
                for (int k = 0; k < _factors; k++)
                {
                    _volatility[exp][ten, k] = _volatility[exp][ten, k] * scaleFactor;
                }
            }
            else
            {
                throw new Exception("Cannot alter finalised volatility.");
            }
        }

        /// <summary>
        /// Computes the volatilities of a particular expiry time bucket.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="inputVol"></param>
        /// <returns></returns>
        public Matrix SingleRowOfVolatilities(int row, Vector inputVol)
        {
            //create covariance
            var covariance = new Matrix(_timeGrid.TenorCount, _timeGrid.TenorCount);
            for (int i = 0; i < _timeGrid.TenorCount; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    covariance[i, j] = _correlation[i, j] * inputVol[i] * inputVol[j];
                }
            }
            //principal component analysis
            var eigenValueDecomposition = new EigenvalueDecomposition(covariance);
            var baseVector = eigenValueDecomposition.RealEigenvalues.ToList();
            var eval = baseVector.GetRange(_timeGrid.TenorCount -1 - _factors, _factors);//Select the sub-vector starting at the index i and with length y 
            var baseMatrix = eigenValueDecomposition.EigenVectors;
            var eigenVectors = baseMatrix.GetMatrix(0, baseMatrix.RowCount - 1, _timeGrid.TenorCount - _factors, _timeGrid.TenorCount - 1);
            var resultVol = new Matrix(_timeGrid.MaxTenor, _factors);
            for (int j = 0; j < _factors; j++)
            {
                eval[j] = Math.Sqrt(eval[j]);
                for (int i = 0; i < _timeGrid.TenorCount; i++)
                {
                    var temp = eval[j] * eigenVectors[i, j];
                    for (int n = _timeGrid.TenorGrid[i]; n < Math.Min(_timeGrid.TenorGrid[i + 1], _timeGrid.MaxTenor - _timeGrid.ExpiryGrid[row]); n++)
                    {
                        resultVol[n, j] = temp;
                    }
                }
            }
            return resultVol;
        }

        #endregion

        #region Implied Volatility

        /// <summary>
        /// Computes the implied volatility of swaption with given expiry and tenor, or a caplet with given expiry.
        /// A caplet implied volatility is equivalent to that of a swaption with the tenor length of 1.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <returns></returns>
        public double FindImpliedVolatility(int expiry, int tenor)
        {
            int exp = expiry - 1;
            int ten = tenor - 1;
            _storedImpliedVolSq[exp][ten] = 0;
            for (int i = 0; i <= exp; i++)
            {
                var tempVector = new DenseVector(_factors);
                for (int j = 0; j <= ten; j++)
                {
                    tempVector = tempVector + _weights.Get(expiry, tenor, j + 1) * _volatility[i].RowD(j + exp - i);
                }
                var tempSum = tempVector * tempVector;
                _storedImpliedVolSq[exp][ten] += tempSum;
                _storedImpliedVolTerm[exp][ten][i] = tempSum;
            }
            _storedImpliedVol[exp][ten] = Math.Sqrt(_storedImpliedVolSq[exp][ten] / expiry);
            double result = _storedImpliedVol[exp][ten];
            return result;
        }

        /// <summary>
        /// Computes the implied volatility of swaption with given expiry and tenor, or a caplet with given expiry, 
        /// with the volatility at one particular expiry time bucket replaced with temporary values.
        /// A caplet implied volatility is equivalent to that of a swaption with the tenor length of 1.
        /// Saves computation time when checking the effect of a simple perturbation.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <param name="newRow">Indicating the expiry time bucket to be replaced.</param>
        /// <param name="replacedRow">New interest rate volatility values for the expiry time bucket.</param>
        /// <returns></returns>
        public double FindImpliedVolatility(int expiry, int tenor, Matrix newRow, int replacedRow)
        {
            int exp = expiry - 1;
            int ten = tenor - 1;
            double result;
            if (exp >= _timeGrid.ExpiryGrid[replacedRow])
            {
                result = _storedImpliedVolSq[exp][ten];
                for (int i = _timeGrid.ExpiryGrid[replacedRow]; i < Math.Min(_timeGrid.ExpiryGrid[replacedRow + 1], expiry); i++)
                {
                    var tempVector = new DenseVector(_factors);
                    for (int j = 0; j <= ten; j++)
                    {
                        tempVector = tempVector + _weights.Get(expiry, tenor, j + 1) * newRow.RowD(j + exp - i);
                    }
                    double tempSum = tempVector * tempVector;
                    result += tempSum - _storedImpliedVolTerm[exp][ten][i];
                }
                result = Math.Sqrt(result / expiry);
            }
            else
            {
                result = _storedImpliedVol[exp][ten];
            }
            return result;
        }

        #endregion

        #region Returning Final Vol

        public InterestRateVolatilities FixAndReturnVol()
        {
            _volFixed = true;
            return this;
        }

        public double VolNorm(int expiry, int tenor)
        {
            double result = _volatility[expiry - 1].RowD(tenor - 1) * _volatility[expiry - 1].RowD(tenor - 1);
            result = Math.Sqrt(result);
            return result;
        }

        #endregion
    }
}