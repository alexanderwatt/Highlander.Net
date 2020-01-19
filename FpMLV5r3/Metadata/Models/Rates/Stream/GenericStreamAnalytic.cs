using System;
using System.Collections.Generic;
using NabCap.QR.ModelFramework;

using Extreme.Mathematics;
using Extreme.Mathematics.EquationSolvers;

using nabCap.QR.Analytics.Helpers;

namespace nabCap.QR.AnalyticModels.Rates
{
    public class GenericStreamAnalytic : ModelAnalyticBase<IGenericStreamParameters, StreamInstrumentMetrics>, IStreamInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal BreakEvenRate
        {
            get
            {
                return this.EvaluateBreakEvenRate();
            }
        }

        /// <summary>
        /// Evaluates the break even rate without solver.
        /// </summary>
        /// <returns></returns>
        virtual public Decimal EvaluateBreakEvenRate()
        {
          return   (((this.AnalyticParameters.NotionalAmount * (this.AnalyticParameters.StartDiscountFactor - this.AnalyticParameters.EndDiscountFactor)) + this.AnalyticParameters.NPV) / this.AnalyticParameters.AccrualFactor)/10000;

        }
    }
}

