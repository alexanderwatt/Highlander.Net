#region Using directives

using FpML.V5r10.Reporting.ModelFramework.Business;

// COM interop attributes

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Italian 30/360 day count convention.
    /// </summary>
    public sealed class Thirty360Italian : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly Thirty360Italian Instance = new Thirty360Italian();

        private Thirty360Italian()
            : base("Thirty360Italian", DayCountConvention.Italian, 360) 
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