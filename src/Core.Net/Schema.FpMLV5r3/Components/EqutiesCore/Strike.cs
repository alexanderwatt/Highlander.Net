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
using Highlander.Utilities.Helpers;

namespace Highlander.Equities
{
    /// <summary>
    /// Option Type
    /// </summary>
    public enum OptionType
    {
        /// <summary>Unknown (Not Specified)</summary>
        NotSpecified = 0,
        /// <summary>Call</summary>
        Call = 1,
        /// <summary>Put</summary>
        Put = 2,
    }

    /// <summary>
    /// Defines a strike (i.e. a pairs call and put position)
    /// </summary>
    [Serializable]
    public class Strike
    {
        private readonly Pair<OptionPosition, OptionPosition> _strikePositions;

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>The style.</value>
        public OptionType Style { get; }

        /// <summary>
        /// Gets or sets a value indicating whether [nodal point].
        /// </summary>
        /// <value><c>true</c> if [nodal point]; otherwise, <c>false</c>.</value>
        public bool NodalPoint { get; set; }

        /// <summary>
        /// Gets a value indicating whether [volatility has been set].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [volatility has been set]; otherwise, <c>false</c>.
        /// </value>
        public bool VolatilityHasBeenSet { get; private set; }

        /// <summary>
        /// Gets the strike price.
        /// </summary>
        /// <value>The strike price.</value>
        public double StrikePrice { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public IVolatilityPoint Volatility { get; private set; }

        /// <summary>
        /// Gets the call.
        /// </summary>
        /// <value>The call.</value>
        public OptionPosition Call => _strikePositions.First;

        /// <summary>
        /// Gets the put.
        /// </summary>
        /// <value>The put.</value>
        public OptionPosition Put => _strikePositions.Second;


        /// <summary>
        /// [ERROR: Unknown property access] the moneyness.
        /// </summary>
        /// <value>The moneyness.</value>
        public double Moneyness { get; set; }

        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public decimal DefaultVolatility { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        public Strike()
        {
            VolatilityHasBeenSet = false;
            StrikePrice = 0;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="strikePrice">The strike price.</param>
        public Strike(OptionType style, double strikePrice)
        {
            InputValidator.EnumTypeNotSpecified("Option Type", style, true);
            InputValidator.NotZero("Strike Price", strikePrice, true);
            Style = style;
            StrikePrice = strikePrice;
        }

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
        public Strike(double strikePrice, OptionPosition callPosition, OptionPosition putPosition)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
            InputValidator.NotZero("StrikePrice", strikePrice, true);
            StrikePrice = strikePrice;
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        /// <param name="units">The units.</param>
        public Strike(double strikePrice, OptionPosition callPosition, OptionPosition putPosition, Units units)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            InputValidator.NotZero("StrikePrice", strikePrice, true);
            StrikePrice = strikePrice;
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);
            PriceUnits = units;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="moneyness">The moneyness.</param>
        /// <param name="fwdPrice">The FWD price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        public Strike(double moneyness, double fwdPrice, OptionPosition callPosition, OptionPosition putPosition)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
            InputValidator.NotZero("StrikePrice", moneyness, true);
            StrikePrice = moneyness * fwdPrice;
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);
        }

        /// <summary>
        /// Sets the volatility.
        /// </summary>
        /// <param name="volatility">The volatility.</param>
        /// <example>
        ///     <code>
        ///     // Adding volatility to a strike with a default value
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatility(volatility, VolatilityState.Default());
        ///     Strike strike = new Strike(..);
        ///     strike.SetVolatility(point);
        ///     </code>
        /// </example>
        public void SetVolatility(IVolatilityPoint volatility)
        {
            if (!VolatilityHasBeenSet && volatility.State.Status == VolatilityStateType.Default && volatility.Value == 0)
            {
                DefaultVolatility = volatility.Value;
            }
            else if (volatility.State.Status == VolatilityStateType.Failure)
            {
                VolatilityHasBeenSet = false;
            }
            else
            {
                VolatilityHasBeenSet = true;
            }
            Volatility = volatility;
        }
    }
}
