
namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// Static class that encapsulates the concept of a financial instrument,
    /// for example Swaption, CapFloor.
    /// The class is a central storage for the names of valid financial
    /// instruments associated with various analytics components.
    /// </summary>
    public static class InstrumentType
    {
        #region Enumerated Type of all Valid Instruments

        /// <summary>
        /// The indtrument.
        /// </summary>
        public enum Instrument
        {
            /// <summary>
            /// Cap/Floor
            /// </summary>
            CapFloor,
            /// <summary>
            /// Swaption
            /// </summary>
            Swaption,
            /// <summary>
            /// Call/Put
            /// </summary>
            CallPut

            
        }

        #endregion Enumerated Type of all Valid Instruments
    }
}