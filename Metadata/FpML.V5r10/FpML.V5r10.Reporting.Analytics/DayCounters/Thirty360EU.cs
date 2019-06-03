using FpML.V5r10.Reporting.ModelFramework.Business;

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// European 30/360 day count convention.
    /// </summary>
    public sealed class Thirty360EU : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly Thirty360EU Instance = new Thirty360EU();

        private Thirty360EU()
            : base("Thirty360EU", DayCountConvention.EU, 360) 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "30E/360";
        }
    }
}