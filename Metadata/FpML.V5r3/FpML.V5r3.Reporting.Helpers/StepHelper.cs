using System;

namespace FpML.V5r3.Reporting.Helpers
{
    public static class StepHelper
    {
        public static Step Create(DateTime stepDate, decimal stepValue)
        {
            var step = new Step {stepDate = stepDate, stepValue = stepValue};

            return step;

        }

        public static Step Create(string id, DateTime stepDate, decimal stepValue)
        {
            var step = new Step {id = id, stepDate = stepDate, stepValue = stepValue};

            return step;

        }
    }
}