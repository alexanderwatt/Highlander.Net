using System;

namespace Orion.Equity.VolatilityCalculator
{
    /// <summary>
    /// A point on a volatility surface
    /// </summary>
    [Serializable]
    public class VolatilityPoint: IVolatilityPoint
    {

        private Decimal _volatility;
        private VolatilityState _volState = VolatilityState.Default();

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public Decimal Value
        {
            get => _volatility;
            set => _volatility = value;
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public VolatilityState State
        {
            get => _volState;
            set => _volState = value;
        }
       

        /// <summary>
        /// Sets the volatility.
        /// </summary>
        /// <param name="volatility">The volatility.</param>
        /// <param name="state">The state.</param>
        /// <example>
        ///     <code>
        ///     // Setting or initialising a volatility point with a default value
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatility(volatility, VolatilityState.Default());
        /// 
        ///     // Setting a volatility point with a failure value
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatility(volatility, VolatilityState.Failure(someException));
        /// 
        ///     // Setting a volatility point with a good value
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatility(volatility, VolatilityState.Success());
        ///     </code>
        /// </example>
        public void SetVolatility(Decimal volatility, VolatilityState state)
        {
            _volatility = volatility;
            _volState = state;
        }

        /// <summary>
        /// Sets the volatility failure.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <example>
        ///     <code>
        ///     // Mark a volatility with a failed state
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatilityFailure(someException);
        ///     </code>
        /// </example>
        /// <seealso cref="SetVolatility"/>
        public void SetVolatilityFailure(System.Exception exception)
        {
            _volState = VolatilityState.Failure(exception);
        }
    }
}
