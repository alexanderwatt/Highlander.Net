#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class FloatingRateCalculationFactory
    {
        public static FloatingRateCalculation Create(string floatingRateIndex, string indexTenor, decimal spreadInitialValue)
        {
            FloatingRateCalculation result = new FloatingRateCalculation();
            result.floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex);
            result.indexTenor = PeriodHelper.Parse(indexTenor);
            result.spreadSchedule = new SpreadSchedule[] { SpreadScheduleFactory.Create(spreadInitialValue) };
            return result;
        }
    }
}