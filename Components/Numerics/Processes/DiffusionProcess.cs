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

namespace Highlander.Numerics.Processes
{
    /// <summary>
    /// Diffusion process class.
    /// </summary>
    /// <remarks>
    /// This class describes a stochastic process governed by 
    /// \f[
    ///		dx_t = \mu(t, x_t)dt + \sigma(t, x_t)dW_t.
    ///	\f]
    /// </remarks>
    public abstract class DiffusionProcess : IDiffusionProcess
    {
        protected DiffusionProcess(double x0)
        {
            X0 = x0;
        }

        public double X0 { get; }

        /// <summary>
        /// The drift part of the equation. 
        /// </summary>
        /// <remarks>
        /// i.e. \f$ \mu(t, x_t) \f$
        /// </remarks>
        /// <param name="time"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract double Drift(double time, double x);

        /// <summary>
        /// The diffusion part of the equation.
        /// </summary>
        /// <remarks>
        /// i.e. \f$\sigma(t,x_t)\f$
        /// </remarks>
        /// <param name="time"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract double Diffusion(double time, double x);

        /// <summary>
        /// The expectation of the process after a time interval.
        /// </summary>
        /// <remarks>
        ///  By default, it returns the Euler approximation defined by
        ///  \f$ x_0 + \mu(t_0, x_0) \Delta t \f$.
        /// </remarks>
        /// <param name="t0"></param>
        /// <param name="x0"></param>
        /// <param name="dt"></param>
        /// <returns>returns \f$ E(x_{t_0 + \Delta t} | x_{t_0} = x_0) \f$.</returns>
        public virtual double Expectation(double t0, double x0, double dt)
        {
            return x0 + Drift(t0, x0)*dt;
        }

        /// <summary>
        /// The variance of the process after a time interval.
        /// </summary>
        /// <remarks>
        /// By default, it returns the Euler approximation defined by
        /// \f$ \sigma(t_0, x_0)^2 \Delta t \f$.
        /// </remarks>
        /// <param name="t0"></param>
        /// <param name="x0"></param>
        /// <param name="dt"></param>
        /// <returns>
        /// returns \f$ Var(x_{t_0 + \Delta t} | x_{t_0} = x_0) \f$.
        /// </returns>
        public virtual double Variance(double t0, double x0, double dt)
        {
            double sigma = Diffusion(t0, x0);
            return sigma*sigma*dt;
        }
    }
}