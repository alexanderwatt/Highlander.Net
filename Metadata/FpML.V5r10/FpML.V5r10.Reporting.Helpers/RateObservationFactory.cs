#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.Helpers
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
