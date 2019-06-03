using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    /// <summary>
    /// Create an instance of a StrikeSchedule. These are used in Calculation objects for CapFloor instruments/products
    /// </summary>
    public static class StrikeScheduleFactory
    {
        public static StrikeSchedule Create(decimal initialValue)
        {
            StrikeSchedule result = new StrikeSchedule {initialValue = initialValue};
            return result;
        }
    }
}
