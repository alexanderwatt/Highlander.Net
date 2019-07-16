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

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class RateObservationFactory
    {
        public static RateObservation[] Create(DateTime resetDate,
            DateTime adjustedFixingDate,
            decimal observedRate,
            decimal treatedRate,
            string observationWeight,
            string rateReference,
            decimal forecastRate,
            decimal treatedForecastRate,
            string id)
        {
            RateObservation rateObservation = new RateObservation();
            RateObservation[] rateObservations = new RateObservation[1];

            rateObservation.resetDate = resetDate;
            rateObservation.resetDateSpecified = true;
            rateObservation.adjustedFixingDate = adjustedFixingDate;
            rateObservation.adjustedFixingDateSpecified = true;
            rateObservation.observedRate = observedRate;
            rateObservation.observedRateSpecified = true;
            rateObservation.treatedRate = treatedRate;
            rateObservation.treatedRateSpecified = true;
            rateObservation.observationWeight = observationWeight;
            rateObservation.rateReference = RateReferenceHelper.Parse(rateReference);
            rateObservation.forecastRate = forecastRate;
            rateObservation.forecastRateSpecified = true;
            rateObservation.treatedForecastRate = treatedForecastRate;
            rateObservation.treatedForecastRateSpecified = true;
            rateObservation.id = id;

            rateObservations[0] = rateObservation;

            return rateObservations;
        }

        public static RateObservation[] Create(DateTime adjustedFixingDate,
            decimal observedRate)
        {
            RateObservation rateObservation = new RateObservation();
            RateObservation[] rateObservations = new RateObservation[1];

            rateObservation.resetDateSpecified = false;
            rateObservation.adjustedFixingDate = adjustedFixingDate;
            rateObservation.adjustedFixingDateSpecified = true;
            rateObservation.observedRate = observedRate;
            rateObservation.observedRateSpecified = true;
            rateObservation.observationWeight = "1";
            rateObservation.forecastRate = observedRate;
            rateObservation.forecastRateSpecified = true;

            rateObservations[0] = rateObservation;

            return rateObservations;
        }

    }
}
