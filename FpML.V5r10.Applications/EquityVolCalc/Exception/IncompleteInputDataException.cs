using System;
using System.Runtime.Serialization;

namespace Orion.Equity.VolatilityCalculator.Exception
{
    /// <summary>
    /// When there is incomplete input data
    /// </summary>
    [Serializable]
    public class IncompleteInputDataException : System.Exception
    {
               const string CMessage = "Extrapolation Failure: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        public IncompleteInputDataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IncompleteInputDataException(string message): base(string.Format(CMessage, message) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IncompleteInputDataException(string message, System.Exception innerException) : base(string.Format(CMessage, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected IncompleteInputDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
