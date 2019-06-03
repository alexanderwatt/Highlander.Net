#region Using directives

using System;
using Orion.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/365 day count convention.
    /// </summary>
    public sealed class Business252 : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        static public readonly Business252 Instance = new Business252();

        private Business252()
            : base("Business252") 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/252.FIXED";
        }
    }
}