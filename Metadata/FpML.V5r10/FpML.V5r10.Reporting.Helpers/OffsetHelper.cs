#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class OffsetHelper
    {
        public  static  Offset FromInterval(Period interval, DayTypeEnum dayType)
        {
            if ((dayType != DayTypeEnum.Business) & (dayType != DayTypeEnum.Calendar))
            {
                throw new ArgumentOutOfRangeException(nameof(dayType), dayType, "Only 'DayTypeEnum.Business' and 'DayTypeEnum.Calendar' day types are currently supported.");
            }
            //  We can only use Business dayType for days intervals.
            //
            if ((dayType == DayTypeEnum.Business) & (interval.period != PeriodEnum.D))
            {
                throw new NotSupportedException();
            }         
            var offset = new Offset
                {
                    period = interval.period,
                    periodMultiplier = interval.periodMultiplier,
                    dayType = dayType,
                    dayTypeSpecified = true
                };
            return offset;
        }
    }
}