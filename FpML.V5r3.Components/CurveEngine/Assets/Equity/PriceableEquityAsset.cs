#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Models.Equity;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets.Equity
{
    /// <summary>
    /// PriceableBondAsset
    /// </summary>
    public abstract class PriceableEquityAsset : PriceableEquityAssetController
    {
        /// <summary>
        /// 
        /// </summary>
        protected bool IsBuilt { get; set; }

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        ///<summary>
        ///</summary>
        public string ModelIdentifier { get; set; }

        ///<summary>
        ///</summary>
        public string Description { get; set; }

        /// <summary>
        /// THe equity valuation curve.
        /// </summary>
        public string DivdendCurveName { get; set; }

        /// <summary>
        /// The coupon rate. If this is a floater it would be the last reset rate.
        /// </summary>
        public decimal DividendYield { get; set; }

        /// <summary>
        /// The issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The instrument identifiers.
        /// </summary>
        public List<InstrumentId> InstrumentIds { get; set; }

        /// <summary>
        /// The clearance system.
        /// </summary>
        public string ClearanceSystem { get; set; }

        /// <summary>
        /// The exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The coupon frequency.
        /// </summary>
        public Period DividendFrequency { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableEquityAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="settlementCalendar">Settlement calendar</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="position">The notional amount.</param>
        /// <param name="settlementDateOffset">The details to calculate the settlement date.</param>
        protected PriceableEquityAsset(DateTime baseDate, Decimal position, RelativeDateOffset settlementDateOffset, 
            IBusinessCalendar settlementCalendar, BasicQuotation marketQuote)
        {
            ModelIdentifier = "GenericEquityAsset";
            Multiplier = 1.0m;
            Notional = position;
            BaseDate = baseDate;
            SettlementDateOffset = settlementDateOffset;
            SettlementDate = GetSettlementDate(baseDate, settlementCalendar, SettlementDateOffset);
            //ExDividendDateOffset = exDivDateOffset;
            SetQuote(marketQuote);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new EquityAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            //
            var bEvalIndexAtMaturity = false;
            if (metrics.Contains(EquityMetrics.IndexAtMaturity))
            {
                bEvalIndexAtMaturity = true;
                // Remove all metrics except DFAM
                //
                metrics.RemoveAll(metricItem => metricItem != EquityMetrics.IndexAtMaturity);
            }
            var metricsToEvaluate = metrics.ToArray();
            IEquityAssetParameters analyticModelParameters = new EquityAssetParameters();
            CalculationResults = new EquityAssetResults();
            var marketEnvironment = modelData.MarketEnvironment;
            IEquityCurve equitycurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                equitycurve = (IEquityCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                equitycurve = (IEquityCurve)modelData.MarketEnvironment.GetPricingStructure(EquityCurveName);              
            }
            if (equitycurve != null)
            {
                //1. Get the current equity value from the curve.
                var equityValue = GetEquityFactor(equitycurve, modelData.ValuationDate, SettlementDate);
                //2. Set the market rate
                analyticModelParameters.Multiplier = Multiplier;
                analyticModelParameters.Quote = QuoteValue;
                analyticModelParameters.EquityPrice = equityValue;
                analyticModelParameters.NotionalAmount = Notional;
                //3. Set the anaytic input parameters and Calculate the respective metrics 
                AnalyticModelParameters = analyticModelParameters;
                CalculationResults = AnalyticsModel.Calculate<IEquityAssetResults, EquityAssetResults>(analyticModelParameters, metricsToEvaluate);
                if (bEvalIndexAtMaturity)
                {
                    //4. Set the anaytic input parameters and Calculate the respective metrics
                    //                   
                    IndexAtMaturity = CalculationResults.IndexAtMaturity;
                }              
                return GetValue(CalculationResults);
            }
            return null;
        }

        /// <summary>
        /// Gets the equity factor.
        /// </summary>
        /// <param name="equityCurve">The equity curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetEquityFactor(IEquityCurve equityCurve,
                                         DateTime valuationDate, DateTime targetDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)equityCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="marketQuote">The marketQuote.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (String.Compare(marketQuote.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Quote = marketQuote;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public BasicQuotation Quote
        {
            get => MarketQuote;
            set
            {
                MarketQuote = value;
                if (value.quoteUnits.Value == "Price")
                {
                    QuoteValue = value.value;
                }
            }
        }

        ///<summary>
        ///</summary>
        public override DateTime GetRiskMaturityDate()
        {
            return SettlementDate;
        }

        ///<summary>
        ///</summary>
        public DateTime GetSettlementDate(DateTime baseDate, IBusinessCalendar settlementCalendar, RelativeDateOffset settlementDateOffset)
        {
            try
            {
                return settlementCalendar.Advance(BaseDate, settlementDateOffset, settlementDateOffset.businessDayConvention);
            }
            catch (System.Exception)
            {
                throw new System.Exception("No settlement calendar set.");
            }
           
        }

        ///<summary>
        ///</summary>
        public override EquityAsset GetEquity()
        {
            var equity = new EquityAsset
            {
                currency = new IdentifiedCurrency { Value = Currency.Value, id = "CouponCurrency" },
                definition = null,//TODO not currrently handled
                description = Description,//TODO not currrently handled
                id = Id
            };
            if (InstrumentIds != null)
            {
                equity.instrumentId = InstrumentIds.ToArray();
            }
            if (Exchange != null)
            {
                equity.exchangeId = ExchangeIdHelper.Parse(Exchange);
            }
            if (ClearanceSystem != null)
            {
                equity.clearanceSystem = ClearanceSystemHelper.Parse(ClearanceSystem);
            }
            return equity;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public Boolean IsExDiv()
        {
            bool result = SettlementDate >= NextExDivDate;
            return result;
        }
    }
}