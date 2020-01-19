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

namespace Highlander.Numerics.Stochastics.Volatilities
{
    /// <summary>
    /// Class that encapsulates the asset parameters used to compute the 
    /// implied volatility from the SABR model.
    /// </summary>
    public class ImpliedVolatilityParameters
    {
        #region Individual Parameter Validation Methods

        /// <summary>
        /// Checks the validity of the asset price.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.
        /// Precondition: assetPrice > 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the asset price is valid, otherwise
        /// false.</returns>
        public static bool CheckAssetPrice(decimal assetPrice,
                                           ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the asset price.
            if(assetPrice <= 0.0m)
            {
                isParameterValid = false;
                errorMessage = "Asset price must be positive.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the asset price.
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.
        /// Precondition: assetPrice > 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the asset price is valid, otherwise
        /// false.</returns>
        public static bool CheckAssetPrice(double assetPrice,
                                           ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the asset price.
            if (assetPrice <= 0.0d)
            {
                isParameterValid = false;
                errorMessage = "Asset price must be positive.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the exercise time.
        /// </summary>
        /// <param name="exerciseTime">Time to option exercise.
        /// Precondition: exerciseTime >= 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the exercise time is valid, otherwise
        /// false.</returns>
        public static bool CheckExerciseTime(decimal exerciseTime,
                                             ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the exercise time.
            if(exerciseTime < 0.0m)
            {
                isParameterValid = false;
                errorMessage = "Exercise time cannot be negative.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the exercise time.
        /// </summary>
        /// <param name="exerciseTime">Time to option exercise.
        /// Precondition: exerciseTime >= 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the exercise time is valid, otherwise
        /// false.</returns>
        public static bool CheckExerciseTime(double exerciseTime,
                                             ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the exercise time.
            if (exerciseTime < 0.0d)
            {
                isParameterValid = false;
                errorMessage = "Exercise time cannot be negative.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the strike.
        /// </summary>
        /// <param name="strike">Option strike price.
        /// Precondition: strike > 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the strike is valid, otherwise
        /// false.</returns>
        public static bool CheckStrike(decimal strike,
                                       ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the strike.
            if(strike <= 0.0m)
            {
                isParameterValid = false;
                errorMessage = "Strike must be positive.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the strike.
        /// </summary>
        /// <param name="strike">Option strike price.
        /// Precondition: strike > 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if the strike is valid, otherwise
        /// false.</returns>
        public static bool CheckStrike(double strike,
                                       ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the strike.
            if (strike <= 0.0d)
            {
                isParameterValid = false;
                errorMessage = "Strike must be positive.";
            }
            return isParameterValid;
        }
        #endregion Individual Parameter Validation Methods

        #region Master Parameter Validation Method

        /// <summary>
        /// Master (wrapper) function that checks the validity of the
        /// implied volatility parameters.
        /// Valid bounds/ranges for the implied volatility parameters are
        /// defined in the functions the check the individual
        /// implied volatility parameters. 
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.</param>
        /// <param name="exerciseTime">Time to option exercise.</param>
        /// <param name="strike">Option strike price.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if all implied volatility parameters are valid, 
        /// otherwise false.</returns>
        public static bool
            CheckImpliedVolatilityParameters(decimal assetPrice,
                                             decimal exerciseTime,
                                             decimal strike,
                                             ref string errorMessage)
        {
            // Initialise the error message container.
            errorMessage = "";
            // Check asset price.
            var areAllParametersValid =
                CheckAssetPrice(assetPrice, ref errorMessage);
            if(!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check exercise time.
            areAllParametersValid =
                CheckExerciseTime(exerciseTime, ref errorMessage);
            if (!areAllParametersValid)
            {
                return areAllParametersValid;
            }

            // Check strike.
            areAllParametersValid =
                CheckStrike(strike, ref errorMessage);
            return areAllParametersValid;
        }

        /// <summary>
        /// Master (wrapper) function that checks the validity of the
        /// implied volatility parameters.
        /// Valid bounds/ranges for the implied volatility parameters are
        /// defined in the functions the check the individual
        /// implied volatility parameters. 
        /// </summary>
        /// <param name="assetPrice">Price of the relevant asset.</param>
        /// <param name="exerciseTime">Time to option exercise.</param>
        /// <param name="strike">Option strike price.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if all implied volatility parameters are valid, 
        /// otherwise false.</returns>
        public static bool
            CheckImpliedVolatilityParameters(double assetPrice,
                                             double exerciseTime,
                                             double strike,
                                             ref string errorMessage)
        {
            // Initialise the error message container.
            errorMessage = "";
            // Check asset price.
            var areAllParametersValid =
                CheckAssetPrice(assetPrice, ref errorMessage);
            if (!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check exercise time.
            areAllParametersValid =
                CheckExerciseTime(exerciseTime, ref errorMessage);
            if (!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check strike.
            areAllParametersValid =
                CheckStrike(strike, ref errorMessage);
            return areAllParametersValid;
        }

        #endregion Master Parameter Validation Method
    }
}