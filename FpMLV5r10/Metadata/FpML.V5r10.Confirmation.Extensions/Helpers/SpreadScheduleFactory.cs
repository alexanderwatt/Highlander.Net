#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class SpreadScheduleFactory
    {
        public static SpreadSchedule Create(decimal initialValue)
        {
            SpreadSchedule result = new SpreadSchedule();
            result.initialValue = initialValue;
            
            return result;
        }
    }
}