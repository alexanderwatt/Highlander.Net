using System;
using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class StepHelper
    {
        public static Step Create(DateTime stepDate, decimal stepValue)
        {
            Step step = new Step();

            step.stepDate = stepDate;
            step.stepValue = stepValue;

            return step;

        }

        public static Step Create(string id, DateTime stepDate, decimal stepValue)
        {
            Step step = new Step();

            step.id = id;
            step.stepDate = stepDate;
            step.stepValue = stepValue;
                
            return step;

        }
    }
}