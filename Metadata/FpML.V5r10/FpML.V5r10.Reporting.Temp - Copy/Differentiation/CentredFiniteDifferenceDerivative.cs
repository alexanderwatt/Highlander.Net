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

//using System;
//using System.Linq;
//using Orion.Analytics.LinearAlgebra;
//using Orion.Analytics.Maths;
//using Orion.Analytics.Utilities;

namespace Orion.Analytics.Differentiation
{
    ///// <summary>
    ///// Type of finite different step size.
    ///// </summary>
    //public enum StepType
    //{
    //    /// <summary>
    //    /// The absolute step size value will be used in numerical derivatives, regardless of order or function parameters.
    //    /// </summary>
    //    Absolute,

    //    /// <summary>
    //    /// A base step size value, h, will be scaled according to the function input parameter. A common example is hx = h*(1+abs(x)), however
    //    /// this may vary depending on implementation. This definition only guarantees that the only scaling will be relative to the
    //    /// function input parameter and not the order of the finite difference derivative.
    //    /// </summary>
    //    RelativeX,

    //    /// <summary>
    //    /// A base step size value, eps (typically machine precision), is scaled according to the finite difference coefficient order
    //    /// and function input parameter. The initial scaling according to finite different coefficient order can be thought of as producing a
    //    /// base step size, h, that is equivalent to <see cref="RelativeX"/> scaling. This stepsize is then scaled according to the function
    //    /// input parameter. Although implementation may vary, an example of second order accurate scaling may be (eps)^(1/3)*(1+abs(x)).
    //    /// </summary>
    //    Relative
    //}

//    /// <summary>
//    /// Class to calculate finite difference coefficients using Taylor series expansion method.
//    /// <remarks>
//    /// <para>
//    /// For n points, coefficients are calculated up to the maximum derivative order possible (n-1).
//    /// The current function value position specifies the "center" for surrounding coefficients.
//    /// Selecting the first, middle or last positions represent forward, backwards and central difference methods.
//    /// </para>
//    /// </remarks>
//    /// </summary>
//    public class FiniteDifferenceCoefficients
//    {
//        /// <summary>
//        /// Number of points for finite difference coefficients. Changing this value recalculates the coefficients table.
//        /// </summary>
//        public int Points
//        {
//            get => _points;
//            set
//            {
//                CalculateCoefficients(value);
//                _points = value;
//            }
//        }

//        private double[][,] _coefficients;

//        private int _points;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="FiniteDifferenceCoefficients"/> class.
//        /// </summary>
//        /// <param name="points">Number of finite difference coefficients.</param>

//        public FiniteDifferenceCoefficients(int points)
//        {
//            Points = points;
//            CalculateCoefficients(Points);
//        }

//        /// <summary>
//        /// Gets the finite difference coefficients for a specified center and order.
//        /// </summary>
//        /// <param name="center">Current function position with respect to coefficients. Must be within point range.</param>
//        /// <param name="order">Order of finite difference coefficients.</param>
//        /// <returns>Vector of finite difference coefficients.</returns>
//        public double[] GetCoefficients(int center, int order)
//        {
//            if (center >= _coefficients.Length)
//                throw new ArgumentOutOfRangeException(nameof(center), "Center position must be within the point range.");
//            if (order >= _coefficients.Length)
//                throw new ArgumentOutOfRangeException(nameof(order), "Maximum difference order is points-1.");
//            // Return proper row
//            var columns = _coefficients[center].GetLength(1);
//            var array = new double[columns];
//            for (int i = 0; i < columns; ++i)
//                array[i] = _coefficients[center][order, i];
//            return array;
//        }

//        private void CalculateCoefficients(int points)
//        {
//            var c = new double[points][,];
//            // For ever possible center given the number of points, compute ever possible coefficient for all possible orders.
//            for (int center = 0; center < points; center++)
//            {
//                // Deltas matrix for center located at 'center'.
//                var a = new Matrix(points, points);
//                var l = points - center - 1;
//                for (int row = points - 1; row >= 0; row--)
//                {
//                    a[row, 0] = 1.0;
//                    for (int col = 1; col < points; col++)
//                    {
//                        a[row, col] = a[row, col - 1] * l / col;
//                    }
//                    l -= 1;
//                }
//                a.Inverse();
//                c[center] = a.Data;
//                // "Polish" results by rounding.
//                var fac = Fn.Factorial(points);
//                for (int j = 0; j < points; j++)
//                for (int k = 0; k < points; k++)
//#if PORTABLE
//                c[center][j, k] = (Math.Round(c[center][j, k] * fac)) / fac;
//#else
//                c[center][j, k] = Math.Round(c[center][j, k] * fac, MidpointRounding.AwayFromZero) / fac;
//#endif
//            }
//            _coefficients = c;
//        }
//    }

