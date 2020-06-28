using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class FixedRateScheduleHelper
    {
        public static Schedule Create(decimal rate)
        {
            Schedule schedule = new Schedule {id = "fixedRate", initialValue = rate};

            return schedule;
        }
    }
}