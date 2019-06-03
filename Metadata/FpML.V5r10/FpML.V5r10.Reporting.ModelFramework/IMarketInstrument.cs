#region Using directives
using FpML.V5r10.Reporting.ModelFramework.Assets;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Interface for concrete instruments.
    /// </summary>
    public interface IMarketInstrument// : IObservable
    {
        /// <summary>
        /// ISIN code of the instrument, when given..
        /// </summary>
        string Identifier
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
        /// This method force the recalculation of the instrument value 
        /// and other results which would otherwise be cached.
        /// </summary>
        /// <remarks>
        /// Explicit invocation of this method is <b>not</b> necessary 
        /// if the instrument registered itself as observer with the 
        /// structures on which such results depend. 
        /// It is strongly advised to follow this policy when possible.
        /// </remarks>
        void Recalculate(IAssetControllerData modeData);

        ///<summary>
        /// Gets the contained priceable asset.
        /// This is to allow the binary serialiser
        /// in the Local cache to recognise the type.
        ///</summary>
        ///<returns></returns>
        IPriceableAssetController GetPriceableAsset();
    }
}