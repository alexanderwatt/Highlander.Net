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

namespace Highlander.Reporting.Analytics.V5r3.Options
{
    /// <summary>
    /// Class that encapsulates a Black vanilla European swaption.
    /// The principal method offered to clients of the class is the
    /// functionality to price European payer and receiver swaptions.
    /// No data validation of inputs is performed by the class.
    /// </summary>
    public class BlackVanillaSwaption
    {
        #region Public Enum for Valid Swaption Types

        /// <summary>
        /// Enumerated type for the category of valid Black vanilla swaptions. 
        /// </summary>
        public enum SwaptionType
        {
            /// <summary>
            /// Payer swaption.
            /// </summary>
            Payer, 
            /// <summary>
            /// Receiver swaption
            /// </summary>
            Receiver
        }

        #endregion Public Enum for Valid Swaption Types

        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="BlackVanillaSwaption"/>.
        /// </summary>
        /// <param name="swaptionType">Identifier for the swaption type.</param>
        /// <param name="strike">Swaption strike expressed as a decimal.
        /// Example: 0.09 for a strike of 9%.</param>
        /// <param name="optionExpiry">Option expiry expressed in years.
        /// Example: 0.5 for a 6 month option expiry.</param>
        public BlackVanillaSwaption(SwaptionType swaptionType, double strike, double optionExpiry)
        {
            // Initialise all private fields.
            _swaptionType = swaptionType;
            _strike = strike;
            _optionExpiry = optionExpiry;
        }

        #endregion Constructor

        #region Public Business Logic Methods

        /// <summary>
        /// Functionality to price a Black vanilla swaption.
        /// </summary>
        /// <param name="swapRate">Swap rate expressed as a decimal.
        /// Example: 0.07 for a swap rate of 7%.</param>
        /// <param name="annuityFactor">Annuity factor, for the swap 
        /// associated with the swaption.</param>
        /// <param name="sigma">Black yield volatility expressed as a decimal.
        /// Example: 0.1191 for a volatility of 11.91%.</param>
        /// <returns>Price of a Black vanilla swaption.</returns>
        public double PriceBlackVanillaSwaption(double swapRate, double annuityFactor, double sigma)
        {
            var price = _swaptionType == SwaptionType.Payer 
                ? PriceBlackVanillaPayerSwaption(swapRate, annuityFactor, sigma) 
                : PriceBlackVanillaReceiverSwaption(swapRate, annuityFactor, sigma);
            return price;
        }

        #endregion Public Business Logic Methods

        #region Private Helper Business Logic Methods

        /// <summary>
        /// Functionality to price a Black vanilla payer swaption.
        /// </summary>
        /// <param name="swapRate">Swap rate expressed as a decimal.
        /// Example: 0.07 for a swap rate of 7%.</param>
        /// <param name="annuityFactor">Annuity factor, for the swap 
        /// associated with the swaption.</param>
        /// <param name="sigma">Black yield volatility expressed as a decimal.
        /// Example: 0.1191 for a volatility of 11.91%.</param>
        /// <returns>Price of a Black vanilla payer swaption.</returns>
        private double PriceBlackVanillaPayerSwaption(double swapRate, double annuityFactor, double sigma)
        {
            var model = new BlackScholesMertonModel(true, swapRate, _strike, sigma, _optionExpiry);
            // Compute and return the price of a Black vanilla payer swaption.
            var price = annuityFactor * model.Value;
            return price;
        }

        /// <summary>
        /// Functionality to price a Black vanilla receiver swaption.
        /// </summary>
        /// <param name="swapRate">Swap rate expressed as a decimal.
        /// Example: 0.07 for a swap rate of 7%.</param>
        /// <param name="annuityFactor">Annuity factor, for the swap 
        /// associated with the swaption.</param>
        /// <param name="sigma">Black yield volatility expressed as a decimal.
        /// Example: 0.1191 for a volatility of 11.91%.</param>
        /// <returns>Price of a Black vanilla receiver swaption.</returns>
        private double PriceBlackVanillaReceiverSwaption(double swapRate, double annuityFactor, double sigma)
        {
            var model = new BlackScholesMertonModel(false, swapRate, _strike, sigma, _optionExpiry);
            // Compute and return the price of a Black vanilla payer swaption.
            var price = annuityFactor * model.Value;
            return price;
        }

        #endregion Private Helper Business Logic Methods

        #region Private Fields

        /// <summary>
        /// Option expiry expressed in years.
        /// Example: 0.5 for a 6 month option expiry.
        /// </summary>
        private readonly double _optionExpiry;
        
        /// <summary>
        /// Swaption strike expressed as a decimal.
        /// Example: 0.09 for a strike of 9%.
        /// </summary>
        private readonly double _strike;
        
        /// <summary>
        /// Identifier for the swaption type.
        /// </summary>
        private readonly SwaptionType _swaptionType;

        #endregion Private Fields
    }
}