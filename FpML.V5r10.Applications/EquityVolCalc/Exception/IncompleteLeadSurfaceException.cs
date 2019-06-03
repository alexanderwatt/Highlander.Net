using System;
using System.Runtime.Serialization;

namespace Orion.Equity.VolatilityCalculator.Exception
{
    /// <summary>
    /// Occures when a lead stock does not have a full volatility surface
    /// </summary>
    [Serializable]
    public class IncompleteLeadSurfaceException : System.Exception
    {
        const string CMessage = "Lead stock surface is not complete: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        public IncompleteLeadSurfaceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IncompleteLeadSurfaceException(string message): base(string.Format(CMessage, message) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IncompleteLeadSurfaceException(string message, System.Exception innerException) : base(string.Format(CMessage, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected IncompleteLeadSurfaceException(SerializationInfo info,  StreamingContext context): base(info, context)
        {
        }

    }
}
