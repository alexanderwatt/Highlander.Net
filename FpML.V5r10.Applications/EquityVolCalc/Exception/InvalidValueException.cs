namespace Orion.Equity.VolatilityCalculator.Exception
{
    /// <summary>
    /// When a field has been supplied with an invalid value
    /// </summary>
    public class InvalidValueException: System.Exception
    {
        /// <summary>
        /// Create a simple exception with no further explanation.
        /// </summary>
        public InvalidValueException()
        { }
        /// <summary>
        /// Create the exception with an explanation of the reason.
        /// </summary>
        /// <param name="message"></param>
        public InvalidValueException(string message) : base(message) { }
    }
}
