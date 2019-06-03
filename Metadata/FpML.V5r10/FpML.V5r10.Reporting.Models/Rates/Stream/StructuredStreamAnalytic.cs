#region Usings

using System;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Solvers;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Stream
{
    public class StructuredStreamAnalytic : ModelAnalyticBase<IStructuredStreamParameters, StreamInstrumentMetrics>, IStreamInstrumentResults, IObjectiveFunction
    {
        #region Properties

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal BreakEvenRate => EvaluateBreakEvenRate();

        ///// <summary>
        ///// Gets the discount factor at maturity.
        ///// </summary>
        ///// <value>The discount factor at maturity.</value>
        //public decimal DiscountFactorAtMaturity
        //{
        //    get { return 0.0m; }
        //}

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public Decimal BreakEvenSpread => EvaluateBreakEvenSpread();

        /// <summary>
        /// Evaluates the break even rate without solver.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            //if (!AnalyticParameters.IsDiscounted)
            //{
                if (AnalyticParameters.AccrualFactor != 0.0m)
                {
                    result = AnalyticParameters.FloatingNPV / AnalyticParameters.AccrualFactor / 10000;
                }              
            //}
            //else
            //{
            //    const double accuracy = 10e-14;
            //    const double guess = 0.1;
            //    var solver = new Newton();
            //    result = Convert.ToDecimal(solver.Solve(this, accuracy, guess, guess));//TODO check this sucker
            //}
            return result;         
        }

        /// <summary>
        /// Evaluates the break even rate without solver.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateBreakEvenSpread()
        {
            var result = 0.0m;
            if (!AnalyticParameters.IsDiscounted)
            {
                if (AnalyticParameters.AccrualFactor != 0.0m)
                {
                    result = (AnalyticParameters.NPV - AnalyticParameters.FloatingNPV) / AnalyticParameters.AccrualFactor / 10000;
                }               
            }
            else
            {
                const double accuracy = 10e-14;
                const double guess = 0.1;
                var solver = new Newton();
                result = Convert.ToDecimal(solver.Solve(this, accuracy, guess, guess));//TODO check this sucker
            }
            return result;
        }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote => BreakEvenRate;

        #endregion

        #region Implementation of IObjectiveFunction

        /// <summary>
        /// Definiton of the objective function.
        /// </summary>
        /// <param name="fixedRate">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        public double Value(double fixedRate)
        {
            var result = AnalyticParameters.TargetNPV;
            //// update fixed leg
            var notionals = AnalyticParameters.CouponNotionals.Length;
            var accruals = AnalyticParameters.CouponYearFractions.Length;
            var discounts = AnalyticParameters.PaymentDiscountFactors.Length;
            if (accruals == discounts && accruals == notionals)
            {
                for (var i = 0; i < accruals; i++)
                {
                    result += AnalyticParameters.CouponNotionals[i] * AnalyticParameters.CouponYearFractions[i] * AnalyticParameters.PaymentDiscountFactors[i] * (decimal)fixedRate;
                }
            }
            return (double)result;
        }

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        public double Derivative(double x)
        {
            throw new NotImplementedException();
        }

        #endregion

        public decimal GetMultiplier()
        {
            var multiplier = AnalyticParameters.Multiplier;
            if (multiplier != null)
            {
                return (decimal)multiplier;
            }
            return 1.0m;
        }
    }
}