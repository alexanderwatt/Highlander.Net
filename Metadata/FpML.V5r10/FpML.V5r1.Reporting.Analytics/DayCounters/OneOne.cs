using System;
using Orion.ModelFramework.Business;

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/365 day count convention.
    /// </summary>
    public sealed class OneOne : DayCounterBase//TODO This has not bee n tested and is not correct!
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        static public readonly OneOne Instance = new OneOne();

        private OneOne()
            : base("OneOne", DayCountConvention.Actual, 365) 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "1/1";
        }
    }
}