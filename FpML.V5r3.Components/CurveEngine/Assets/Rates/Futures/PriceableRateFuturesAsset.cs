#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Models.Rates.Futures;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;
using Orion.CurveEngine.Helpers;
using Orion.CalendarEngine;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.Markets;
using Orion.Models.Assets;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public class PriceableRateFuturesAsset : PriceableRateSpreadAssetController, IPriceableClearedRateAssetController, IPriceableFuturesAssetController
    {
        ///<summary>
        ///</summary>
        ///<typeparam name="TR"></typeparam>
        public delegate TR DelegateToCalculateMethod<out TR>();

        /// <summary>
        /// Do the futures expiries roll to the main cycle or not.
        /// </summary>
        public bool MainCycle { get; protected set; }

        /// <summary>
        /// Is the last trading date the risk maturity OR
        /// is the last trading date the beginning of a forward period
        /// and the rick maturity date is at the end of this interval.
        /// </summary>
        public bool IsBackwardLooking { get; protected set; }

        /// <summary>
        /// The exchange code for the contract.
        /// </summary>
        public string Code { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LastTradeDate { get; set; }

        /// <summary>
        /// Returns the futures expiry or the option expiry date.
        /// </summary>
        /// <returns></returns>
        public DateTime GetBootstrapDate() => LastTradeDate;

        /// <summary>
        /// 
        /// </summary>
        public Future Future { get; protected set; }

        /// <summary>
        /// Gets the exchange initial margin.
        /// </summary>
        /// <value>The exchange initial margin.</value>
        public decimal InitialMargin => 0.0m;

        /// <summary>
        /// Gets the last settled value.
        /// </summary>
        /// <value>The last settled value.</value>
        public decimal LastSettledValue => 0.0m;

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>The current value.</value>
        public decimal IndexAtMaturity => MarketQuote.value;


        private IModelAnalytic<IRateFuturesAssetParameters, RateMetrics> _analyticsModel;

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IRateFuturesAssetParameters, RateMetrics> AnalyticsModel 
        {
            get 
            {
                if (_analyticsModel != null) return _analyticsModel;
                switch (ModelIdentifier)
                {
                    case RateFutureAssetAnalyticModelIdentifier.ZB:
                        _analyticsModel = new NZDFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.IR:
                        _analyticsModel = new BankBillsFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.IB:
                        _analyticsModel = new CashFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.ED:
                    case RateFutureAssetAnalyticModelIdentifier.RA:
                    case RateFutureAssetAnalyticModelIdentifier.BAX:
                        _analyticsModel = new EuroDollarFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.ER:
                        _analyticsModel = new EuroFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.L:
                        _analyticsModel = new EuroSterlingFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.ES:
                        _analyticsModel = new EuroSwissFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.EY:
                        _analyticsModel = new EuroYenFuturesAssetAnalytic();
                        break;
                    case RateFutureAssetAnalyticModelIdentifier.HR:
                        _analyticsModel = new EuroHKDFuturesAssetAnalytic();
                        break;
                }
                return _analyticsModel;
            }
            set => _analyticsModel = value;
        }

        /// <summary>
        /// 
        /// </summary>
        protected RateFutureAssetAnalyticModelIdentifier ModelIdentifier;

        /// <summary>
        /// 
        /// </summary>
        public IRateAssetResults AnalyticResults { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Gets the day counter.
        /// </summary>
        /// <value>The day counter.</value>
        public IDayCounter DayCounter { get; set; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets the time to expiry.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToExpiry()
        {
            return TimeToExpiry;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Volatility { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Strike { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FuturesLag { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateFuturesAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The number of contracts.</param>
        /// <param name="nodeStruct"></param>
        /// <param name="rollCalendar">The payment calender.</param>
        /// <param name="marketQuote">In the case of a future, this is a rate.</param>
        /// <param name="extraQuote">In the case of a future, this is the futures convexity volatility.</param>
        public PriceableRateFuturesAsset(DateTime baseDate, int position, FutureNodeStruct nodeStruct, IBusinessCalendar rollCalendar,
                                         BasicQuotation marketQuote, decimal extraQuote)
        {
            MainCycle = nodeStruct.MainCycle;
            IsBackwardLooking = nodeStruct.IsBackwardLooking;
            Id = nodeStruct.Future.id;
            Position = position;
            BaseDate = baseDate;
            SetQuote(marketQuote);
            //This handles the case where the underlying index is pat of an IR future node type.
            if (nodeStruct is IRFutureNodeStruct isIRFuture)
            {
                UnderlyingRateIndex = isIRFuture.RateIndex;
                Volatility = extraQuote;
            }
            BusinessDayAdjustments = nodeStruct.BusinessDayAdjustments;            
            FuturesLag = nodeStruct.SpotDate;
            Future = nodeStruct.Future;
            var exchange = nodeStruct.Future.exchangeId.Value;
            var idParts = Id.Split('-');
            var exchangeCommodityName = idParts[2];
            Enum.TryParse(exchangeCommodityName, out ModelIdentifier);
            Code = idParts[3];
            //Catch the relative rolls.
            if (int.TryParse(Code, out int intResult))
            {
                var tempTradingDate = LastTradingDayHelper.ParseCode(exchangeCommodityName);
                Code = MainCycle ? tempTradingDate.GetNthMainCycleCode(baseDate, intResult) : tempTradingDate.GetNthCode(baseDate, intResult);
            }
            BusinessDayConventionEnum convention = BusinessDayAdjustments.businessDayConvention;
            LastTradeDate = LastTradingDayHelper.GetLastTradingDay(baseDate, exchangeCommodityName, Code);
            DayCounter = DayCounterHelper.Parse(UnderlyingRateIndex.dayCountFraction.Value);
            Offset offset = OffsetHelper.FromInterval(UnderlyingRateIndex.term, DayTypeEnum.Calendar);
            //For backward looking contracts like the IB and Fed funds and other SFR based contracts
            if (IsBackwardLooking)
            {
                //Use baseDate rather than the start of the contract date to avoid the reset issue.
                AdjustedStartDate = BaseDate;
                TimeToExpiry = (decimal)DayCounter.YearFraction(BaseDate, LastTradeDate);
                RiskMaturityDate = rollCalendar.Advance(LastTradeDate, FuturesLag, convention);
                YearFraction = (decimal)DayCounter.YearFraction(AdjustedStartDate, RiskMaturityDate);
            }
            else
            {
                AdjustedStartDate = ((BaseCalendar)rollCalendar).Advance(LastTradeDate, FuturesLag, convention);
                TimeToExpiry = (decimal)DayCounter.YearFraction(BaseDate, LastTradeDate);
                RiskMaturityDate = rollCalendar.Advance(AdjustedStartDate, offset, convention);
                YearFraction = (decimal)DayCounter.YearFraction(AdjustedStartDate, RiskMaturityDate);
            }
            CurveName = CurveNameHelpers.GetExchangeTradedCurveName(idParts[0], exchange, exchangeCommodityName);
        }

        /// <summary>
        /// Sets the market quote, which is a rate for a future and a lognormal volatility for an option.
        /// </summary>
        /// <param name="marketQuote">The fixed rate.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (string.Compare(marketQuote.measureType.Value, PriceableSimpleRateAsset.RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                marketQuote.measureType.Value = PriceableSimpleRateAsset.RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", PriceableSimpleRateAsset.RateQuotationType);
            }
            MarketQuote = marketQuote.measureType.Value == PriceableSimpleRateAsset.RateQuotationType
                             ? marketQuote
                             : null;
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so that is all we evaluate - every other metric is ignored
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(RateMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                //  remove all except DiscountFactorAtMaturity metric
                //
                metrics.RemoveAll(metricItem => metricItem != RateMetrics.DiscountFactorAtMaturity);
            }
            var metricsToEvaluate = metrics.ToArray();
            var analyticModelParameters = new RateFuturesAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry
            };
            var marketEnvironment = modelData.MarketEnvironment;
            IPricingStructure curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = ((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                curve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                curve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = modelData.MarketEnvironment.GetPricingStructure(CurveName);
            } 
            //2. TODO Add vol Surfaces...
            analyticModelParameters.Volatility = Volatility;
            //3. Get the start discount factor
            StartDiscountFactor = GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
            analyticModelParameters.StartDiscountFactor = StartDiscountFactor;
            //4. Get the Rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            if (bEvalDiscountFactorAtMaturity)
            {
                //5. Set the analytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
                EndDiscountFactor = DiscountFactorAtMaturity;
            }
            else
            {
                //4. Get the end discount factor
                EndDiscountFactor = GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                analyticModelParameters.EndDiscountFactor = EndDiscountFactor;
                //5. Get the TimeToExpiry
                analyticModelParameters.TimeToExpiry = GetTimeToExpiry();
                //6. Get the position
                analyticModelParameters.Position = Position;
                //7. Set the analytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            var analyticModelParameters = new RateFuturesAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      AdjustedStartDate,
                                      BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.DiscountFactorAtMaturity });
            return AnalyticResults.DiscountFactorAtMaturity;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            var analyticModelParameters = new RateFuturesAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                EndDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      GetRiskMaturityDate(), BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote + Spread.value;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            var analyticModelParameters = new RateFuturesAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                EndDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      GetRiskMaturityDate(), BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        #region IPriceableClearedRateAssetController

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <param name="discountedSpace">The OIS Space. Not used for margin futures.</param>
        /// <returns></returns>
        public virtual decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace, IInterpolatedSpace discountedSpace)
        {
            var analyticModelParameters = new RateFuturesAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                EndDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      GetRiskMaturityDate(), BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        #endregion

        #region Implementation of IPriceableSpreadAssetController
        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public override IList<decimal> Values => new[] { StartDiscountFactor, DiscountFactorAtMaturity };

        /// <summary>
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public override decimal ValueAtMaturity => DiscountFactorAtMaturity;

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override IList<DateTime> GetRiskDates()
        {
            return new[] { AdjustedStartDate, RiskMaturityDate };
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the market quote of the asset.</returns>
        public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return MarketQuote.value - CalculateImpliedQuote(interpolatedSpace);
        }

        #endregion

    }
}