    /// <summary>
    /// Concrete implementation of the interface IFiniteDifferenceDerivative.
    /// Class encapsulates the functionality to compute numerical derivatives
    /// by a Centred Finite Difference scheme.
    /// </summary>
    public class CentredFiniteDifferenceDerivative : IFiniteDifferenceDerivative
    {
        //#region Properties

        //private readonly int _points;
        //int _center;
        //double _stepSize = Math.Pow(2, -10);
        //double _epsilon = Precision.PositiveMachineEpsilon;
        //double _baseStepSize = Math.Pow(2, -26);
        //private readonly FiniteDifferenceCoefficients _coefficients;

        ///// <summary>
        ///// Type of step size for computing finite differences. If set to absolute, dx = h.
        ///// If set to relative, dx = (1+abs(x))*h^(2/(order+1)). This provides accurate results when
        ///// h is approximately equal to the square-root of machine accuracy, epsilon.
        ///// </summary>
        //public StepType StepType { get; set; } = StepType.Relative;

        ///// <summary>
        ///// Sets and gets the location of the center point for the finite difference derivative.
        ///// </summary>
        //public int Center
        //{
        //    get => _center;
        //    set
        //    {
        //        if (value >= _points || value < 0)
        //            throw new ArgumentOutOfRangeException(nameof(value), "Center must lie between 0 and points -1");
        //        _center = value;
        //    }
        //}

        ///// <summary>
        ///// Sets and gets the finite difference step size. This value is for each function evaluation if relative stepsize types are used.
        ///// If the base step size used in scaling is desired, see <see cref="Epsilon"/>.
        ///// </summary>
        ///// <remarks>
        ///// Setting then getting the StepSize may return a different value. This is not unusual since a user-defined step size is converted to a
        ///// base-2 representable number to improve finite difference accuracy.
        ///// </remarks>
        //public double StepSize
        //{
        //    get => _stepSize;
        //    set
        //    {
        //        //Base 2 yields more accurate results...
        //        var p = Math.Log(Math.Abs(value)) / Math.Log(2);
        //        _stepSize = Math.Pow(2, Math.Round(p));
        //    }
        //}

        ///// <summary>
        ///// Sets and gets the base fininte difference step size. This assigned value to this parameter is only used if <see cref="StepType"/> is set to RelativeX.
        ///// However, if the StepType is Relative, it will contain the base step size computed from <see cref="Epsilon"/> based on the finite difference order.
        ///// </summary>
        //public double BaseStepSize
        //{
        //    get => _baseStepSize;
        //    set
        //    {
        //        //Base 2 yields more accurate results...
        //        var p = Math.Log(Math.Abs(value)) / Math.Log(2);
        //        _baseStepSize = Math.Pow(2, Math.Round(p));
        //    }
        //}

        ///// <summary>
        ///// Sets and gets the base finite difference step size. This parameter is only used if <see cref="StepType"/> is set to Relative.
        ///// By default this is set to machine epsilon, from which <see cref="BaseStepSize"/> is computed.
        ///// </summary>
        //public double Epsilon
        //{
        //    get => _epsilon;
        //    set
        //    {
        //        //Base 2 yields more accurate results...
        //        var p = Math.Log(Math.Abs(value)) / Math.Log(2);
        //        _epsilon = Math.Pow(2, Math.Round(p));
        //    }
        //}

