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
using System.Collections.Generic;
using System.Xml.Serialization;
using Orion.Equity.VolatilityCalculator.Helpers;

namespace Orion.Equity.VolatilityCalculator
{
    [Serializable]
    public class ForwardExpiry : ICloneable
    {
        private readonly List<Strike> _strikes = new List<Strike>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardExpiry"/> class.
        /// </summary>
        public ForwardExpiry()
        {
            RawStrikePrices = new List<Double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardExpiry"/> class.
        /// </summary>
        public ForwardExpiry(DateTime expiryDate, decimal price)
        {
            RawStrikePrices = new List<Double>();
            FwdPrice = price;
            ExpiryDate = expiryDate;      
        }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        /// <value>The expiry date.</value>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the FWD price.
        /// </summary>
        /// <value>The FWD price.</value>
        public decimal FwdPrice { get; set; }

        /// <summary>
        /// Gets or sets the interest rate.
        /// </summary>
        /// <value>The interest rate.</value>
        public double InterestRate { get; set; }

        /// <summary>
        /// Gets or sets the strikes.
        /// </summary>
        /// <value>The strikes.</value>
        [XmlArray("Strikes")]
        public Strike[] Strikes
        {
            get
            {
                var strikes = new Strike[_strikes.Count];
                if (_strikes.Count > 0)
                {
                    _strikes.CopyTo(strikes, 0);
                }
                return strikes;
            }
        }

        /// <summary>
        /// Gets the nodal strikes.
        /// </summary>
        /// <value>The nodal strikes.</value>
        internal Strike[] NodalStrikes
        {
            get
            {
                var strikeList = new List<Strike>(_strikes);
                List<Strike> nodalStrikes = strikeList.FindAll(item => item.NodalPoint);
                return nodalStrikes.ToArray();
            }
            
        }

        /// <summary>
        /// Gets the raw strike prices.
        /// </summary>
        /// <value>The raw strike prices.</value>
        public List<double> RawStrikePrices { get; }


        /// <summary>
        /// Adds the strike.
        /// </summary>
        /// <param name="strike">The strike.</param>
        /// <param name="nodal"></param>
        /// <example>
        ///     <code>
        ///     // Create a Strike instance
        ///     OptionPosition callOption = new OptionPosition("123", 456, PositionType.Call);
        ///     OptionPosition putOption = new OptionPosition("123", 789, PositionType.Put);
        ///     Strike strike = new Strike(123, callOption, putOption);
        /// 
        ///     // Adds a strike to an expiry instance
        ///     ForwardExpiry expiry = new ForwardExpiry(new DateTime(2008,10,21), 400.97, 8.4);
        ///     expiry.AddStrike(strike);
        ///     </code>
        /// </example>
        public void AddStrike(Strike strike, bool nodal)
        {
            StrikeHelper.AddStrike(strike, _strikes);
            strike.NodalPoint = nodal;
            RawStrikePrices.Add(strike.StrikePrice);           
        }


        /// <summary>
        /// Removes the strike.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <example>
        ///     // Adds the AMP subsidiary stock to the BHP lead stock instance
        ///     Strike strike = new Strike(123, callPosition<see>
        ///                                                     <cref>T:nabCap.QR.EquityVolatility.OptionPosition</cref>
        ///                                                 </see>
        ///     , putPosition<see>
        ///                      <cref>T:nabCap.QR.EquityVolatility.OptionPosition</cref>
        ///                  </see>
        ///     );
        ///     ForwardExpiry expiry = new ForwardExpiry(new DateTime(2008,10,21), 400.97, 8.4);
        ///     expiry.AddStrike(strike);
        /// 
        ///     // Remove it
        ///     expiry.RemoveStrike(123);
        /// </example>
        public void RemoveStrike(Double strikePrice)
        {
            Strike matchedStrike = _strikes.Find(strike => (strike.StrikePrice == strikePrice)
                );

            if (matchedStrike != null)
            {
                _strikes.Remove(matchedStrike);
            }

            if (RawStrikePrices.Contains(strikePrice))
            {
                RawStrikePrices.Remove(strikePrice);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return MemberwiseClone();
        }     
    }
}
