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


#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Helpers
{
    /// <summary>
    /// Class that encapsulates the settings used to calibrate a Caplet 
    /// volatility smile.
    /// SABR calibration is used to calibrate the Caplet smile at each
    /// expiry. The union of all SABR calibrations constitutes the Caplet
    /// volatility smile.
    /// </summary>
    public class SmileCalibrationSettings
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="CapletSmileCalibrationSettings"/>.
        /// </summary>
        /// <param name="beta">SABR parameter Beta used to calibrate the Caplet 
        /// smile at each expiry.</param>
        /// <param name="expiryInterpolationType">Interpolation type applied in
        /// the expiry dimension.</param>
        /// <param name="handle">Unique name used to identify the settings 
        /// object that will be instantiated.</param>
        public SmileCalibrationSettings
            (decimal beta, 
             ExpiryInterpolationType expiryInterpolationType,
             string handle)
        {
            // Map arguments to the correct private field.
            Beta = beta;
            ExpiryInterpolationType = expiryInterpolationType;
            Handle = handle;
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Accessor method for the private field _beta.
        /// </summary>
        /// <value>SABR parameter Beta used to calibrate the Caplet smile.
        /// </value>
        public decimal Beta { get; }

        /// <summary>
        /// Accessor method for the private field _expiryInterpolationType.
        /// </summary>
        /// <value>The type of the interpolation used in the expiry
        /// dimension.</value>
        public ExpiryInterpolationType ExpiryInterpolationType { get; }

        /// <summary>
        /// Accessor method for the private field _handle.
        /// </summary>
        /// <value>Unique name used to identify the settings object.</value>
        public string Handle { get; }

        #endregion

        #region Private Fields

        #endregion
    }
}