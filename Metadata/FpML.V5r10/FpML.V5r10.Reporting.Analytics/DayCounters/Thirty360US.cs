#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// American 30/360 day count convention.
    /// </summary>
    public sealed class Thirty360US : DayCounterBase
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly Thirty360US Instance = new Thirty360US();

        private Thirty360US()
            : base("Thirty360US", DayCountConvention.US, 360) 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "30/360";
        }
    }
}