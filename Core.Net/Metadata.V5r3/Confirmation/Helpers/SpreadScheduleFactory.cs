#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class SpreadScheduleFactory
    {
        public static SpreadSchedule Create(decimal initialValue)
        {
            var result = new SpreadSchedule {initialValue = initialValue};

            return result;
        }
    }
}