#region Using directives

using System;
using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class OffsetHelper
    {
        public  static  Offset FromInterval(Period interval, DayTypeEnum dayType)
        {
            if ((dayType != DayTypeEnum.Business) & (dayType != DayTypeEnum.Calendar))
            {
                throw new ArgumentOutOfRangeException("dayType", dayType, "Only 'DayTypeEnum.Business' and 'DayTypeEnum.Calendar' day types are currently supported.");
            }


            //  We can only use Business dayType for days intervals.
            //
            if ((dayType == DayTypeEnum.Business) & (interval.period != PeriodEnum.D))
            {
                throw new NotSupportedException();
            }
            
            Offset offset = new Offset();
            
            offset.period = interval.period;
            offset.periodMultiplier = interval.periodMultiplier;

            offset.dayType = dayType;
            offset.dayTypeSpecified = true;

            return offset;
        }
    }
}