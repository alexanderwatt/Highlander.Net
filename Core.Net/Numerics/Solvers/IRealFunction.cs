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
using Highlander.Numerics.LinearAlgebra.Sparse;
using Highlander.Numerics.Maths.Collections;

namespace Highlander.Numerics.Solvers
{
    /// <summary>
    /// Interface to objective function for 1-D solvers.
    /// </summary>
    /// <remarks>
    /// This is the function whose zeroes must be found.
    /// </remarks>
    public interface IObjectiveFunction
    {
        /// <summary>
        /// A string representation of the objective function.
        /// </summary>
        /// <returns>A String representing the object.</returns>
        string ToString();

        /// <summary>
        /// Definition of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        double Value(double x);

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        double Derivative(double x);
    }

    /// <summary>
    /// Unary function.
    /// </summary>
    /// <remarks>
    /// Classes implementing this interface must provide a method
    /// <see cref="Value"/> acting as a unary function with a single
    /// argument and a result both of type <see cref="double"/>.
    /// </remarks>
    /// <seealso cref="UnaryFunction">UnaryFunction delegate</seealso>
    /// <seealso cref="Functional"/>
    public interface IUnaryFunction
    {
        /// <summary>
        /// A unary function.
        /// </summary>
        /// <param name="x">First argument.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        double Value(double x);
    }

    /// <summary>
    /// Binary function.
    /// </summary>
    /// <remarks>
    /// Classes implementing this interface must provide a method
    /// <see cref="Value"/> acting as a binary function with two 
    /// arguments and a result all of type <see cref="double"/>.
    /// </remarks>
    /// <seealso cref="BinaryFunction">BinaryFunction delegate</seealso>
    public interface IBinaryFunction
    {
        /// <summary>
        /// A binary function.
        /// </summary>
        /// <param name="x">First argument.</param>
        /// <param name="y">Second argument.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        double Value(double x, double y);
    }

    /// <summary>
    /// A real function.
    /// </summary>
    /// <param name="objectiveFunctionValue"></param>
    /// <returns></returns>
    public delegate Double RealFunction(double objectiveFunctionValue);

    /// <summary>
    /// Binary function delegate.
    /// </summary>
    /// <remarks>
    /// Delegates of this type act as a binary function with two 
    /// arguments and a result all of type <see cref="double"/>.
    /// </remarks>
    /// <param name="x">First argument.</param>
    /// <param name="y">Second argument.</param>
    /// <returns>
    /// The function value.
    /// </returns>
    /// <seealso cref="IBinaryFunction"/>
    public delegate double BinaryFunction(double x, double y);

    ///<summary>
    ///</summary>
    ///<param name="x"></param>
    public delegate double MultivariateRealFunction(double[] x);

    ///<summary>
    ///</summary>
    ///<param name="x"></param>
    public delegate double QRMultivariateRealFunction(DoubleVector x);

    ///<summary>
    /// A payoff function delegate.
    ///</summary>
    ///<param name="parameters"></param>
    public delegate double PayOffFunction(DenseVector parameters);   

        /// <summary>
    /// Unary function delegate.
    /// </summary>
    /// <remarks>
    /// Delegates of this type act as a unary function with a single
    /// argument and a result both of type <see cref="double"/>.
    /// </remarks>
    /// <param name="x">First argument.</param>
    /// <returns>
    /// The function value.
    /// </returns>
    /// <seealso cref="IUnaryFunction"/>
    /// <seealso cref="Functional"/>
    /// <example>
    /// [C#]
    /// <code>
    /// UnaryFunction sin = new UnaryFunction(Math.Sin);
    /// </code>
    /// </example>
    public delegate double UnaryFunction(double x);

    /// <summary>
    /// Objective function for 1-D solvers. 
    /// </summary>
    /// <remarks>
    /// This is the function whose zeroes must be found.
    /// </remarks>
    public abstract class ObjectiveFunction : IObjectiveFunction
    {

        /// <summary>
        /// Definition of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        public abstract double Value(double x);

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        public virtual double Derivative(double x)
        {
            throw new NotImplementedException("ObjFnNotImpl");
            // was return double.MaxValue; // which was return Null<double>
        }
    }
}