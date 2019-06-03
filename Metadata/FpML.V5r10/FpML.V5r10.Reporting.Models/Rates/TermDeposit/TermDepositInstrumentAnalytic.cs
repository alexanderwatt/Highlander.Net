using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.Reporting.Models.Rates.TermDeposit
{
    public class TermDepositInstrumentAnalytic : ModelAnalyticBase<ITermDepositInstrumentParameters, TermDepositInstrumentMetrics>, ITermDepositInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal BreakEvenRate => AnalyticParameters.BreakEvenRate;

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal ImpliedQuote => AnalyticParameters.BreakEvenRate;

        ///// <summary>
        ///// Gets the derivative with respect to the Rate.
        ///// </summary>
        ///// <value>The delta wrt the fixed rate.</value>
        //public decimal DeltaR
        //{
        //    get{ return AnalyticParameters.DeltaR; }
        //}

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity => 0.0m;

        ///// <summary>
        ///// Gets the derivative with respect to the Rate.
        ///// </summary>
        ///// <value>The historical delta wrt the fixed rate.</value>
        //public decimal HistoricalDeltaR
        //{
        //    get { return AnalyticParameters.HistoricalDeltaR; }
        //}

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public Decimal BreakEvenSpread => AnalyticParameters.BreakEvenSpread;

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE => new decimal[]{};

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public Decimal[] PCETerm => new Decimal[] { };

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => AnalyticParameters.MarketQuote;
    }
}