        ///// <summary>
        ///// Number of times a function is evaluated for numerical derivatives.
        ///// </summary>
        //public int Evaluations { get; private set; }

        //#endregion

        #region Constructor

        /// <summary>
        /// Constructor for the <see cref="CentredFiniteDifferenceDerivative"/>
        /// class. Internally, the constructor maps each array into its
        /// appropriate private field.
        /// No data validation is performed by the class because it is assumed
        /// that the client has performed all necessary data validation.
        /// </summary>
        /// <param name="xArray">One dimensional array of x values.</param>
        /// <param name="yArray">One dimensional array of y values</param>
        /// Precondition: length of the x and y arrays are identical, and is
        /// at least two.
        /// Precondition: x array in strict ascending order.
        /// Precondition: x and y arrays form ordered (x,y) pairs.
        public CentredFiniteDifferenceDerivative(decimal[] xArray,
                                                 decimal[] yArray)
        {
            // Map arguments to their private fields.
            _xArray = new decimal[xArray.Length];
            xArray.CopyTo(_xArray, 0);
            _yArray = new decimal[yArray.Length];
            yArray.CopyTo(_yArray, 0);
        }

        /// <summary>
        /// Initializes a NumericalDerivative class with the default 3 point center difference method.
        /// </summary>
        public CentredFiniteDifferenceDerivative() //: this(3, 1)
        {}

        ///// <summary>
        ///// Initialized a NumericalDerivative class.
        ///// </summary>
        ///// <param name="points">Number of points for finite difference derivatives.</param>
        ///// <param name="center">Location of the center with respect to other points. Value ranges from zero to points-1.</param>
        //public CentredFiniteDifferenceDerivative(int points, int center)
        //{
        //    if (points < 2)
        //    {
        //        throw new ArgumentOutOfRangeException(nameof(points), "Points must be two or greater.");
        //    }
        //    _center = center;
        //    _points = points;
        //    Center = center;
        //    _coefficients = new FiniteDifferenceCoefficients(points);
        //}

        #endregion Constructor

        #region Public Business Logic Methods

        ///// <summary>
        ///// Evaluates the derivative of a scalar univariate function.
        ///// </summary>
        ///// <remarks>
        ///// Supplying the optional argument currentValue will reduce the number of function evaluations
        ///// required to calculate the finite difference derivative.
        ///// </remarks>
        ///// <param name="f">Function handle.</param>
        ///// <param name="x">Point at which to compute the derivative.</param>
        ///// <param name="order">Derivative order.</param>
        ///// <param name="currentValue">Current function value at center.</param>
        ///// <returns>Function derivative at x of the specified order.</returns>
        //public double EvaluateDerivative(Func<double, double> f, double x, int order, double? currentValue = null)
        //{
        //    var c = _coefficients.GetCoefficients(Center, order);
        //    var h = CalculateStepSize(_points, x, order);
        //    var points = new double[_points];
        //    for (int i = 0; i < _points; i++)
        //    {
        //        if (i == Center && currentValue.HasValue)
        //            points[i] = currentValue.Value;
        //        else if (Math.Abs(c[i]) > 0) // Only evaluate function if it will actually be used.
        //        {
        //            points[i] = f(x + (i - Center) * h);
        //            Evaluations++;
        //        }
        //    }
        //    return EvaluateDerivative(points, order, h);
        //}

