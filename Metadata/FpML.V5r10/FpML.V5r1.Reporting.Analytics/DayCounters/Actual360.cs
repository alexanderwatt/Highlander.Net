#region Using directives

using System;
using Orion.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/360 day count convention.
    /// </summary>
    public sealed class Actual360 : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        static public readonly Actual360 Instance = new Actual360();

        private Actual360()
            : base("Actual360", DayCountConvention.Actual, 360) 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/360";
        }
    }
}
