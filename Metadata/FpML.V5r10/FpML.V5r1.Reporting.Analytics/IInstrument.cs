
namespace FpML.V5r10.Reporting.Analytics
{
    /// <summary>
    /// Interface for concrete instruments.
    /// </summary>
    public interface IInstrument// : IObservable
    {
        /// <summary>
        /// ISIN code of the instrument, when given..
        /// </summary>
        string IsinCode
        {
            get;
        }

        /// <summary>
        /// A brief textual description of the instrument.
        /// </summary>
        string Description
        {
            get;
        }
			
        /// <summary>
        /// Net present value of the instrument.
        /// </summary>
        /// <returns>Returns the net present value.</returns>
        double NPV
        {
            get;
        }

        /// <summary>
        /// Returns whether the instrument is still tradable.
        /// </summary>
        bool IsExpired
        {
            get;
        }

        /// <summary>
        /// This method force the recalculation of the instrument value 
        /// and other results which would otherwise be cached.
        /// </summary>
        /// <remarks>
        /// Explicit invocation of this method is <b>not</b> necessary 
        /// if the instrument registered itself as observer with the 
        /// structures on which such results depend. 
        /// It is strongly advised to follow this policy when possible.
        /// </remarks>
        void Recalculate();

    }
}