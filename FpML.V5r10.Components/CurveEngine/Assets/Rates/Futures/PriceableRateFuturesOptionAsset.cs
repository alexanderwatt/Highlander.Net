#region Using directives

using System;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Rates.Futures;
using FpML.V5r10.Reporting.Models.Rates.FuturesOptions;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.Assets.ExchangeTraded;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.Assets.Rates.Futures
{
    ///<summary>
    ///</summary>
    public class PriceableRateFuturesOptionAsset : PriceableFuturesOptionAssetController, IPriceableRateOptionAssetController
    {
        #region Properties

        private IModelAnalytic<IRateFuturesOptionAssetParameters, RateFuturesOptionMetrics> _analyticsModel;

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IRateFuturesOptionAssetParameters, RateFuturesOptionMetrics> AnalyticsModel
        {
            get
            {
                if (_analyticsModel != null) return _analyticsModel;
                switch (ModelIdentifier)
                {
                    case RateFutureAssetAnalyticModelIdentifier.IR:
                        _analyticsModel = new BankBillsFuturesOptionAssetAnalytic();
                        break;
                }
                return _analyticsModel;
            }
            set => _analyticsModel = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected RateFutureAssetAnalyticModelIdentifier ModelIdentifier;

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

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateFuturesOptionAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The number of contracts.</param>
        /// <param name="isPut">Is the option a put?</param>
        /// <param name="nodeStruct">The instrument</param>
        /// <param name="rollCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">In the case of a future, this is a rate.</param>
        /// <param name="extraQuote">In the case of a future, this is the futures convexity volatility.</param>
        public PriceableRateFuturesOptionAsset(DateTime baseDate, int position, bool isPut, IRFutureOptionNodeStruct nodeStruct,
            IBusinessCalendar rollCalendar, BasicQuotation marketQuote, Decimal extraQuote) : 
            base(baseDate, position, isPut, nodeStruct, rollCalendar, marketQuote, extraQuote)
        {
            UnderlyingRateIndex = nodeStruct.RateIndex;
            Strike = extraQuote;
            var exchange = nodeStruct.Future.exchangeId.Value;
            var idParts = Id.Split('-');
            var exchangeCommodityName = idParts[2];
            Enum.TryParse(exchangeCommodityName, out ModelIdentifier);
            var immCode = idParts[3];
            //Catch the relative rolls.
            if (int.TryParse(immCode, out int intResult))
            {
                var tempTradingDate = LastTradingDayHelper.ParseCode(exchangeCommodityName);
                immCode = tempTradingDate.GetNthMainCycleCode(baseDate, intResult);
            }
            BusinessDayConventionEnum convention = BusinessDayAdjustments.businessDayConvention;
            LastTradeDate = LastTradingDayHelper.GetLastTradingDay(baseDate, exchangeCommodityName, immCode);
            OptionsExpiryDate = ((BaseCalendar)rollCalendar).Advance(LastTradeDate, ExpiryLag);
            TimeToExpiry = (decimal)Actual365.Instance.YearFraction(BaseDate, OptionsExpiryDate);
            AdjustedStartDate = ((BaseCalendar)rollCalendar).Advance(LastTradeDate, FuturesLag);
            DayCounter = DayCounterHelper.Parse(UnderlyingRateIndex.dayCountFraction.Value);            
            Offset offset = OffsetHelper.FromInterval(UnderlyingRateIndex.term, DayTypeEnum.Calendar);
            RiskMaturityDate = rollCalendar.Advance(AdjustedStartDate, offset, convention);
            YearFraction = (decimal)DayCounter.YearFraction(AdjustedStartDate, RiskMaturityDate);
            CurveName = CurveNameHelpers.GetExchangeTradedCurveName(idParts[0], exchange, exchangeCommodityName);
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
            // Determine if VolatilityAtExpiry has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalVolatilityAtExpiry = false;
            if (metrics.Contains(RateFuturesOptionMetrics.VolatilityAtExpiry))
            {
                bEvalVolatilityAtExpiry = true;
                //  remove all except VolatilityAtExpiry metric
                //
                metrics.RemoveAll(metricItem => metricItem != RateFuturesOptionMetrics.VolatilityAtExpiry);
            }
            var metricsToEvaluate = metrics.ToArray();
            var analyticModelParameters = new RateFuturesOptionAssetParameters
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
            if (marketEnvironment.GetType() == typeof(MarketEnvironment) && CurveName != null)
            {
                curve = modelData.MarketEnvironment.GetPricingStructure(CurveName);
            } 
            //2. TODO Add vol Surfaces...
            analyticModelParameters.Volatility = Volatility;
            analyticModelParameters.IsVolatilityQuote = IsVolatilityQuote;
            //3. Get the start discount factor
            StartDiscountFactor = GetIndex(curve, AdjustedStartDate, modelData.ValuationDate);
            analyticModelParameters.StartDiscountFactor = StartDiscountFactor;
            analyticModelParameters.YearFraction = YearFraction;
            analyticModelParameters.IsPut = IsPut;
            //4. Get the Rate
            if (curve is IExchangeTradedCurve forecastCurve)
            {
                analyticModelParameters.Rate = GetIndex(forecastCurve, GetExpiryDate(), modelData.ValuationDate);
            }
            else
            {
                analyticModelParameters.EndDiscountFactor = GetIndex(curve, RiskMaturityDate, modelData.ValuationDate);
                if (Math.Abs(YearFraction) > 0)
                {
                    var rate = (StartDiscountFactor / analyticModelParameters.EndDiscountFactor - 1) / YearFraction;
                    analyticModelParameters.Rate = rate;
                }
            }
            if (bEvalVolatilityAtExpiry)
            {
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateFuturesOptionAssetResults, RateFuturesOptionAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            else
            {
                //5. Get the TimeToExpiry
                analyticModelParameters.TimeToExpiry = GetTimeToExpiry();
                //6. Get the position
                analyticModelParameters.NumberOfContracts = Position;
                //7. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateFuturesOptionAssetResults, RateFuturesOptionAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public decimal CalculateVolatility(IInterpolatedSpace interpolatedSpace)
        {
            var analyticModelParameters = new RateFuturesOptionAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetIndex(interpolatedSpace,
                                      AdjustedStartDate,
                                      BaseDate),
                Volatility = Volatility
            };
            StartDiscountFactor = GetIndex(interpolatedSpace, AdjustedStartDate, BaseDate);
            analyticModelParameters.StartDiscountFactor = StartDiscountFactor;
            analyticModelParameters.YearFraction = YearFraction;
            //4. Get the Rate
            //TODO This needs to replaced with a XIBOR calculation
            if (interpolatedSpace is IExchangeTradedCurve forecastCurve)
            {
                analyticModelParameters.Rate = GetIndex(forecastCurve, GetExpiryDate(), BaseDate);
            }
            else
            {
                analyticModelParameters.EndDiscountFactor = GetIndex(interpolatedSpace, RiskMaturityDate, BaseDate);
                if (Math.Abs(YearFraction) > 0)
                {
                    var rate = (StartDiscountFactor / analyticModelParameters.EndDiscountFactor - 1) / YearFraction;
                    analyticModelParameters.Rate = rate;
                }
            }
            AnalyticResults = new RateFuturesOptionAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateFuturesOptionAssetResults, RateFuturesOptionAssetResults>(analyticModelParameters, new[] { RateFuturesOptionMetrics.VolatilityAtExpiry });
            var analyticResults = (IRateFuturesOptionAssetResults) AnalyticResults;
            if (analyticResults?.OptionVolatility != null)
            {
                return (decimal) analyticResults?.OptionVolatility;
            }
            return 0.0m;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            var analyticModelParameters = new RateFuturesOptionAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetIndex(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            StartDiscountFactor = analyticModelParameters.StartDiscountFactor;
            analyticModelParameters.YearFraction = YearFraction;
            //4. Get the Rate
            if (interpolatedSpace is IExchangeTradedCurve forecastCurve)
            {
                analyticModelParameters.Rate = GetIndex(forecastCurve, GetExpiryDate(), BaseDate);
            }
            else
            {
                analyticModelParameters.EndDiscountFactor = GetIndex(interpolatedSpace, RiskMaturityDate, BaseDate);
                if (Math.Abs(YearFraction) > 0)
                {
                    var rate = (StartDiscountFactor / analyticModelParameters.EndDiscountFactor - 1) / YearFraction;
                    analyticModelParameters.Rate = rate;
                }
            }
            AnalyticResults = new RateFuturesOptionAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateFuturesOptionAssetResults, RateFuturesOptionAssetResults>(analyticModelParameters, new[] { RateFuturesOptionMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return OptionsExpiryDate;
        }

        /// <summary>
        /// Gets the year fraction to maturity.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToExpiry(DateTime baseDate, DateTime expiryDate)
        {
            return (decimal)DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED).YearFraction(baseDate, expiryDate);
        }

        #region IPriceableClearedRateAssetController

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <param name="discountedSpace">The OIS Space. Not used for margined futures.</param>
        /// <returns></returns>
        public Decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace, IInterpolatedSpace discountedSpace)
        {
            var analyticModelParameters = new RateFuturesOptionAssetParameters
            {
                YearFraction = YearFraction,
                TimeToExpiry = TimeToExpiry,
                StartDiscountFactor =
                    GetIndex(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                Volatility = Volatility
            };
            //3. Get the Rate
            //
            StartDiscountFactor = analyticModelParameters.StartDiscountFactor;
            analyticModelParameters.YearFraction = YearFraction;
            //4. Get the Rate
            if (interpolatedSpace is IExchangeTradedCurve forecastCurve)
            {
                analyticModelParameters.Rate = GetIndex(forecastCurve, GetExpiryDate(), BaseDate);
            }
            else
            {
                analyticModelParameters.EndDiscountFactor = GetIndex(interpolatedSpace, RiskMaturityDate, BaseDate);
                if (Math.Abs(YearFraction) > 0)
                {
                    var rate = (StartDiscountFactor / analyticModelParameters.EndDiscountFactor - 1) / YearFraction;
                    analyticModelParameters.Rate = rate;
                }
            }
            AnalyticResults = new RateFuturesOptionAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateFuturesOptionAssetResults, RateFuturesOptionAssetResults>(analyticModelParameters, new[] { RateFuturesOptionMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        #endregion

    }
}