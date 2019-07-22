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

#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.FuturesOptions
{
    public class RateFuturesOptionImpliedQuote
    {
        private readonly double _rate;
        private readonly double _volatility;
        private readonly double _timeToExpiry;

        /// <summary>
        /// Futures implied quote.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="volatility"></param>
        /// <param name="timeToExpiry"></param>
        public RateFuturesOptionImpliedQuote(double impliedRate, double volatility, double timeToExpiry)
        {
            _rate = impliedRate;
            _volatility = volatility;
            _timeToExpiry = timeToExpiry;
        }

        /// <summary>
        /// Values the specified discount factor.
        /// </summary>
        /// <param name="guessRate">The guessed rate.</param>
        /// <returns>The error</returns>
        public double Value(double guessRate)
        {
            var quoteError = _rate - EvaluateImpliedVolatility(guessRate, _volatility, _timeToExpiry);
            return quoteError;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private static Double EvaluateImpliedVolatility(double rate, double volatility, double timeToExpiry)
        {
            try
            {
                var x = System.Math.Pow(volatility, 2);
                var t = timeToExpiry;
                var y = System.Math.Exp(x * t) - 1;
                var factor1 = 2 * y;
                var factor2 = t * rate - 1;
                var factor3 = System.Math.Pow(factor2, 2);
                var factor4 = 4 * rate / x * y;
                return x / factor1 * (factor2 + System.Math.Sqrt(factor3 + factor4));
            }
            catch
            {
                throw new System.Exception("Real solution does not exist");
            }
        }
    }
}