#region Using directives

using System;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class FloatingRateDefinitionHelper
    {
        public static FloatingRateDefinition CreateSimple(FloatingRateIndex floatingRateIndex, Period tenor,
            decimal derivedRate, decimal spread)
        {
            var floatingRateDefinition = new FloatingRateDefinition
                {
                    calculatedRate = derivedRate + spread,
                    calculatedRateSpecified = true,
                    spread = spread,
                    spreadSpecified = true
                };
            return floatingRateDefinition;
        }

        public static FloatingRateDefinition CreateSimple(FloatingRateIndex floatingRateIndex, Period tenor, DateTime adjustedFixingDate,
            decimal observedRate, decimal spread)
        {
            return CreateSimple(adjustedFixingDate, observedRate, spread);
        }

        public static FloatingRateDefinition CreateSimple(DateTime adjustedFixingDate, decimal observedRate, decimal spread)
        {
            var floatingRateDefinition = new FloatingRateDefinition
                {
                    calculatedRate = observedRate + spread,
                    calculatedRateSpecified = true,
                    rateObservation = RateObservationFactory.Create(adjustedFixingDate, observedRate),
                    spread = spread,
                    spreadSpecified = true
                };
            return floatingRateDefinition;
        }

        public static FloatingRateDefinition CreateCapFloor(FloatingRateIndex floatingRateIndex, Period tenor, DateTime adjustedFixingDate, 
            decimal observedRate, decimal spread, decimal capfloorstrike, bool isCap)
        {
            FloatingRateDefinition floatingRateDefinition = CreateSimple(floatingRateIndex, tenor, adjustedFixingDate, observedRate, spread);
            var strike = new Strike {strikeRate = capfloorstrike};
            if (isCap)
            {

                floatingRateDefinition.capRate = new[] { strike };
            }
            else 
            {
                floatingRateDefinition.floorRate = new[] { strike };
            }
            return floatingRateDefinition;
        }
    }
}
