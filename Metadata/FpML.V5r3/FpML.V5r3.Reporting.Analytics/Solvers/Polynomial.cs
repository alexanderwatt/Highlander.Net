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

#region Using Directives

using System;

#endregion

namespace Orion.Analytics.Solvers
{
    public class Polynomial
    {
        #region Constructor

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="poly">Creating a shallow copy of poly</param>
        public Polynomial(Polynomial poly)
        {
            try
            {
                ValidateDegree(poly._degree);
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }

            Degree  = poly._degree;

            try
            {
                ValidateNumOfCoefficients(poly._coeffs);
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }
            _coeffs = new decimal[poly._degree + 1];
            _coeffs = poly._coeffs;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="degree">degree of polynomial</param>
        /// PreCondition: degree > 0
        public Polynomial(int degree)
        {
            try
            {
                ValidateDegree(degree);
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }

            _degree = degree;

            _coeffs = new decimal[degree + 1];
            for (int i = 0; i <= degree; ++i)
            {
                _coeffs[i] = 0;
            }
        }

        /// <summary>
        /// constructor
        /// Precondition: degree > 0
        /// </summary>
        /// <param name="coeff">the value of all coefficients</param>
        /// <param name="degree">degree of polynomial</param>
        public Polynomial(decimal coeff, int degree)
        {
            try
            {
                ValidateDegree(degree);
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }
            _degree = degree;
            _coeffs = new decimal[degree + 1];
            for (int i = 0; i <= degree; ++i)
            {
                _coeffs[i] = coeff;
            }

        }

        /// <summary>
        /// constructor
        /// Preconditions:  degree > 0
        ///                 degree + 1 = number of coefficients provided
        /// </summary>
        /// <param name="coeffs">Array of coefficients</param>
        /// <param name="degree">Degree of coefficients</param>
        public Polynomial(decimal[] coeffs, int degree)
        {
            try
            {
                ValidateDegree(degree);  
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }
            _degree = degree;
            try
            {
                ValidateNumOfCoefficients(coeffs);
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
            }
            _coeffs = new decimal[degree + 1];
            _coeffs = coeffs;
        }

        /// <summary>
        /// Check to make sure if degree of polynomial is
        /// greater than zero
        /// </summary>
        /// <param name="degree">degree of polynomial</param>
        public void ValidateDegree(int degree)
        {          
            if (degree < 0)
            {
                const string errorMessage =
                    "A degree of polynomial can't be a negative number.";
                throw new Exception(errorMessage);
            }           
        }

        /// <summary>
        /// Check the number of coefficients is equal to the
        /// degree of polynomial + 1
        /// </summary>
        /// <param name="coeffs">array of coefficients</param>
        public void ValidateNumOfCoefficients(decimal[] coeffs)
        {
           
            if (coeffs.Length != Degree + 1)
            {
                const string errorMessage =
                    "The number of provided coefficients are not equal to the degree of polynomial.";

                throw new ArgumentException(errorMessage);
            }

        }

        #endregion

        #region Binary operator

        /// <summary>
        /// Adding two polynomial together
        /// </summary>
        /// <param name="firstPoly">first polynomial</param>
        /// <param name="secondPoly">second polynomial</param>
        /// <returns>The summation of two polynomial</returns>
        public static Polynomial operator +(Polynomial firstPoly, Polynomial secondPoly)
        {
            var largeDegree = firstPoly._degree > secondPoly._degree ? firstPoly._degree : secondPoly._degree;
            Polynomial result = new Polynomial(largeDegree);
            for (int i = 0; i < largeDegree; ++i)
            {
                if (i <= firstPoly._degree)
                {
                    result._coeffs[i] += firstPoly._coeffs[i];
                }
                if (i <= secondPoly._degree)
                {
                    result._coeffs[i] += secondPoly._coeffs[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Subtraction of two polynomial
        /// </summary>
        /// <param name="firstPoly">first polynomial</param>
        /// <param name="secondPoly">second polynomial</param>
        /// <returns></returns>
        public static Polynomial operator -(Polynomial firstPoly, Polynomial secondPoly)
        {
            var largeDegree = firstPoly._degree > secondPoly._degree ? firstPoly._degree : secondPoly._degree;
            Polynomial result = new Polynomial(largeDegree);
            for (int i = 0; i < largeDegree; ++i)
            {
                if (i <= firstPoly._degree)
                {
                    result._coeffs[i] += firstPoly._coeffs[i];
                }
                if (i <= secondPoly._degree)
                {
                    result._coeffs[i] += secondPoly._coeffs[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Muliplying two polynomial
        /// </summary>
        /// <param name="firstPoly">first polynomial</param>
        /// <param name="secondPoly">second polynomial</param>
        /// <returns></returns>
        public static Polynomial operator *(Polynomial firstPoly, Polynomial secondPoly)
        {
            var largeDegree = firstPoly.Degree + secondPoly.Degree;
            Decimal[] coeffs = new Decimal[largeDegree + 1];

            for (int i = 0; i <= firstPoly.Degree; ++i)
            {
                for (int j = 0; j <= secondPoly.Degree; ++j)
                {
                    coeffs[i + j] += firstPoly.Coeffs[i] * secondPoly.Coeffs[j];
                }
            }
            Polynomial result = new Polynomial(coeffs, largeDegree);          
            return result;
        }

        #endregion

        #region Accessing properties

        public int Degree
        {
            get => _degree;
            set => _degree = value;
        }

        public decimal[] Coeffs => _coeffs;

        /// <summary>
        /// return the value of coeff for a given degree.
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public decimal GetCoeff(int degree)
        {
            decimal coeff;
            if (degree > Degree || Degree < 0)
                coeff = 0.0m;
            else
                coeff = _coeffs[degree];
            return coeff;
        }

        #endregion

        #region Business logic

        public decimal Value(decimal x)
        {
            decimal sum = 0.0m;
            for (int i = 0; i <= _degree; ++i)
            {
                sum += _coeffs[i] * (decimal) Math.Pow((double)x, i);
            }
            return sum;
        }

        #endregion

        #region Private Fileds

        /// <summary>
        /// degree of polynomial
        /// </summary>
        private int _degree;

        /// <summary>
        /// array of coeffients
        /// </summary>
        private readonly decimal[] _coeffs;

        #endregion
    }
}