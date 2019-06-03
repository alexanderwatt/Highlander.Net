#region Using directives

using System;
using Math=System.Math;

#endregion

namespace Orion.Models.Rates.Futures
{
    public class RateFuturesImpliedQuote
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
        public RateFuturesImpliedQuote(double impliedRate, double volatility, double timeToExpiry)
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
            double quoteError = _rate - EvaluateAdjustmentRate(guessRate, _volatility, _timeToExpiry);
            return quoteError;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private static Double EvaluateAdjustmentRate(double rate, double volatility, double timeToExpiry)
        {
            try
            {
                double x = Math.Pow(volatility, 2);
                double t = timeToExpiry;
                double y = Math.Exp(x * t) - 1;
                double factor1 = 2 * y;
                double factor2 = t * rate - 1;
                double factor3 = Math.Pow(factor2, 2);
                double factor4 = 4 * rate / x * y;
                return (x / factor1 * (factor2 + Math.Sqrt(factor3 + factor4)));
            }
            catch
            {
                throw new Exception("Real solution does not exist");
            }
        }

    }
}