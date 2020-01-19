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
using Highlander.Numerics.Processes;
using Highlander.Numerics.Rates;
using Highlander.Numerics.Stochastics.Volatilities;
using MathNet.Numerics.LinearAlgebra.Double;

#endregion

namespace Highlander.Numerics.Stochastics.Pedersen
{
    /// <summary>
    /// Given a set of volatilities approximately fitting the data, 
    /// the cascade algorithm scales each volatility entry (without 
    /// altering correlation) to exact fit each of the data points
    /// as much as possible.
    /// </summary>
    class CascadeAlgorithm
    {
        #region Private Fields

        private readonly double[][] _scales;
        private readonly bool[][] _isScaled;
        private readonly CalibrationTargets _targets;
        private readonly PedersenTimeGrid _timeGrid;
        private readonly InterestRateVolatilities _volatilities;
        private readonly CascadeParameters _parameters;
        private readonly SwaptionWeights _swaptionWeights;
        private readonly int _factors;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="volatilities">Existing volatilities</param>
        /// <param name="timeGrid">Time discretisation</param>
        /// <param name="swaptionWeights"></param>
        /// <param name="targets"></param>
        /// <param name="parameters"></param>
        /// <param name="factors"></param>
        public CascadeAlgorithm(InterestRateVolatilities volatilities, PedersenTimeGrid timeGrid,
                                SwaptionWeights swaptionWeights, CalibrationTargets targets, CascadeParameters parameters, int factors)
        {
            _targets = targets;
            _timeGrid = timeGrid;
            _volatilities = volatilities;
            _parameters = parameters;
            _swaptionWeights = swaptionWeights;
            _factors = factors;
            _scales = new double[_timeGrid.MaxExpiry][];
            _isScaled = new bool[_timeGrid.MaxExpiry][];
            for (int i = 0; i < _timeGrid.MaxExpiry; i++)
            {
                _scales[i] = new double[_timeGrid.MaxTenor - i];
                _isScaled[i] = new bool[_timeGrid.MaxTenor - i];
            }
        }

        #endregion

        #region Algorithm

        /// <summary>
        /// Main method
        /// </summary>
        public void ApplyCascade()
        {
            for (int i = 1; i <= _timeGrid.MaxExpiry; i++)
            {
                for (int j = 1; j <= _timeGrid.MaxTenor - i + 1; j++)
                {
                    double target;
                    double maxScale;
                    if (j == 1)
                    {
                        target = _targets.GetCapletImpliedVol(i);
                        maxScale = _parameters.CapletMaxScale;
                    }
                    else
                    {
                        target = _targets.GetSwaptionImpliedVol(i, j);
                        maxScale = _parameters.SwaptionMaxScale;
                    }
                    //Once a non zero calibration target is found, scale vols to match it
                    if (target > 0)
                    {
                        var tempScale = FindScale(i, j, maxScale, target);
                        ApplyScale(i, j, tempScale);
                    }
                }
            }
        }

        /// <summary>
        /// Computes volatility scale for a given target. Equivalent to solving (or finding minimum if there is no solution) a quadratic.
        /// The scale is only applied to volatilities which has not been scaled already (towards another target).
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <param name="bound"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private double FindScale(int expiry, int tenor, double bound, double target)
        {
            int exp = expiry - 1;
            int ten = tenor - 1;
            double c0 = 0;
            double c1 = 0;
            double c2 = 0;
            double result;
            var tempVector1 = new DenseVector(_factors);
            var tempVector2 = new DenseVector(_factors);
            for (int i = 0; i <= exp; i++)
            {
                tempVector1.Clear();
                tempVector2.Clear();
                for (int j = 0; j <= ten; j++)
                {
                    if (_isScaled[i][j + exp - i])
                    {
                        tempVector1 = tempVector1 + _swaptionWeights.Get(expiry, tenor, j + 1) * _volatilities.Volatility[i].RowD(j + exp - i);
                    }
                    else
                    {
                        tempVector2 = tempVector2 + _swaptionWeights.Get(expiry, tenor, j + 1) * _volatilities.Volatility[i].RowD(j + exp - i);
                    }
                }
                c0 += tempVector1 * tempVector1;
                c1 += tempVector1 * tempVector2;
                c2 += tempVector2 * tempVector2;
            }
            c1 = 2 * c1;
            c0 -= target * target * expiry;
            //quadratic discriminant
            double disc = c1 * c1 - 4 * c2 * c0;
            if (disc >= 0)
            {
                //find positive root, if exists
                result = (-c1 + Math.Sqrt(disc)) / (2 * c2);
            }
            else
            {
                //find minimum if no roots exist
                result = -c1 / (2 * c2);
            }
            if (result > bound)
            {
                result = bound;
            }
            else if (result < 1.0 / bound)
            {
                result = 1.0 / bound;
            }
            return result;
        }

        /// <summary>
        /// Applying scale to volatilities that have yet to be scaled.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <param name="scale"></param>
        private void ApplyScale(int expiry, int tenor, double scale)
        {
            int exp = expiry - 1;
            int ten = tenor - 1;
            for (int i = 0; i <= exp; i++)
            {
                for (int j = 0; j <= ten; j++)
                {
                    if (!_isScaled[i][j + exp - i])
                    {
                        _isScaled[i][j + exp - i] = true;
                        _scales[i][j + exp - i] = scale;
                        _volatilities.Scale(i, j + exp - i, scale);
                    }
                }
            }
        }

        #endregion
    }
}