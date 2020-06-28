namespace FpML.V5r3.Confirmation
{
    public static class FixedRateScheduleHelper
    {
        public static Schedule Create(decimal rate)
        {
            var schedule = new Schedule {id = "fixedRate", initialValue = rate};

            return schedule;
        }
    }
}