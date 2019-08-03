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
using Orion.Analytics.Stochastics.Volatilities;

#endregion

namespace Orion.Analytics.Stochastics.SABR
{
    /// <summary>
    /// Class that encapsulates the functionality to compute the SABR implied
    /// volatility, given all the SABR model parameters.
    /// No calibration of the SABR model parameters to the relevant market 
    /// data occurs in the class.
    /// </summary>
    public class SABRImpliedVolatility
    {
        #region Constructor
     
        /// <summary>
        /// Constructor for the class <see cref="SABRImpliedVolatility"/>.
        /// </summary>
        /// <param name="sabrParameters">Object that contains all the 
        /// SABR parameters.</param>
        /// <param name="checkSABRParameters">Flag used to signal whether
        /// the parameters for the SABR model require validation (true)
        /// or no validation required (false).</param>
        public SABRImpliedVolatility(SABRParameters sabrParameters,
                                     bool checkSABRParameters)
        {
            // Inspect whether a check of the SABR parameters has been requested.
            if(checkSABRParameters)
            {
                // Check of the SABR parameters requested.
                var errorMessage = "";
                var allSABRParametersValid =
                    SABRParameters.CheckSABRParameters(sabrParameters,
                                                       ref errorMessage);
                if(!allSABRParametersValid)
                {
                    // Invalid SABR parameter found.
                    throw new ArgumentException(errorMessage);
                }
            }           
            // SABR parameters are either all valid or a check of the these
            // parameters has not been requested: store all SABR parameters.
            _alpha = sabrParameters.Alpha;
            _beta = sabrParameters.Beta;
            _nu = sabrParameters.Nu;
            _rho = sabrParameters.Rho;
        }

        #endregion Constructor

        #region Public Business Logic Methods

        /// <summary>
        /// Compute the SABR implied volatility at a user defined asset price,
        /// exercise time and strike.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.
        /// Precondition: assetPrice &gt; 0.0.</param>
        /// <param name="exerciseTime">Time to option exercise.
        /// Precondition: exerciseTime &gt; 0.0.</param>
        /// <param name="strike">Option strike price.
        /// Precondition: strike &gt; 0.0.</param>
        /// <param name="errorMessage">Container for any possible error
        /// message.</param>
        /// <param name="result">Container to store the result(SABR implied
        /// volatility.</param>
        /// <param name="checkImpliedVolParameters">If set to <c>true</c> then
        ///  a check of the implied volatility parameters is performed.</param>
        /// <returns>
        /// True if SABR implied volatility computed, else
        /// false.
        /// </returns>
        public bool SABRInterpolatedVolatility(double assetPrice,
                                               double exerciseTime,
                                               double strike,
                                               ref string errorMessage,
                                               ref double result,
                                               bool checkImpliedVolParameters)
        {
            // Declare and initialise the return variable.
            var success = false;
            // Ensure that asset parameters are checked only if required.
            var areAssetParametersValid = true;
            if (checkImpliedVolParameters)
            {
                areAssetParametersValid =
                    ImpliedVolatilityParameters.CheckImpliedVolatilityParameters(
                        assetPrice, exerciseTime, strike, ref errorMessage);
            }
            if (areAssetParametersValid)
            {
                // Valid asset parameters: compute the SABR interpolated
                // volatility.
                var z = ComputeZ(assetPrice, strike);
                var x = ComputeX(z);
                result = ComputeSigma(assetPrice, exerciseTime, strike, x, z);
                // Update the return variable to indicate successful completion.
                success = true;
                errorMessage = "";
            }
            return success;
        }

