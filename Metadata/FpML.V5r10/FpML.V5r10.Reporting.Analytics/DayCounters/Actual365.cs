#region Using directives

using FpML.V5r10.Reporting.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/365 day count convention.
    /// </summary>
    public sealed class Actual365 : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly Actual365 Instance = new Actual365();

        public Actual365()
            : base("Actual365", DayCountConvention.Actual, 365)
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/365.FIXED";
        }
    }
}
