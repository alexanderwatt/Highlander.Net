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

namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// Class that encapsulates the concept of a two dimensional key.
    /// The first part of the key is the instrument and the second part of the
    /// key is the currency.
    /// Example: (Swaption, AUD).
    /// </summary>
    public class TwoDimensionalKey
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="TwoDimensionalKey"/>.
        /// </summary>
        /// <param name="firstKeyPart">The first part of the key.</param>
        /// <param name="secondKeyPart">The second part of the key.</param>
        public TwoDimensionalKey(InstrumentType.Instrument firstKeyPart,
                                 string secondKeyPart)
        {
            FirstKeyPart = firstKeyPart;
            SecondKeyPart = secondKeyPart;
        }

        #endregion Constructor
        
        #region Accessor Methods

        /// <summary>
        /// Gets the first part of a two dimensional key.
        /// </summary>
        /// <value>First part of a two dimensional key.</value>
        public InstrumentType.Instrument FirstKeyPart { get; }

        /// <summary>
        /// Gets the second part of a two dimensional key.
        /// </summary>
        /// <value>Second part of a two dimensional key.</value>
        public string SecondKeyPart { get; }

        #endregion Accessor Methods

        #region Private Fields

        #endregion Private Fields
    }
}