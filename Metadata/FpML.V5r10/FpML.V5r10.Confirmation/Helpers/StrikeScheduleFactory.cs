namespace FpML.V5r3.Confirmation
{
    /// <summary>
    /// Create an instance of a StrikeSchedule. These are used in Calculation objects for CapFloor instruments/products
    /// </summary>
    public static class StrikeScheduleFactory
    {
        public static StrikeSchedule Create(decimal initialValue)
        {
            var result = new StrikeSchedule {initialValue = initialValue};
            return result;
        }
    }
}
