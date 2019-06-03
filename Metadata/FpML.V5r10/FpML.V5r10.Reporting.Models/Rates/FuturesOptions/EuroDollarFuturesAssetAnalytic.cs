using System;

using nabCap.QR.Analytics.Convexity;
using NabCap.QR.ModelFramework;

namespace nabCap.QR.AnalyticModels.Rates
{
    public class EuroDollarFuturesAssetAnalytic : ModelAnalyticBase<IFuturesAssetParameters, RateMetrics>, IRateAssetResults
    {
        private const Decimal cOne = 1.0m;

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV
        {
            get
            {
                return EvaluateNPV();
            }
        }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal ImpliedQuote
        {
            get
            {
                return EvaluateImpliedQuote();
            }
        }

        /// <summary>
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        public decimal DeltaR
        {
            get
            {
                return EvaluateDeltaR();
            }
        }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public Decimal AccrualFactor
        {
            get
            {
                return EvaluateAccrualFactor();
            }
        }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public Decimal ConvexityAdjustment
        {
            get
            {
                return EvaluateConvexityAdjustment(AnalyticParameters.Rate);
            }
        }

        /// <summary>
        /// Gets the adjusted rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal AdjustedRate
        {
            get
            {
                return EvaluateAdjustedRate();
            }
        }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity
        {
            get
            {
                return EvaluateDiscountFactorAtMaturity();
            }
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote
        {
            get
            {
                return EvaluateMarketRate();
            }
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()//TODo the aqdjustment needs to be at the rate, not the implied rate.
        {
            try
            {
                var result = FuturesConvexity.FuturesImpliedQuoteFromMarginAdjustedWithArrears(EvaluateImpliedRate(),
                                                                 (double)AnalyticParameters.YearFraction,
                                                                 (double)AnalyticParameters.TimeToExpiry,
                                                                 (double)AnalyticParameters.Volatility);
                return result;
            }
            catch
            {
                throw new Exception("Real solution does not exist");
            }
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateConvexityAdjustment(Decimal rate)
        {
            return FuturesConvexity.FuturesMarginWithArrearsConvexityAdjustment(rate,
                                                                 (double)AnalyticParameters.YearFraction,
                                                                 (double)AnalyticParameters.TimeToExpiry,
                                                                 (double)AnalyticParameters.Volatility);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        virtual public Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor / (cOne + AnalyticParameters.YearFraction * EvaluateAdjustedRate());
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            return AnalyticParameters.Position * (AnalyticParameters.Rate - ImpliedQuote) * 25m;

        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAdjustedRate()
        {
            return EvaluateMarketRate() - EvaluateConvexityAdjustment(AnalyticParameters.Rate);
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateDeltaR()//TODO this is not correct.
        {
            return AnalyticParameters.Position * 25m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateMarketRate()//TODO this is not correct.
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the accrual factor
        /// </summary>
        /// <returns></returns>
        public Decimal EvaluateAccrualFactor()
        {
            return 0.25m;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public Decimal EvaluateImpliedRate()
        {
            var rate = (AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor - cOne) / AnalyticParameters.YearFraction;
            return rate;
        }
    }
}
