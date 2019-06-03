#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class FloatingRateCalculationFactory
    {
        public static FloatingRateCalculation Create(string floatingRateIndex, string indexTenor, decimal spreadInitialValue)
        {
            var result = new FloatingRateCalculation
                {
                    floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
                    indexTenor = PeriodHelper.Parse(indexTenor),
                    spreadSchedule = new[] {SpreadScheduleFactory.Create(spreadInitialValue)}
                };
            return result;
        }
    }
}