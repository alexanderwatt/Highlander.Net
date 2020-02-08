using System;
using National.QRSC.ModelFramework;

namespace National.QRSC.AnalyticModels.Generic.Coupons
{
    public class CouponAnalytic : ModelAnalyticBase<ICouponParameters, CouponMetrics>, ICouponResults
    {
        protected const Decimal cOne = 1.0m;
        protected const Decimal BasisPoint = 10000.0m;

        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        public decimal BucketedDelta1
        {
            get { return LocalCurrencyBucketedDelta1 * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        public decimal[] BucketedDeltaVector
        {
            get
            {
                var length = LocalCurrencyBucketedDeltaVector.Length;
                var result = new Decimal[length];
                for (int index = 0; index <= length; index++)
                {
                    result[index] = LocalCurrencyBucketedDeltaVector[index] * EvaluateReportingCurrencyFxRate();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        public decimal[] BucketedDeltaVector2
        {
            get
            {
                var length = LocalCurrencyBucketedDeltaVector2.Length;
                var result = new Decimal[length];
                for (int index = 0; index <= length; index++)
                {
                    result[index] = LocalCurrencyBucketedDeltaVector2[index] * EvaluateReportingCurrencyFxRate();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal Delta1
        {
            get { return LocalCurrencyDelta1 * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal ExpectedValue
        {
            get { return LocalCurrencyExpectedValue * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal HistoricalValue
        {
            get { return LocalCurrencyHistoricalValue * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        public decimal NFV
        {
            get { return LocalCurrencyNFV * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public decimal NPV
        {
            get { return LocalCurrencyNPV * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        public decimal SimpleCVA
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        public decimal HistoricalDelta1
        {
            get { return LocalCurrencyHistoricalDelta1 * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal LocalCurrencyAccrualFactor
        {
            get { return EvaluateAccrualFactor(); }
        }

        ///// <summary>
        ///// Gets the accrual factor.
        ///// </summary>
        ///// <value>The accrual factor.</value>
        //public decimal LocalCurrencyAccrualFactor
        //{
        //    get { return AccrualFactor; }
        //}

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor
        {
            get { return LocalCurrencyAccrualFactor * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate
        {
            get { return EvaluateBreakEvenRate(); }
        }

        /// <summary>
        /// Gets the Bucketed Delta
        /// </summary>
        /// <value>The Bucketed Delta</value>
        public Decimal LocalCurrencyBucketedDelta1
        {
            get
            {
                return EvaluateBucketedDelta1();
            } 
        }

        /// <summary>
        /// Gets the vector of Bucketed Delta
        /// </summary>
        /// <value>The vector of Bucketed Delta</value>
        public Decimal[] LocalCurrencyBucketedDeltaVector
        {
            get
            {
                return EvaluateBucketedDeltaVector();
            }
        }

        public Decimal[] LocalCurrencyBucketedDeltaVector2
        {
            get
            {
                return EvaluateBucketDelta12();
            }
        }

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public Decimal BreakEvenSpread
        {
            get
            {
                return EvaluateBreakEvenSpread();
            }
        }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        public decimal HistoricalAccrualFactor
        {
            get { return LocalCurrencyHistoricalAccrualFactor * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDelta0
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta0();
                }
                return result;
            }
        }

        ///// <summary>
        ///// Gets the derivative with respect to the forward Rate.
        ///// </summary>
        ///// <value>The delta wrt the fixed rate.</value>
        //public decimal LocalCurrencyDelta0
        //{
        //    get { return Delta0; }
        //}

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta0
        {
            get { return LocalCurrencyDelta0 * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal LocalCurrencyNPV
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateNPV();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The npv.</value>
        public decimal LocalCurrencySimpleCVA
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal LocalCurrencyFloatingNPV
        {
            get
            {
                return EvaluateFloatingNPV();
            }
        }

        ///// <summary>
        ///// Gets the npv.
        ///// </summary>
        ///// <value>The net present value of a floating coupon.</value>
        //public decimal LocalCurrencyFloatingNPV
        //{
        //    get { return FloatingNPV; }
        //}

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        public decimal FloatingNPV
        {
            get { return LocalCurrencyFloatingNPV * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote
        {
            get { return EvaluateMarketQuote(); }
        }

        /// <summary>
        /// Gets the Net Future Valuie of realised cash flows.
        /// </summary>
        /// <value>The NFV.</value>
        public Decimal LocalCurrencyNFV
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateNPV();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The non-discounted expected value.</value>
        public decimal LocalCurrencyExpectedValue
        {
            get
            {
                var result = 0m;
                if (!AnalyticParameters.IsRealised)
                {
                    result = EvaluateExpectedValue();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        public decimal LocalCurrencyHistoricalAccrualFactor
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateAccrualFactor();
                }
                return result;
            }
        }

        ///// <summary>
        ///// Gets the historical accrual factor.
        ///// </summary>
        ///// <value>The historical accrual factor.</value>
        //public decimal LocalCurrencyHistoricalAccrualFactor
        //{
        //    get { return HistoricalAccrualFactor; }
        //}

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal LocalCurrencyHistoricalDelta0
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta0();
                }
                return result;
            }
        }

        ///// <summary>
        ///// Gets the historical derivative with respect to the forward Rate.
        ///// </summary>
        ///// <value>The historical delta wrt the fixed rate.</value>
        //public decimal LocalCurrencyHistoricalDelta0
        //{
        //    get { return HistoricalDelta0; }
        //}

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDelta0
        {
            get { return LocalCurrencyHistoricalDelta0 * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the historical delta wrt the fixed rate R.
        /// </summary>
        public decimal LocalCurrencyHistoricalDeltaR
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDeltaR();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        public decimal LocalCurrencyHistoricalValue
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateExpectedValue();
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote
        {
            get
            {
                return EvaluateBreakEvenRate();
            }
        }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        public decimal LocalCurrencyDelta1
        {
            get { return EvaluateDelta1(); }
        }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDeltaR
        {
            get { return EvaluateDeltaR(); }
        }

        ///// <summary>
        ///// Gets the derivative with respect to the Rate.
        ///// </summary>
        ///// <value>The delta wrt the fixed rate.</value>
        //public decimal LocalCurrencyDeltaR
        //{
        //    get { return DeltaR; }
        //}

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR
        {
            get { return LocalCurrencyDeltaR * EvaluateReportingCurrencyFxRate(); }
        }

        /// <summary>
        /// Gets the derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The fistorical delta1.</value>
        public decimal LocalCurrencyHistoricalDelta1
        {
            get
            {
                var result = 0m;
                if (AnalyticParameters.IsRealised)
                {
                    result = EvaluateDelta1();
                }
                return result;
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
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateNPV()
        {
            return EvaluateExpectedValue() * AnalyticParameters.PaymentDiscountFactor;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        virtual protected Decimal EvaluateExpectedValue()
        {
            return AnalyticParameters.NotionalAmount;
        }

        /// <summary>
        /// Evaluates the vector of bucketed delta
        /// </summary>
        /// <returns></returns>
        virtual protected Decimal[] EvaluateBucketedDeltaVector()
        {
            Decimal[] temp = { 0.0m };
            return temp;
        }

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns>The break even rate</returns>
        virtual protected Decimal EvaluateBreakEvenRate()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        virtual protected Decimal EvaluateAccrualFactor()
        {     
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaR()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta0()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDelta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * AnalyticParameters.BuckettingRate;
            return EvaluateNPV() * -AnalyticParameters.CurveYearFraction / (1 + temp) / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaCCR()
        {
            return EvaluateNPV() * -AnalyticParameters.CurveYearFraction / 10000;
        }

        /// <summary>
        /// Evaluates the rate.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateMarketQuote()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the break even spread.
        /// </summary>
        /// <returns>The break even spread</returns>
        protected virtual Decimal EvaluateBreakEvenSpread()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        virtual protected Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.PaymentDiscountFactor;
        }

        /// <summary>
        /// Find the index of this coupon year fraction in Array of bucketed year fractions
        /// </summary>
        /// <returns></returns>
        protected int FindYearFrationIndex()
        {
            var time = (double)(AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear);
            var cycles = Convert.ToInt32(time);

            return cycles;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal[] EvaluateBucketDelta12()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * AnalyticParameters.BuckettingRate;
            var time = (AnalyticParameters.CurveYearFraction / AnalyticParameters.PeriodAsTimesPerYear);
            var cycles = Convert.ToInt32(Math.Floor(time));
            var remainder = AnalyticParameters.CurveYearFraction - cycles * AnalyticParameters.PeriodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -EvaluateNPV() * AnalyticParameters.PeriodAsTimesPerYear / (1 + temp) / 10000;
            }
            var tailValue = remainder * AnalyticParameters.BuckettingRate;
            result[result.Length - 1] = -EvaluateNPV() * remainder / (1 + tailValue) / 10000;
            return result;
        }

        /// <summary>
        /// Evaluating Bucketed Delta
        /// </summary>
        /// <returns>The bucketed delta</returns>
        protected virtual Decimal EvaluateBucketedDelta1()
        {
            return EvaluateDelta1();
        }

        /// <summary>
        /// Evaluating the floating NPV.
        /// </summary>
        /// <returns>The NPV</returns>
        protected virtual Decimal EvaluateFloatingNPV()
        {
            return 0.0m;
        }

        /// <summary>
        /// Evaluating the fx rate.
        /// </summary>
        /// <returns>The NPV</returns>
        protected Decimal EvaluateReportingCurrencyFxRate()
        {
            var result = 1.0m;
            if (AnalyticParameters != null)
            {
                if (AnalyticParameters.Currency != AnalyticParameters.ReportingCurrency)
                {
                    result = AnalyticParameters.ToReportingCurrencyRate;
                }

            }
            return result;
        }
        
    }
}