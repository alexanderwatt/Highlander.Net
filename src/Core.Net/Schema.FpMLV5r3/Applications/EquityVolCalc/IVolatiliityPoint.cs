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

using System;

namespace Highlander.EquityVolatilityCalculator.V5r3
{
    public interface IVolatilityPoint
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        decimal Value { get; }

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
        void SetVolatility(decimal volatility, VolatilityState state);


        /// <summary>
        /// Sets the volatility failure.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void SetVolatilityFailure(Exception exception);
    }
}
