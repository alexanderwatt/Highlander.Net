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
        #region declarations

        private readonly Parameters _param;
        private readonly Economy _economy;
        private double[][] _gamma;
        private bool[][] _ischanged;
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
            _gamma = new double[_param.Uexpiry][];
            _ischanged = new bool[_param.Uexpiry][];
            for (int i = 0; i < _param.Uexpiry; i++)
            {
                _gamma[i] = new double[_param.Utenor - i];
                _ischanged[i] = new bool[_param.Utenor - i];
            }
            for (int i = 0; i < _param.Uexpiry; i++)
            {
                for (int j = 0; j < _param.Utenor - i; j++)
                {
                    double target;
                    double bound;
                    if (j == 0)
                    {
                        target = _economy.ReturnIvol(i);
                        bound = CpltBound;
                    }
                    else
                    {
                        target = _economy.ReturnIvol(i , j);
                        bound = SwpnBound;
                    }
                    if (target > 0)
                    {
                        double tempgamma = FindGamma(i, j, bound, target);
                        ScaleXi(i, j, tempgamma);
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
            var tempvec1 = new GeneralVector(_param.NFAC);
            var tempvec2 = new GeneralVector(_param.NFAC);
            GeneralMatrix[] xi = _economy.Xi;
            double[][][] ai = _economy.Ai;
            for (int i = 0; i <= exp; i++)
            {
                tempvec1.SetToZero();
                tempvec2.SetToZero();
                for (int j = exp - i; j <= exp - i + ten; j++)
                {
                    if (_ischanged[i][j])
                    {
                        tempvec1 = (GeneralVector)tempvec1.Add(Vector.Multiply(ai[exp][ten][j - exp + i], xi[i][j, Range.All]));
                    }
                    else
                    {
                        tempvec2 = (GeneralVector)tempvec2.Add(Vector.Multiply(ai[exp][ten][j - exp + i], xi[i][j, Range.All]));
                    }
                }
                c0 += Vector.DotProduct(tempvec1, tempvec1);
                c1 += Vector.DotProduct(tempvec1, tempvec2);
                c2 += Vector.DotProduct(tempvec2, tempvec2);
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
                result = (-c1) / (2 * c2);
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

        private void ScaleXi(int exp, int ten, double newgamma)
        {
            GeneralMatrix[] xi = _economy.Xi;
            for (int i = 0; i <= exp; i++)
            {
                for (int j = exp - i; j <= exp - i + ten; j++)
                {
                    if (!_ischanged[i][j])
                    {
                        _ischanged[i][j] = true;
                        _gamma[i][j] = newgamma;
                        for (int k = 0; k < _param.NFAC; k++)
                        {
                            xi[i][j, k] = xi[i][j, k] * newgamma;
                        }
                    }
                }
            }
        }
    }
}
