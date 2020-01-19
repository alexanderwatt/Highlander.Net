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
using Highlander.Equities;
using Highlander.Equity.Calculator.V5r3.Interpolation;

namespace Highlander.Equity.Calculator.V5r3
{
    /// <summary>
    /// Defines a strike (i.e. a pairs call and put position)
    /// </summary>
    [Serializable]
    public class EquityStrike : Strike
    {
        /// <summary>
        /// Gets or sets the tenor interpolation model.
        /// </summary>
        /// <value>The tenor model.</value>
        public WingInterpolation InterpolationModel { get; set; }

        public EquityStrike() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        /// <example>
        ///     <code>
        ///     // Creates a Strike instance
        ///     OptionPosition callOption = new OptionPosition("123", 456, PositionType.Call);
        ///     OptionPosition putOption = new OptionPosition("123", 789, PositionType.Put);
        ///     Strike strike = new Strike(123, callOption, putOption);
        ///     </code>
        /// </example>
        public EquityStrike(double strikePrice, OptionPosition callPosition, OptionPosition putPosition) 
            : base(strikePrice, callPosition, putPosition)
        {
            InterpolationModel = new WingInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        /// <param name="units">The units.</param>
        public EquityStrike(double strikePrice, OptionPosition callPosition, OptionPosition putPosition, Units units)
            : base(strikePrice, callPosition, putPosition, units)
        {
            InterpolationModel = new WingInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="moneyness">The moneyness.</param>
        /// <param name="fwdPrice">The FWD price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        public EquityStrike(double moneyness, double fwdPrice, OptionPosition callPosition, OptionPosition putPosition)
            : base(moneyness, fwdPrice, callPosition, putPosition)
        {
            InterpolationModel = new WingInterpolation();
        }
    }
}