        ///// <summary>
        ///// Evaluates the derivative of equidistant points using the finite difference method.
        ///// </summary>
        ///// <param name="points">Vector of points StepSize apart.</param>
        ///// <param name="order">Derivative order.</param>
        ///// <param name="stepSize">Finite difference step size.</param>
        ///// <returns>Derivative of points of the specified order.</returns>
        //public double EvaluateDerivative(double[] points, int order, double stepSize)
        //{
        //    if (points == null)
        //        throw new ArgumentNullException(nameof(points));
        //    if (order >= _points || order < 0)
        //        throw new ArgumentOutOfRangeException(nameof(order), "Order must be between zero and points-1.");
        //    var c = _coefficients.GetCoefficients(Center, order);
        //    var result = c.Select((t, i) => t * points[i]).Sum();
        //    result /= Math.Pow(stepSize, order);
        //    return result;
        //}

        ///// <summary>
        ///// Creates a function handle for the derivative of a scalar univariate function.
        ///// </summary>
        ///// <param name="f">Input function handle.</param>
        ///// <param name="order">Derivative order.</param>
        ///// <returns>Function handle that evaluates the derivative of input function at a fixed order.</returns>
        //public Func<double, double> CreateDerivativeFunctionHandle(Func<double, double> f, int order)
        //{
        //    return x => EvaluateDerivative(f, x, order);
        //}

        /// <summary>
        /// Encapsulates the methodology to compute numerical first derivatives
        /// by a Centred Finite Difference scheme. If the derivative requested
        /// coincides with one of the ends of the grid of knot points, then a
        /// one sided Finite Difference is used to compute the derivative.
        /// </summary>
        /// <param name="index">Zero based index in the array of x and y
        /// values at which the derivative is required.</param>
        /// <returns>Numerical first derivative.</returns>
        public decimal ComputeFirstDerivative(int index)
        {
            decimal deltaX;
            decimal deltaY;
            decimal firstDerivative; // return variable
            // Compute the first derivative based on the index location.
            if(index == 0)
            {
                // Derivative at extreme LEFT.
                deltaX = _xArray[1] - _xArray[0];
                deltaY = _yArray[1] - _yArray[0];
                firstDerivative = deltaY/deltaX;
            }
            else if(index == _xArray.Length - 1)
            {
                // Derivative at extreme RIGHT.
                var maxIndex = _xArray.Length - 1;
                deltaX = _xArray[maxIndex] - _xArray[maxIndex - 1];
                deltaY = _yArray[maxIndex] - _yArray[maxIndex - 1];
                firstDerivative = deltaY/deltaX;
            }
            else
            {
                // Derivative at an interior point.
                deltaX = _xArray[index + 1] - _xArray[index];
                deltaY = _yArray[index + 1] - _yArray[index];
                var forwardDerivative = deltaY/deltaX;
                deltaX = _xArray[index] - _xArray[index - 1];
                deltaY = _yArray[index] - _yArray[index - 1];
                var backwardDerivative = deltaY/deltaX;
                firstDerivative =
                    (forwardDerivative + backwardDerivative)/2m;
            }
            return firstDerivative;
        }

        #endregion Public Business Logic Methods

        #region Private Fields

        //private double CalculateStepSize(int points, double x, double order)
        //{
        //    // Step size relative to function input parameter
        //    if (StepType == StepType.RelativeX)
        //    {
        //        StepSize = BaseStepSize * (1 + Math.Abs(x));
        //    }
        //    // Step size relative to function input parameter and order
        //    else if (StepType == StepType.Relative)
        //    {
        //        var accuracy = points - order;
        //        BaseStepSize = Math.Pow(Epsilon, (1 / (accuracy + order)));
        //        StepSize = BaseStepSize * (1 + Math.Abs(x));
        //    }
        //    // Do nothing for absolute step size.
        //    return StepSize;
        //}

        /// <summary>
        /// Array of x values.
        /// Convention is that values used in the computation of derivatives
        /// are arranged into the from (x, y).
        /// </summary>
        private readonly decimal[] _xArray;

        /// <summary>
        /// Array of y values.
        /// Convention is that values used in the computation of derivatives
        /// are arranged into the from (x, y).
        /// </summary>
        private readonly decimal[] _yArray;

        #endregion Private Fields
    }
}