#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Instruments.ForeignExchange;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap;
using Orion.Analytics.Schedulers;
using Orion.Analytics.Utilities;
using Orion.Constants;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    public class FxSwapPricer : InstrumentControllerBase, IPriceableFxSwap<IFxSwapParameters, IFxSwapInstrumentResults>, IPriceableInstrumentController<FxSwap>
    {
        // Analytics
        public IModelAnalytic<IFxSwapParameters, FxSwapInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "FxSwap";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public string DiscountCurveName1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DiscountCurveName2 { get; set; }

        // BucketedCouponDates
        //protected Period BucketingInterval;
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        //protected DateTime[] BucketingDates = { };

        /// <summary>
        /// Gets the payments in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        public IList<InstrumentControllerBase> Currency1Payments { get; set; }

        /// <summary>
        /// Gets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        public IList<InstrumentControllerBase> Currency2Payments { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IFxSwapParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        public IFxSwapInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceableFxSwapLeg> Legs = new List<PriceableFxSwapLeg>();

        /// <summary>
        /// 
        /// </summary>
        public FxSwapPricer()
        {
            Multiplier = 1.0m;
        }

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="fxSwapFpML"></param>
        /// <param name="basePartyReference"></param>
        public FxSwapPricer(FxSwap fxSwapFpML, string basePartyReference)
        {
            //BusinessCentersResolver.ResolveBusinessCenters(swapFpML);
            Multiplier = 1.0m;
            Id = fxSwapFpML.id;
            OrderedPartyNames = new List<string>();
            //We make the assumption that the termination date is the same for all legs..
            var lastDate = new DateTime();
            ProductType = ProductTypeSimpleEnum.FxSwap;
            PaymentCurrencies = new List<string>();
            var tempDate = new DateTime();
            var fxSwapStream = fxSwapFpML.nearLeg;
            if (fxSwapStream != null)
            {
                //Set the id of the first stream.
                fxSwapStream.id = fxSwapStream.id + "_" + "nearLeg";
                var leg = new PriceableFxSwapLeg(fxSwapStream, basePartyReference, ProductTypeSimpleEnum.FxSwap);
                Legs.Add(leg);
                //Add the currencies for the trade pricer.
                if (!PaymentCurrencies.Contains(leg.Currency1.Value))
                {
                    PaymentCurrencies.Add(leg.Currency1.Value);
                }
                if(!PaymentCurrencies.Contains(leg.Currency2.Value))
                {
                    PaymentCurrencies.Add(leg.Currency2.Value);
                }
                //find the last date.
                tempDate = leg.LastDate();
                //Add the payments
                Currency1Payments = new List<InstrumentControllerBase>();
                Currency2Payments = new List<InstrumentControllerBase>();
                Currency1Payments.Add(leg.Currency1Payment);
                Currency2Payments.Add(leg.Currency2Payment);
            }
            fxSwapStream = fxSwapFpML.farLeg;
            if (fxSwapStream != null)
            {
                //Set the id of the first stream.
                fxSwapStream.id = fxSwapStream.id + "_" + "farLeg";
                var leg = new PriceableFxSwapLeg(fxSwapStream, basePartyReference, ProductTypeSimpleEnum.FxSwap);
                Legs.Add(leg);
                //Add the currencies for the trade pricer.
                if (!PaymentCurrencies.Contains(leg.Currency1.Value))
                {
                    PaymentCurrencies.Add(leg.Currency1.Value);
                }
                if (!PaymentCurrencies.Contains(leg.Currency2.Value))
                {
                    PaymentCurrencies.Add(leg.Currency2.Value);
                }
                //Add the payments
                Currency1Payments = new List<InstrumentControllerBase>();
                Currency2Payments = new List<InstrumentControllerBase>();
                Currency1Payments.Add(leg.Currency1Payment);
                Currency2Payments.Add(leg.Currency2Payment);
            }
            if (lastDate < tempDate)
            {
                lastDate = tempDate;
            }
            RiskMaturityDate = lastDate;
        }

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public FxSwap Build()
        {
            var fxSwap = new FxSwap();
            var leg1 = Legs[0].Build();
            var leg2 = Legs[1].Build();
            fxSwap.nearLeg = leg1;
            fxSwap.farLeg = leg2;
            fxSwap.id = Id;
            fxSwap.Items = new object[] { ProductTypeHelper.Create(ProductTypeSimpleEnum.FxSwap.ToString()) };
            fxSwap.ItemsElementName = new[] { ItemsChoiceType2.productType };
            return fxSwap;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = new List<InstrumentControllerBase>(Legs.ToArray());

            return children;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetLegs()
        {
            var children = new List<InstrumentControllerBase>(Legs.ToArray());

            return children;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<IPriceableInstrumentController<InterestRateStream>> GetInstumentControllers()
        {
            return GetLegs().Cast<IPriceableInstrumentController<InterestRateStream>>().ToList();
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public List<IPriceableInstrumentController<Payment>> GetPaymentControllers()
        {
            return Legs.SelectMany(controller => controller.Payments).Cast<IPriceableInstrumentController<Payment>>().ToList();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected virtual Decimal AggregatePaymentMetric(InstrumentMetrics metric)
        {
            string[] metrics = { metric.ToString() };
            var childValuations = GetChildValuations(GetPaymentControllers().ToArray(), new List<string>(metrics), ModelData);

            var results = new List<decimal>();

            if (childValuations != null)
            {
                results.AddRange(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))));
            }
            decimal result = Aggregator.SumDecimals(results.ToArray());

            return result;
        }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="currency1CurveName">New name of the currecny1 discount curve.</param>
        /// <param name="currency2CurveName">New name of the currecny2 discount curve.</param>
        public void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName)
        {
            DiscountCurveName1 = currency1CurveName;
            DiscountCurveName2 = currency2CurveName;
        }

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate));

            return bucketDates.ToArray();
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            CalculationResults = null;
            UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precendence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new FxSwapAnalytic();
            }
            var swapControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation swapValuation;
            //Sets the evolution type for calculations.
            foreach (var leg in Legs)
            {
                leg.PricingStructureEvolutionType = PricingStructureEvolutionType;
                leg.BucketedDates = BucketedDates;
                leg.Multiplier = Multiplier;
            }
            var legControllers = new List<InstrumentControllerBase>(GetLegs());
            //The assetValuation list.
            var childValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                childValuations.AddRange(legControllers.Select(leg => leg.Calculate(modelData)));
            }
            else
            {
                childValuations = EvaluateChildMetrics(legControllers, modelData, Metrics);
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            childControllerValuations.id = Id + ".FxLeg";
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (swapControllerMetrics.Count > 0)
            {
                IFxSwapParameters analyticModelParameters = new FxSwapParameters
                {
                    Multiplier = Multiplier,
                    //AccrualFactor = AggregateCouponMetric(InstrumentMetrics.AccrualFactor),
                    //FloatingNPV = AggregateCouponMetric(InstrumentMetrics.FloatingNPV),
                    //NPV = AggregateCouponMetric(InstrumentMetrics.NPV)
                };
                CalculationResults = AnalyticsModel.Calculate<IFxSwapInstrumentResults, FxSwapInstrumentResults>(analyticModelParameters, swapControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var swapControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                swapValuation = AssetValuationHelper.UpdateValuation(swapControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(swapControllerMetrics), new List<string>(Metrics));
            }
            else
            {
                swapValuation = childControllerValuations;
            }
            CalculationPerfomedIndicator = true;
            swapValuation.id = Id;
            return swapValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        /// <summary>
        /// Builds a fx swap.
        /// </summary>
        /// <returns></returns>
        public static FxSwap Parse(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
            DateTime startValueDate, DateTime forwardValueDate, Decimal startRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            var fxSwap = new FxSwap
                {
                    Items = new object[] {ProductTypeHelper.Create(ProductTypeSimpleEnum.FxSwap.ToString())},
                    ItemsElementName = new[] {ItemsChoiceType2.productType}
                };
            var leg1 = ParseSpot(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, startValueDate, startRate);
            var leg2 = PriceableFxSwapLeg.ParseForward(exchangeCurrency2PayPartyReference, exchangeCurrency1PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, forwardValueDate, forwardRate, forwardRate, forwardPoints);
            fxSwap.nearLeg = leg1;
            fxSwap.farLeg = leg2 ;
            return fxSwap;
        }

        public static Trade CreateFxSwap(string tradeId, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
            DateTime startValueDate, DateTime forwardValueDate, Decimal startRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            var trade = new Trade {id = tradeId, tradeHeader = new TradeHeader()};
            var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
            var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate {Value = tradeDate};
            var fxSwap = Parse(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints);
            FpMLFieldResolver.TradeSetFxSwap(trade, fxSwap);
            return trade;
        }


        /// <summary>
        /// Build the fx leg
        /// </summary>
        /// <param name="exchangeCurrency1PayPartyReference"></param>
        /// <param name="exchangeCurrency2PayPartyReference"></param>
        /// <param name="exchangeCurrency1Amount"></param>
        /// <param name="exchangeCurrency1"></param>
        /// <param name="exchangeCurrency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="spotRate"></param>
        /// <returns></returns>
        public static FxSwapLeg ParseSpot(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference, decimal exchangeCurrency1Amount,
                                          string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis, DateTime valueDate, Decimal spotRate)
        {
            var fxLeg = FxSwapLeg.CreateSpot(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
                                             exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate);
            return fxLeg;
        }

        public static Trade CreateFxSwap(string tradeId, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
                                        decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
                                        DateTime valueDate, Decimal spotRate, Decimal? forwardRate, Decimal? forwardPoints)
        {
            var trade = new Trade { id = tradeId, tradeHeader = new TradeHeader() };
            var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
            var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
            var nearLeg = new FxSwapLeg();
            var farLeg = new FxSwapLeg(); 
            if (forwardRate == null)
            {
                nearLeg = ParseSpot(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
                                  exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate);
            }
            else
            {
                farLeg = PriceableFxSwapLeg.ParseForward(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
                                                        exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, (decimal)forwardRate, forwardPoints);
            }
            var fxSwap = new FxSwap
                {
                    nearLeg = nearLeg,
                    farLeg = farLeg,
                    Items = new object[] {ProductTypeHelper.Create(ProductTypeSimpleEnum.FxSwap.ToString())},
                    ItemsElementName = new[] {ItemsChoiceType2.productType}
                };
            FpMLFieldResolver.TradeSetFxSwap(trade, fxSwap);
            return trade;
        }

        #endregion
    }
}