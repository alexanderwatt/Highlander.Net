
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments
{
    /// <summary>
    /// TFpML - Denotes the FpML structure that gets built from the underlying Instrument
    /// </summary>
    /// <typeparam name="TFpML">The type of the fpML.</typeparam>
    public interface IPriceableInstrumentController<out TFpML> : IModelController<IInstrumentControllerData, AssetValuation>
    {
        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        TFpML Build();
    }
}