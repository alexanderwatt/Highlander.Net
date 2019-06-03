using System;

namespace Orion.Equity.VolatilityCalculator
{
    /// <summary>
    /// States of a Volatility point extrapolation
    /// </summary>
    [Serializable]
    public enum VolatilityStateType
    {
        /// <summary>Not Set</summary>
        Unspecified = 0,
        /// <summary>Default</summary>
        Default = 1,
        /// <summary>Successfully extrapolated</summary>
        Success = 2,
        /// <summary>Failed to extrapolate</summary>
        Failure = 3,
        /// <summary>Exceeds Threshold</summary>
        ExceedsThreshold = 4,
    }

    /// <summary>
    /// Defines the state of a give volatility point
    /// </summary>
    [Serializable]
    public class VolatilityState
    {
        readonly VolatilityStateType _stateType = VolatilityStateType.Unspecified;
        readonly string _stateReason = string.Empty;
        public System.Exception FailureException { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilityState"/> class.
        /// </summary>
        public VolatilityState() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilityState"/> class.
        /// </summary>
        /// <param name="stateType">Type of the state.</param>
        /// <param name="reason">The reason.</param>
        /// <example>
        ///     <code>
        ///     // Create a default volatility state
        ///     VolatilityState state = new VolatilityState(VolatilityStateType.Default, string.Empty);
        /// 
        ///     // Create a failure volatility state
        ///     VolatilityState state = new VolatilityState(VolatilityStateType.Failure, "Unable to extrapolate");
        /// 
        ///     // Create a Success volatility state
        ///     VolatilityState state = new VolatilityState(VolatilityStateType.Success, string.Empty);
        ///     </code>
        /// </example>
        public VolatilityState(VolatilityStateType stateType, string reason)
        {
            _stateType = stateType;
            _stateReason = reason;
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public VolatilityStateType Status => _stateType;

        /// <summary>
        /// Gets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public String Reason => _stateReason;

        /// <summary>
        /// Defaults this instance.
        /// </summary>
        /// <returns></returns>
        public static VolatilityState Default()
        {
            VolatilityState state = new VolatilityState(VolatilityStateType.Default, string.Empty);
            return state;
        }

        /// <summary>
        /// Failures the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static VolatilityState Failure(System.Exception exception)
        {
            VolatilityState state =
                new VolatilityState(VolatilityStateType.Failure, exception.GetBaseException().Message)
                {
                    FailureException = exception
                };
            return state;
        }

        /// <summary>
        /// Successes this instance.
        /// </summary>
        /// <returns></returns>
        public static VolatilityState Success()
        {
            return new VolatilityState(VolatilityStateType.Success, string.Empty);
        }
    }
}