        /// <summary>
        /// Compute the SABR implied volatility at a user defined asset price,
        /// exercise time and strike.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.
        /// Precondition: assetPrice &gt; 0.0.</param>
        /// <param name="exerciseTime">Time to option exercise.
        /// Precondition: exerciseTime &gt; 0.0.</param>
        /// <param name="strike">Option strike price.
        /// Precondition: strike &gt; 0.0.</param>
        /// <param name="errorMessage">Container for any possible error
        /// message.</param>
        /// <param name="result">Container to store the result(SABR implied
        /// volatility.</param>
        /// <param name="checkImpliedVolParameters">If set to <c>true</c> then
        ///  a check of the implied volatility parameters is performed.</param>
        /// <returns>
        /// True if SABR implied volatility computed, else
        /// false.
        /// </returns>
        public bool SABRInterpolatedVolatility(decimal assetPrice,
                                               decimal exerciseTime,
                                               decimal strike,
                                               ref string errorMessage,
                                               ref decimal result,
                                               bool checkImpliedVolParameters)
        {
            // Declare and initialise the return variable.
            var success = false;
            // Ensure that asset parameters are checked only if required.
            var areAssetParametersValid = true;
            if(checkImpliedVolParameters)
            {
                areAssetParametersValid = 
                    ImpliedVolatilityParameters.CheckImpliedVolatilityParameters(
                        assetPrice, exerciseTime, strike, ref errorMessage);
            }
            if(areAssetParametersValid)
            {
                // Valid asset parameters: compute the SABR interpolated
                // volatility.
                var z = ComputeZ(assetPrice, strike);
                var x = ComputeX(z);
                result = ComputeSigma(assetPrice, exerciseTime, strike, x, z);
                // Update the return variable to indicate successful completion.
                success = true;
                errorMessage = "";
            }
            return success;
        }

        #endregion Public Business Logic Methods

        #region Private Helper Business Logic Methods

        /// <summary>
        /// Computes the variable z in the SABR model.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset, for example 
        /// forward swap rate.</param>
        /// <param name="strike">Option strike price.</param>
        /// <returns>SABR variable z.</returns>
        private decimal ComputeZ(decimal assetPrice, decimal strike)
        {
            // Declare and initialise all the required variables.
            var oneMinusBeta = 1.0m - _beta;
            var lambda = _nu / _alpha *
                             (decimal)Math.Pow(decimal.ToDouble(assetPrice * strike),
                                                decimal.ToDouble(oneMinusBeta/2.0m));
            var moneyness = (double)(assetPrice/strike);
            // Compute and return the SABR variable z.
            var z = lambda * (decimal)Math.Log(moneyness);
            return z;
        }

        /// <summary>
        /// Computes the variable z in the SABR model.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset, for example 
        /// forward swap rate.</param>
        /// <param name="strike">Option strike price.</param>
        /// <returns>SABR variable z.</returns>
        private double ComputeZ(double assetPrice, double strike)
        {
            // Declare and initialise all the required variables.
            var oneMinusBeta = 1.0m - _beta;
            var lambda = _nu / _alpha *
                             (decimal)Math.Pow(assetPrice * strike,
                                                decimal.ToDouble(oneMinusBeta) / 2.0d);
            var moneyness = assetPrice / strike;
            // Compute and return the SABR variable z.
            var z = lambda * (decimal)Math.Log(moneyness);
            return (double)z;
        }

        /// <summary>
        /// Computes the variable x in the SABR model.
        /// </summary>
        /// <param name="z">SABR variable z, as computed by the method
        /// ComputeZ.</param>
        /// <returns>SABR variable x.</returns>
        private decimal ComputeX(decimal z)
        {
            return (decimal)ComputeX((double)z);
        }

        /// <summary>
        /// Computes the variable x in the SABR model.
        /// </summary>
        /// <param name="z">SABR variable z, as computed by the method
        /// ComputeZ.</param>
        /// <returns>SABR variable x.</returns>
        private double ComputeX(double z)
        {
            // Compute the numerator and denominator of the fraction
            // that defines the SABR variable x.
            double numerator = z - (double)_rho + Math.Sqrt(1d - 2d * (double)_rho * z + z * z);
            double denominator = 1d - (double)_rho;
            // Compute and return the SABR variable x.
            double x = Math.Log(numerator / denominator);
            return x;
        }

