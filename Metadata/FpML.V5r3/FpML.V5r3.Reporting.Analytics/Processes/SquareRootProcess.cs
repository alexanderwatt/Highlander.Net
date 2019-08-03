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

using System;

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Square-root process class.
    /// </summary>
    /// <remarks>
    /// This class describes a square-root process governed by 
    /// \f[
    /// 	dx = a (b - x_t) dt + \sigma \sqrt{x_t} dW_t.
    /// \f]
    /// </remarks>
    public class SquareRootProcess : DiffusionProcess 
    {
        ///<summary>
        ///</summary>
        ///<param name="b"></param>
        ///<param name="a"></param>
        ///<param name="sigma"></param>
        public SquareRootProcess(double b, double a, double sigma)
            : this(b, a, sigma, 0.0)
        {}

        ///<summary>
        ///</summary>
        ///<param name="b"></param>
        ///<param name="a"></param>
        ///<param name="sigma"></param>
        ///<param name="x0"></param>
        public SquareRootProcess(double b, double a, double sigma, 
                                 double x0) : base(x0)
        {
            _mean = b;
            _speed = a;
            _volatility = sigma;
        }

        private readonly double _mean;
        private readonly double _speed;
        private readonly double _volatility;

        public override double Drift(double time, double x)
        {
            return _speed * (_mean - x);
        }
        public override double Diffusion(double time, double x)
        {
            return _volatility * Math.Sqrt(x);
        }
    }
}