using System;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    ///</summary>
    public static class CompoundingHelper
    {
        ///<summary>
        ///</summary>
        ///<param name="baseDate"></param>
        ///<param name="frequency"></param>
        ///<param name="dayCountfraction"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static decimal PeriodFractionFromCompoundingFrequency(DateTime baseDate, CompoundingFrequency frequency, DayCountFraction dayCountfraction)
        {
            return PeriodFractionFromCompoundingFrequency(baseDate, frequency.ToEnum(), dayCountfraction);
        }

        ///<summary>
        ///</summary>
        ///<param name="baseDate"></param>
        ///<param name="frequency"></param>
        ///<param name="dayCountfraction"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static decimal PeriodFractionFromCompoundingFrequency(DateTime baseDate, CompoundingFrequencyEnum frequency, DayCountFraction dayCountfraction)
        {
            switch (frequency)
            {
                case CompoundingFrequencyEnum.Continuous:
                    return 0.0m;

                case CompoundingFrequencyEnum.Daily:
                    IDayCounter dc = DayCounterHelper.Parse(dayCountfraction.Value);
                    return (decimal) dc.YearFraction(baseDate, baseDate.AddDays(1.0d));

                case CompoundingFrequencyEnum.Weekly:
                    return (decimal) 1/52;

                case CompoundingFrequencyEnum.Monthly:
                    return (decimal) 1/12;

                case CompoundingFrequencyEnum.Quarterly:
                    return (decimal) 1/4;

                case CompoundingFrequencyEnum.SemiAnnual:
                    return (decimal) 1/2;

                case CompoundingFrequencyEnum.Annual:
                    return 1.0m;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}