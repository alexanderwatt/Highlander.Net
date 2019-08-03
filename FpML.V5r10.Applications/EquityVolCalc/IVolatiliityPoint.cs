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

using System;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    public interface IVolatilityPoint
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        Decimal Value { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        VolatilityState State { get; }

        /// <summary>
        /// Sets the volatility.
        /// </summary>
        /// <param name="volatility">The volatility.</param>
        /// <param name="state">The state.</param>
        void SetVolatility(Decimal volatility, VolatilityState state);


        /// <summary>
        /// Sets the volatility failure.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void SetVolatilityFailure(System.Exception exception);
    }
}
