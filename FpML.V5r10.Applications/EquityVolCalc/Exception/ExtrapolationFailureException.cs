using System;
using System.Runtime.Serialization;

namespace Orion.Equity.VolatilityCalculator.Exception
{
    /// <summary>
    /// Occures when a volatility surface point cannot be extrapolated
    /// </summary>
    [Serializable]
    public class ExtrapolationFailureException : System.Exception
    {
        const string CMessage = "Extrapolation Failure: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        public ExtrapolationFailureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ExtrapolationFailureException(string message): base(string.Format(CMessage, message) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExtrapolationFailureException(string message, System.Exception innerException) : base(string.Format(CMessage, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ExtrapolationFailureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}