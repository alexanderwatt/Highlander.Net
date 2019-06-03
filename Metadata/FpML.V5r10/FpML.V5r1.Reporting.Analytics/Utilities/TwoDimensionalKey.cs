
namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// Class that encapsulates the concept of a two dimensional key.
    /// The first part of the key is the instrument and the second part of the
    /// key is the currency.
    /// Example: (Swaption, AUD).
    /// </summary>
    public class TwoDimensionalKey
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="TwoDimensionalKey"/>.
        /// </summary>
        /// <param name="firstKeyPart">The first part of the key.</param>
        /// <param name="secondKeyPart">The second part of the key.</param>
        public TwoDimensionalKey(InstrumentType.Instrument firstKeyPart,
                                 string secondKeyPart)
        {
            _firstKeyPart = firstKeyPart;
            _secondKeyPart = secondKeyPart;
        }

        #endregion Constructor
        
        #region Accessor Methods

        /// <summary>
        /// Gets the first part of a two dimensional key.
        /// </summary>
        /// <value>First part of a two dimensional key.</value>
        public InstrumentType.Instrument FirstKeyPart
        {
            get {return _firstKeyPart;}
        }

        /// <summary>
        /// Gets the second part of a two dimensional key.
        /// </summary>
        /// <value>Second part of a two dimensional key.</value>
        public string SecondKeyPart
        {
            get { return _secondKeyPart; }
        }

        #endregion Accessor Methods

        #region Private Fields

        /// <summary>
        /// First part of a two dimensional key.
        /// </summary>
        private readonly InstrumentType.Instrument _firstKeyPart;

        /// <summary>
        /// Second part of a two dimensional key.
        /// </summary>
        private readonly string _secondKeyPart;

        #endregion Private Fields
    }
}