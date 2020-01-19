#region Using directives

using System;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class FloatingRateDefinitionHelper
    {
        public static FloatingRateDefinition CreateSimple(FloatingRateIndex floatingRateIndex, Period tenor,
            decimal derivedRate, decimal spread)
        {
            FloatingRateDefinition floatingRateDefinition = new FloatingRateDefinition();
            floatingRateDefinition.calculatedRate = derivedRate + spread;
            floatingRateDefinition.calculatedRateSpecified = true;
            floatingRateDefinition.spread = spread;
            floatingRateDefinition.spreadSpecified = true;
            return floatingRateDefinition;
        }

        public static FloatingRateDefinition CreateSimple(FloatingRateIndex floatingRateIndex, Period tenor, DateTime adjustedFixingDate,
            decimal observedRate, decimal spread)
        {
            return CreateSimple(adjustedFixingDate, observedRate, spread);
        }

        public static FloatingRateDefinition CreateSimple(DateTime adjustedFixingDate, decimal observedRate, decimal spread)
        {
            FloatingRateDefinition floatingRateDefinition = new FloatingRateDefinition();
            floatingRateDefinition.calculatedRate = observedRate + spread;
            floatingRateDefinition.calculatedRateSpecified = true;
            floatingRateDefinition.rateObservation = RateObservationFactory.Create(adjustedFixingDate, observedRate);
            floatingRateDefinition.spread = spread;
            floatingRateDefinition.spreadSpecified = true;
            return floatingRateDefinition;
        }

        public static FloatingRateDefinition CreateCapFloor(FloatingRateIndex floatingRateIndex, Period tenor, DateTime adjustedFixingDate, 
            decimal observedRate, decimal spread, decimal capfloorstrike, bool isCap)
        {
            FloatingRateDefinition floatingRateDefinition = CreateSimple(floatingRateIndex, tenor, adjustedFixingDate, observedRate, spread);
            var strike = new Strike();
            strike.strikeRate = capfloorstrike;
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
