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

using System;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// A pricer for collars interface
    /// </summary>
    public interface ICollarPricer
    {
        /// <summary>
        /// Finds the price.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindPrice(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost call strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostCallStrike(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost put strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostPutStrike(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost strike.
        /// </summary>
        /// <param name="optionType">Type of the option.</param>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostStrike(OptionType optionType, Stock stock, ZeroAUDCurve zeroRateCurve);
    }
}
