
using FpML.V5r3.Reporting;

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    ///</summary>
    public static class FrequencyHelper
    {
        ///<summary>
        ///</summary>
        ///<param name="interval"></param>
        ///<returns></returns>
        public static int ToFrequency(Period interval)
        {
            var frequency = 0;
            if(interval.period == PeriodEnum.M)
            {
                frequency = 12 / int.Parse(interval.periodMultiplier);
            }
            if (interval.period == PeriodEnum.Y)
            {
                frequency = 1;// int.Parse(interval.periodMultiplier);
            }
            return frequency;
        }
    }
}