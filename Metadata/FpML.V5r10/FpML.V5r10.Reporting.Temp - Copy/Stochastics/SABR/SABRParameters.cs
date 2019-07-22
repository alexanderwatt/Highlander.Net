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

namespace Orion.Analytics.Stochastics.SABR
{
    /// <summary>
    /// Class that encapsulates the four parameters used by the SABR model.
    /// The four SABR parameters are:
    /// 1) alpha: related to the ATM implied volatility;
    /// 2) beta: determines the distribution of the asset process;
    /// 3) nu: volatility of the volatility (VolVol);
    /// 4) rho: correlation between the asset and volatility.
    /// No data validation occurs in the constructor, but the class does
    /// provide static methods to validate individual SABR parameters; this 
    /// design choice was made to enhance performance.
    /// 
    /// </summary>
    public class SABRParameters
    {
        #region Constructor

        /// <summary>
        /// Initializes an instance of the class <see cref="SABRParameters"/> 
        /// </summary>
        /// <param name="alpha">Parameter alpha in the SABR model.
        /// Precondition: alpha > 0.0.</param>
        /// <param name="beta">Parameter beta in the SABR model.
        /// Precondition: beta must be in the range [0,1].</param>
        /// <param name="nu">Parameter nu in the SABR model.
        /// Precondition: nu >= 0.0.</param>
        /// <param name="rho">Parameter rho in the SABR model.
        /// Precondition: rho must be in the range (-1, 1).</param>
        public SABRParameters(decimal alpha, decimal beta, decimal nu, decimal rho)
        {
            Alpha = alpha;
            Beta = beta;
            Nu = nu;
            Rho = rho;
        }

        /// <summary>
        /// Initializes an instance of the class <see cref="SABRParameters"/> 
        /// </summary>
        /// <param name="alpha">Parameter alpha in the SABR model.
        /// Precondition: alpha > 0.0.</param>
        /// <param name="beta">Parameter beta in the SABR model.
        /// Precondition: beta must be in the range [0,1].</param>
        /// <param name="nu">Parameter nu in the SABR model.
        /// Precondition: nu >= 0.0.</param>
        /// <param name="rho">Parameter rho in the SABR model.
        /// Precondition: rho must be in the range (-1, 1).</param>
        public SABRParameters(double alpha, double beta, double nu, double rho)
        {
            Alpha = (decimal)alpha;
            Beta = (decimal)beta;
            Nu = (decimal)nu;
            Rho = (decimal)rho;
        }

        #endregion Constructor

        #region Public Individual Parameter Validation Methods

        /// <summary>
        /// Checks the validity of the SABR parameter alpha.
        /// </summary>
        /// <param name="alpha">Parameter alpha in the SABR model.
        /// Precondition: alpha > 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if SABR parameter alpha is valid, otherwise
        /// false.</returns>
        public static bool CheckSABRParameterAlpha(decimal alpha,
                                                   ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the SABR parameter.
            if(alpha <= 0.0m)
            {
                isParameterValid = false;
                errorMessage = "SABR parameter alpha must be positive.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the SABR parameter beta.
        /// </summary>
        /// <param name="beta">Parameter beta in the SABR model.
        /// Precondition: beta must be in the range [0,1].</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if SABR parameter beta is valid, otherwise
        /// false.</returns>
        public static bool CheckSABRParameterBeta(decimal beta,
                                                  ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the SABR parameter.
            if(beta < 0.0m || beta > 1.0m)
            {
                isParameterValid = false;
                errorMessage = "SABR parameter beta not in the range [0,1].";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the SABR parameter nu.
        /// </summary>
        /// <param name="nu">Parameter beta in the SABR model.
        /// Precondition: nu >= 0.0.</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if SABR parameter nu is valid, otherwise
        /// false.</returns>
        public static bool CheckSABRParameterNu(decimal nu,
                                                ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the SABR parameter.
            if (nu < 0.0m)
            {
                isParameterValid = false;
                errorMessage = "SABR parameter nu cannot be negative.";
            }
            return isParameterValid;
        }

        /// <summary>
        /// Checks the validity of the SABR parameter rho.
        /// </summary>
        /// <param name="rho">Parameter rho in the SABR model.
        /// Precondition: rho must be in the range (-1, 1).</param>
        /// <param name="errorMessage">Container for any possible error 
        /// message.</param>
        /// <returns>True if SABR parameter rho is valid, otherwise
        /// false.</returns>
        public static bool CheckSABRParameterRho(decimal rho,
                                                 ref string errorMessage)
        {
            // Initialise the return variable.
            var isParameterValid = true;
            errorMessage = "";
            // Check the SABR parameter.
            //var check = Math.Abs(rho);
            if (rho >= 1.0m || rho <= -1.0m)
            {
                isParameterValid = false;
                errorMessage = "SABR parameter rho not in the range (-1,1)";
            }
            return isParameterValid;
        }

        #endregion Public Individual Parameter Validation Methods

        #region Public Master Parameter Validation Method

        /// <summary>
        /// Master (wrapper) function that checks the validity of the
        /// SABR parameters.
        /// Valid bounds/ranges for the SABR parameters are defined in the
        /// functions the check the individual SABR parameters.
        /// </summary>
        /// <param name="sabrParameters">Object that contains all the 
        /// SABR parameters.</param>
        /// <param name="errorMessage">Container for any possible error
        /// message.</param>
        /// <returns>
        /// True if all SABR parameters are valid, otherwise
        /// false.
        /// </returns>
        public static bool CheckSABRParameters(SABRParameters sabrParameters,
                                               ref string errorMessage)
        {
            // Initialise the error message container. 
            errorMessage = "";
            // Check SABR parameter alpha.
            var areAllParametersValid =
                CheckSABRParameterAlpha(sabrParameters.Alpha,
                                        ref errorMessage);
            if(!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check SABR parameter beta.
            areAllParametersValid = CheckSABRParameterBeta(sabrParameters.Beta,
                                                           ref errorMessage);
            if (!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check SABR parameter nu.
            areAllParametersValid = CheckSABRParameterNu(sabrParameters.Nu,
                                                         ref errorMessage);
            if (!areAllParametersValid)
            {
                return areAllParametersValid;
            }
            // Check SABR parameter rho.
            areAllParametersValid = CheckSABRParameterRho(sabrParameters.Rho,
                                                          ref errorMessage);
            return areAllParametersValid;
        }

        #endregion Public Master Parameter Validation Method

        #region Public Accessor and Setter Methods

        /// <summary>
        /// Public accessor methods for the SABR parameter alpha.
        /// </summary>
        /// <value>Gets or sets the SABR parameter alpha.</value>
        public decimal Alpha { get; set; }

        /// <summary>
        /// Public accessor methods for the SABR parameter beta.
        /// </summary>
        /// <value>Gets or sets the SABR parameter alpha.</value>
        public decimal Beta { get; set; }

        /// <summary>
        /// Public accessor methods for the SABR parameter nu.
        /// </summary>
        /// <value>Gets or sets the SABR parameter nu.</value>
        public decimal Nu { get; set; }

        /// <summary>
        /// Public accessor methods for the SABR parameter rho.
        /// </summary>
        /// <value>Gets or sets the SABR parameter rho.</value>
        public decimal Rho { get; set; }

        #endregion Public Accessor and Setter Methods

    }
}