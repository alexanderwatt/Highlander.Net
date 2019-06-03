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
using Orion.Analytics.Utilities;

namespace Orion.Analytics.Stochastics.SABR
{
    /// <summary>
    /// Class that encapsulates all settings used for the calibration of the
    /// SABR model.
    /// </summary>
    public class SABRCalibrationSettings
    {
        #region Constructor

        /// <summary>
        /// Constructor for the  class <see cref="SABRCalibrationSettings"/>.
        /// </summary>
        /// <param name="handle">Name that identifies the SABR calibration
        ///  settings object.</param>
        /// <param name="instrument">The instrument type, for example
        /// Swaption.</param>
        /// <param name="currency">Currency as a three letter code.</param>
        /// <param name="beta">The value of the SABR parameter beta.
        /// Beta must be in the range [0.0, 1.0].</param>
        public SABRCalibrationSettings(string handle,
                                       InstrumentType.Instrument instrument,
                                       string currency,
                                       decimal beta)
        {
            // Initialise all fields, other than the SABR parameter beta.
            Handle = handle;
            Instrument = instrument;
            Currency = currency;
            
            // Initialise the SABR parameter beta.
            InitialiseBeta(beta);
        }

        #endregion Constructor

        #region Accessor Methods

        /// <summary>
        /// Accessor to the SABR parameter beta.
        /// </summary>
        /// <value>SABR parameter beta.</value>
        public decimal Beta { get; private set; }

        /// <summary>
        /// Accessor to the currency code.
        /// </summary>
        /// <value>Currency code.</value>
        public string Currency { get; }

        /// <summary>
        /// Accessor to the handle (name) of the object that encapsulates
        /// all settings for the calibration of the SABR model.
        /// </summary>
        /// <value>Handle to the SABR calibration settings object.</value>
        public string Handle { get; }

        /// <summary>
        /// Accessor to the instrument type.
        /// </summary>
        /// <value>Instrument type.</value>
        public InstrumentType.Instrument Instrument { get; }

        #endregion Accessor Methods

        #region Private Initialisation Methods

        /// <summary>
        /// Initialise the SABR parameter beta to the value that will be used
        /// during calibration of the SABR model.
        /// Function checks for a valid beta parameter.
        /// Post condition: private field _beta is set, provided that the input
        /// value beta is valid.
        /// </summary>
        /// <param name="beta">Override for the SABR parameter beta.</param>
        private void InitialiseBeta(decimal beta)
        {
            // SABR parameter beta provided: check validity.
            var errorMessage = "";
            var isValid =
                SABRParameters.CheckSABRParameterBeta(beta, ref errorMessage);

            if(!isValid)
            {
                throw new System.ArgumentException(errorMessage);
            }
            Beta = beta;
        }

        #endregion Private Initialisation Methods

        #region Object Overrides

        /// <summary>
        /// Override to suppress compiler warning.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Override the default object implementation of Equals
        /// This version will compare the raw label strings to determine equality
        /// </summary>
        /// <param name="elt">The label to compare</param>
        /// <returns>True if the values attached to the labels match</returns>
        public override bool Equals(object elt)
        {
            var eq = false;
            if (elt.GetType() == typeof (SABRCalibrationSettings))
            {
                var b_instrument = ((SABRCalibrationSettings) elt).Instrument;
                var b_currency = ((SABRCalibrationSettings) elt).Currency;
                var b_Beta = ((SABRCalibrationSettings)elt).Beta;
                eq = Instrument == b_instrument;
                eq = eq && Currency == b_currency;
                eq = eq && Beta == b_Beta;
            }
            return eq;
        }

        #endregion

        #region Private Fields

        #endregion Private Fields
    }
}