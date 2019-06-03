using System;

namespace Orion.Equity.VolatilityCalculator
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
