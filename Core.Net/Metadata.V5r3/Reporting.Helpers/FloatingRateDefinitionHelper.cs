/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Helpers.V5r3
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
