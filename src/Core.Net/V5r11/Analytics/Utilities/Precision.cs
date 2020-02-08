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

namespace Highlander.Reporting.Analytics.V5r3.Utilities
{

    public static class Precision
    {
        /// <summary>
        /// The number of binary digits used to represent the binary number for a double precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int DoubleWidth = 53;

        /// <summary>
        /// The number of binary digits used to represent the binary number for a single precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int SingleWidth = 24;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
        /// </summary>
        public static readonly double DoublePrecision = Math.Pow(2, -DoubleWidth);

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
        /// </summary>
        public static readonly double PositiveDoublePrecision = 2 * DoublePrecision;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 single-precision floating numbers (32 bit).
        /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
        /// </summary>
        public static readonly double SinglePrecision = Math.Pow(2, -SingleWidth);

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 single-precision floating numbers (32 bit).
        /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
        /// </summary>
        public static readonly double PositiveSinglePrecision = 2 * SinglePrecision;

        /// <summary>
        /// The number of significant decimal places of double-precision floating numbers (64 bit).
        /// </summary>
        public static readonly int DoubleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(DoublePrecision)));

        /// <summary>
        /// The number of significant decimal places of single-precision floating numbers (32 bit).
        /// </summary>
        public static readonly int SingleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(SinglePrecision)));

        /// <summary>
        /// Actual double precision machine epsilon, the smallest number that can be added to 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Higham.
        /// On a standard machine this is equivalent to `PositiveDoublePrecision`.
        /// </summary>

        public static readonly double PositiveMachineEpsilon = MeasurePositiveMachineEpsilon();

        /// <summary>
        /// Value representing 10 * 2^(-53) = 1.11022302462516E-15
        /// </summary>
        static readonly double DefaultDoubleAccuracy = DoublePrecision * 10;

        /// <summary>
        /// Value representing 10 * 2^(-24) = 5.96046447753906E-07
        /// </summary>
        static readonly float DefaultSingleAccuracy = (float) (SinglePrecision * 10);

        /// <summary>
        /// Compares two doubles and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this double a, double b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a, b, a - b, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this float a, float b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a, b, a - b, maximumAbsoluteError);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this double a, double b)
        {
            return AlmostEqualNorm(a, b, a - b, DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this float a, float b)
        {
            return AlmostEqualNorm(a, b, a - b, DefaultSingleAccuracy);
        }


        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum absolute error.
        /// </summary>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum absolute error, false otherwise.</returns>
        public static bool AlmostEqualNorm(this double a, double b, double diff, double maximumAbsoluteError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }
            return Math.Abs(diff) < maximumAbsoluteError;
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum error.
        /// </summary>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum error, false otherwise.</returns>
        public static bool AlmostEqualNormRelative(this double a, double b, double diff, double maximumError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }
            // If one is almost zero, fall back to absolute equality
            if (Math.Abs(a) < DoublePrecision || Math.Abs(b) < DoublePrecision)
            {
                return Math.Abs(diff) < maximumError;
            }
            if ((a == 0 && Math.Abs(b) < maximumError) || (b == 0 && Math.Abs(a) < maximumError))
            {
                return true;
            }
            return Math.Abs(diff) < maximumError * Math.Max(Math.Abs(a), Math.Abs(b));
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqualRelative(this double a, double b)
        {
            return AlmostEqualNormRelative(a, b, a - b, DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Calculates the actual positive double precision machine epsilon - the smallest number that can be added to 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Higham.
        /// </summary>
        /// <returns>Machine epsilon</returns>
        static double MeasurePositiveMachineEpsilon()
        {
            double eps = 1.0d;
            while (1.0d + eps / 2.0d > 1.0d)
                eps /= 2.0d;
            return eps;
        }
    }
}

        