        /// <summary>
        /// Helper function that computes the implied volatility from 
        /// the SABR model.</summary>
        /// <param name="assetPrice">Price of the relevant asset, for example 
        /// forward swap rate.</param>
        /// <param name="exerciseTime">Time to option exercise.</param>
        /// <param name="strike">Option strike price.</param>
        /// <param name="x">SABR variable x, as computed by the method
        /// ComputeX.</param>
        /// <param name="z">SABR variable z, as computed by the method
        /// ComputeZ.</param>
        /// <returns>Implied volatility as computed by the SABR model.</returns>
        private double ComputeSigma(double assetPrice,
                                     double exerciseTime,
                                     double strike,
                                     double x,
                                     double z)
        {
            // Compute the value of the multiplier (z/x): include
            // logic to avoid division by zero.
            var multiplier =
                double.Epsilon < Math.Abs(x) ? z / x : 1.0d;
            // Compute expressions that occur recursively in the formula
            // for the implied volatility.
            var oneMinusBeta = 1.0d - (double)_beta;
            var mu = Math.Log(assetPrice / strike); // log moneyness
            var lambda = Math.Pow(assetPrice * strike, oneMinusBeta / 2.0d);
            // Compute the leading order term in the SABR model formula
            // for the implied volatility.
            var a = decimal.ToDouble(_alpha);
            var b = 1.0d + (1 / 24.0d) * oneMinusBeta * oneMinusBeta * mu * mu +
                        (1 / 1920.0d) * Math.Pow(oneMinusBeta, 4) *
                        Math.Pow(mu, 4);
            b *= lambda;
            var leadingTerm = a / b * multiplier;
            // Compute the second order term in the SABR model formula
            // for the implied volatility.
            var c =
                oneMinusBeta * oneMinusBeta * decimal.ToDouble(_alpha * _alpha) / (24.0d * lambda * lambda);
            var d = decimal.ToDouble(_rho * _beta * _nu * _alpha) / (4.0d * lambda);
            var e = decimal.ToDouble(_nu * _nu) * (2.0d - 3.0d * decimal.ToDouble(_rho * _rho)) / 24.0d;
            var secondOrderTerm = (c + d + e) * exerciseTime;
            // Compute and return the SABR implied volatility.
            var sigma = leadingTerm * (1.0d + secondOrderTerm);
            return sigma;
        }

        /// <summary>
        /// Helper function that computes the implied volatility from 
        /// the SABR model.</summary>
        /// <param name="assetPrice">Price of the relevant asset, for example 
        /// forward swap rate.</param>
        /// <param name="exerciseTime">Time to option exercise.</param>
        /// <param name="strike">Option strike price.</param>
        /// <param name="x">SABR variable x, as computed by the method
        /// ComputeX.</param>
        /// <param name="z">SABR variable z, as computed by the method
        /// ComputeZ.</param>
        /// <returns>Implied volatility as computed by the SABR model.</returns>
        private decimal ComputeSigma(decimal assetPrice,
                                     decimal exerciseTime,
                                     decimal strike,
                                     decimal x,
                                     decimal z)
        {
            // Compute the value of the multiplier (z/x): include
            // logic to avoid division by zero.
            var multiplier =
                double.Epsilon < Math.Abs((double)x) ? z/x : 1.0m;
            // Compute expressions that occur recursively in the formula
            // for the implied volatility.
            var oneMinusBeta = 1.0m - _beta;
            var mu = (decimal)Math.Log(decimal.ToDouble(assetPrice / strike)); // log moneyness
            var lambda = (decimal)Math.Pow(decimal.ToDouble(assetPrice*strike),
                                      decimal.ToDouble(oneMinusBeta/2.0m));
            // Compute the leading order term in the SABR model formula
            // for the implied volatility.
            var a = _alpha;
            var b = 1.0m + 1/24.0m*oneMinusBeta*oneMinusBeta*mu*mu +
                        1 / 1920.0m * (decimal)Math.Pow(decimal.ToDouble(oneMinusBeta), 4) *
                        (decimal)Math.Pow(decimal.ToDouble(mu), 4);
            b *= lambda;
            var leadingTerm = a/b*multiplier;
            // Compute the second order term in the SABR model formula
            // for the implied volatility.
            var c =
                oneMinusBeta*oneMinusBeta*_alpha*_alpha/(24.0m*lambda*lambda);
            var d = _rho * _beta * _nu * _alpha / (4.0m * lambda);
            var e = _nu * _nu * (2.0m - 3.0m * _rho * _rho) / 24.0m;
            var secondOrderTerm = (c + d + e) * exerciseTime;
            // Compute and return the SABR implied volatility.
            var sigma = leadingTerm * (1.0m + secondOrderTerm);
            return sigma;
        }

        #endregion Private Helper Business Logic Methods

        #region Private Fields

        /// <summary>
        /// Parameter alpha in the SABR model.
        /// </summary>
        private readonly decimal _alpha;

        /// <summary>
        /// Parameter beta in the SABR model.
        /// </summary>
        private readonly decimal _beta;

        /// <summary>
        /// Parameter nu in the SABR model.
        /// </summary>
        private readonly decimal _nu;

        /// <summary>
        /// Parameter rho in the SABR model.
        /// </summary>
        private readonly decimal _rho;

        #endregion Private Fields
    }
}