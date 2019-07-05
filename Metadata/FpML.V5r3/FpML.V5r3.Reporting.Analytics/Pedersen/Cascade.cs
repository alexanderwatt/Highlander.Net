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

using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Orion.Analytics.Pedersen
{
    public class Cascade
    {
        #region Declarations

        private readonly Parameters _param;
        private readonly Economy _economy;
        private double[][] _gamma;
        private bool[][] _isChanged;
        public double SwpnBound { get; set; }

        public double CpltBound { get; set; }

        #endregion

        public Cascade(Economy eco, Parameters p)
        {
            _param = p;
            _economy = eco;
        }

        public void Go()
        {
            _gamma = new double[_param.UnderlyingExpiry][];
            _isChanged = new bool[_param.UnderlyingExpiry][];
            for (int i = 0; i < _param.UnderlyingExpiry; i++)
            {
                _gamma[i] = new double[_param.UnderlyingTenor - i];
                _isChanged[i] = new bool[_param.UnderlyingTenor - i];
            }
            for (int i = 0; i < _param.UnderlyingExpiry; i++)
            {
                for (int j = 0; j < _param.UnderlyingTenor - i; j++)
                {
                    double target;
                    double bound;
                    if (j == 0)
                    {
                        target = _economy.ReturnImpliedVolatility(i);
                        bound = CpltBound;
                    }
                    else
                    {
                        target = _economy.ReturnImpliedVolatility(i , j);
                        bound = SwpnBound;
                    }
                    if (target > 0)
                    {
                        double tempGamma = FindGamma(i, j, bound, target);
                        ScaleXi(i, j, tempGamma);
                    }
                }
            }
            _economy.OutputResult();
        }

        private double FindGamma(int exp, int ten, double bound, double target)
        {
            double c0 = 0;
            double c1 = 0;
            double c2 = 0;
            double result;
            var tempVector1 = new DenseVector(_param.NumberOfFactors);
            var tempVector2 = new DenseVector(_param.NumberOfFactors);
            var xi = _economy.Xi;
            double[][][] ai = _economy.Ai;
            for (int i = 0; i <= exp; i++)
            {
                tempVector1.Clear();
                tempVector2.Clear();
                for (int j = exp - i; j <= exp - i + ten; j++)
                {
                    if (_isChanged[i][j])
                    {
                        tempVector1 = tempVector1 + ai[exp][ten][j - exp + i] * xi[i].RowD(j);
                    }
                    else
                    {
                        tempVector2 = tempVector2 + ai[exp][ten][j - exp + i] * xi[i].RowD(j);
                    }
                }
                c0 += tempVector1 * tempVector1;
                c1 += tempVector1 * tempVector2;
                c2 += tempVector2 * tempVector2;
            }
            c1 = 2 * c1;
            c0 -= target * target * (exp + 1);
            double disc = c1 * c1 - 4 * c2 * c0;
            if (disc >= 0)
            {
                result = (-c1 + Math.Sqrt(disc)) / (2 * c2);
            }
            else
            {
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

        private void ScaleXi(int exp, int ten, double newGamma)
        {
            var xi = _economy.Xi;
            for (int i = 0; i <= exp; i++)
            {
                for (int j = exp - i; j <= exp - i + ten; j++)
                {
                    if (!_isChanged[i][j])
                    {
                        _isChanged[i][j] = true;
                        _gamma[i][j] = newGamma;
                        for (int k = 0; k < _param.NumberOfFactors; k++)
                        {
                            xi[i][j, k] = xi[i][j, k] * newGamma;
                        }
                    }
                }
            }
        }
    }
}
