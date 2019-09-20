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

using System;

namespace Highlander.Numerics.Processes
{
    /// <summary>
    /// Ornstein-Uhlenbeck process class.
    /// </summary>
    /// <remarks>
    /// This class describes the Ornstein-Uhlenbeck process governed by 
    /// \f[
    /// 	dx = -a x_t dt + \sigma dW_t.
    /// \f]
    /// </remarks>
    public class OrnsteinUhlenbeckProcess : DiffusionProcess 
    {
        ///<summary>
        ///</summary>
        ///<param name="speed"></param>
        ///<param name="volatility"></param>
        public OrnsteinUhlenbeckProcess(double speed, double volatility)
            : this(speed, volatility, 0.0)
        {}

        ///<summary>
        ///</summary>
        ///<param name="speed"></param>
        ///<param name="volatility"></param>
        ///<param name="x0"></param>
        public OrnsteinUhlenbeckProcess(double speed, double volatility, 
                                        double x0) : base(x0)
        {
            _speed = speed;
            _volatility = volatility;
        }

        private readonly double _speed;
        private readonly double _volatility;

        public override double Drift(double time, double x)
        {
            return - _speed*x;
        }

        public override double Diffusion(double time, double x)
        {
            return _volatility;
        }

        public override double Expectation(double t0, double x0, double dt)
        {
            return x0*Math.Exp(-_speed*dt);
        }

        public override double Variance(double t0, double x0, double dt)
        {
            return 0.5 * _volatility*_volatility / _speed *
                   ( 1.0 - Math.Exp(-2.0*_speed*dt) );
        }
    }
}