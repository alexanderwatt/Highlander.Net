#region Usings

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
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.ForeignExchange.FxLeg;
using Orion.Analytics.Schedulers;
using Orion.Analytics.Utilities;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.ValuationEngine.Factory;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    [Serializable]
    public class FxSingleLegPricer : InstrumentControllerBase, IPriceableFxLeg<IFxLegParameters, IFxLegInstrumentResults>, IPriceableInstrumentController<FxSingleLeg>
    {
        #region Member Fields

        protected const string CModelIdentifier = "FxLeg";
        //protected const string CDefaultBucketingInterval = "3M";

        // BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        #endregion

        #region Public Fields

        // Analytics
        public IModelAnalytic<IFxLegParameters, FxLegInstrumentMetrics> AnalyticsModel { get; set; }

        // Requirements for pricing
        /// <summary>
        /// The first reporting currency fx curve.
        /// </summary>
        public string ReportingCurrencyFxCurveName1 { get; set; }

        /// <summary>
        /// The second reporting currency fx curve.
        /// </summary>
        public string ReportingCurrencyFxCurveName2 { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IFxLegParameters AnalyticModelParameters { get; protected set; }

        // Requirements for pricing
        /// <summary>
        /// The first currency.
        /// </summary>
        public Currency Currency1 { get; set; }

        /// <summary>
        /// THe first currency curve.
        /// </summary>
        public string Currency1DiscountCurveName { get; set; }

        /// <summary>
        /// THe first currency payment.
        /// </summary>
        public InstrumentControllerBase Currency1Payment => Payments[0];

        // Requirements for pricing
        /// <summary>
        /// The second currency.
        /// </summary>
        public Currency Currency2 { get; set; }

        /// <summary>
        /// The second currency curve.
        /// </summary>
        public string Currency2DiscountCurveName { get; set; }

        /// <summary>
        /// THe second currency payment.
        /// </summary>
        public InstrumentControllerBase Currency2Payment => Payments[1];

        /// <summary>
        /// The underlying payments for a delivered fx transaction.
        /// </summary>
        public IList<PriceablePayment> Payments { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IFxLegInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// THe exchange rate.
        /// </summary>
        public ExchangeRate ExchangeRate { get; set; }

        /// <summary>
        /// The HybridValuation flag.
        /// </summary>
        public bool HybridValuation { get; protected set; }

        /// <summary>
        /// THe ifx index curve, if hybrid vakuation is used.
        /// </summary>
        public string FxIndexCurveName { get; set; }

        /// <summary>
        /// The IsNonDeliverableForward flag.
        /// </summary>
        public bool IsNonDeliverableForward { get; set; }

        /// <summary>
        /// The settlement currency.
        /// </summary>
        public Currency SettlementCurrency { get; set; }

        /// <summary>
        /// The settlement currency curve.
        /// </summary>
        public string SettlementCurrencyDiscountCurveName { get; set; }

        #endregion

        #region Constructors

        public FxSingleLegPricer()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new FxLegAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FxSingleLegPricer"/> class.  All the cashfloews must be signed.
        /// </summary>
        /// <param name="fxLeg">The fxLeg.</param>
        /// <param name="baseParty">The the base party.</param>
        /// <param name="productType">THe product type: This should only be FxSpot or FxForward.</param>
        public FxSingleLegPricer(FxSingleLeg fxLeg, string baseParty, ProductTypeSimpleEnum productType)
        {
            Multiplier = 1.0m;
            OrderedPartyNames = new List<string>();
            Id = fxLeg.id;
            AnalyticsModel = new FxLegAnalytic();
            ProductType = productType;
            ExchangeRate = fxLeg.exchangeRate;
            HybridValuation = true;
            //Get the currency.
            PaymentCurrencies = new List<string>();
            Currency1 = fxLeg.exchangedCurrency1.paymentAmount.currency;
            Currency2 = fxLeg.exchangedCurrency2.paymentAmount.currency;
            if (!PaymentCurrencies.Contains(Currency1.Value))
            {
                PaymentCurrencies.Add(Currency1.Value);
            }
            if (!PaymentCurrencies.Contains(Currency2.Value))
            {
                PaymentCurrencies.Add(Currency2.Value);
            }
            //Set the default discount curve name.
            Currency1DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency1.Value, true);
            //Set the default discount curve name.
            Currency2DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency2.Value, true);
            //TODO
            //Set the appropraiet cross
            //if the quotebasis is : Currency1PerCurrency2 the currency curve is: currency1-currency2
            if (ExchangeRate.quotedCurrencyPair.quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
            {
                FxIndexCurveName = MarketEnvironmentHelper.ResolveFxCurveNames(Currency1.Value,
                                                                    Currency2.Value);
            }            //Otherwise it is Currency2-Currency1
            else
            {
                FxIndexCurveName = MarketEnvironmentHelper.ResolveFxCurveNames(Currency2.Value,
                                                                    Currency1.Value);
            }
            if (fxLeg.nonDeliverableSettlement != null)//TODO Not implemented yet.
            {
                IsNonDeliverableForward = true;
                SettlementCurrency = fxLeg.nonDeliverableSettlement.settlementCurrency;
                SettlementCurrencyDiscountCurveName = CurveNameHelpers.GetDiscountCurveName(SettlementCurrency.Value, true);
            }
            //Build the coupons and principal exchanges.
            Payments = PriceableInstrumentsFactory.CreatePriceableFxLegPayment(baseParty, fxLeg);                   
            //TODO: add extra cashflowss.
            RiskMaturityDate = fxLeg.Items1ElementName[0] == Items1ChoiceType.valueDate ? fxLeg.Items1[0] : LastDate();
        }

        #endregion

        #region Bucketing

        /// <summary>
        /// Gets the bucketed coupon dates.
        /// </summary>
        /// <param name="baseDate">The base datew.</param>
        /// <param name="bucketInterval">The bucket interval.</param>
        /// <returns></returns>
        protected IDictionary<string, DateTime[]> GetBucketedPaymentDates(DateTime baseDate, Period bucketInterval)
        {
            IDictionary<string, DateTime[]> bucketPaymentDates = new Dictionary<string, DateTime[]>();
            var bucketDates = new List<DateTime>();

            foreach (var payment in Payments)
            {
                bucketDates.AddRange(new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(baseDate, payment.PaymentDate, BucketingInterval, RollConventionEnum.NONE, out _, out _)));
                bucketDates = RemoveDuplicates(bucketDates);
                bucketPaymentDates.Add(payment.Id, bucketDates.ToArray());
            }
            return bucketPaymentDates;
        }

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>();
            if (Payments.Count > 1)
            {
                bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, bucketInterval, RollConventionEnum.NONE, out _, out _));
            }
            return bucketDates.ToArray();
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public FxSingleLeg Build()//TODO implement this.
        {
            var fxLeg = new FxSingleLeg
                            {
                                Items = new object[] { new ProductType { Value = ProductType.ToString() } },
                                ItemsElementName = new[] { ItemsChoiceType2.productType },
                                exchangedCurrency1 = Payments[0].Build(),
                                exchangedCurrency2 = Payments[1].Build(),
                                exchangeRate = ExchangeRate,
                                id = Id,
                                Items1 = new[] {RiskMaturityDate},
                                Items1ElementName = new[] { Items1ChoiceType.valueDate }
                            };
            return fxLeg;
        }

        #endregion

        #region Metrics for Valuation

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
                AnalyticsModel = new FxLegAnalytic();
            }
            AssetValuation streamValuation;
            //Check if risk calc are required.
            bool delta0PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta0PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta0PDH.ToString()) != null;
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var childValuations = new List<AssetValuation>();
            foreach (var payment in Payments)
            {
                payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                payment.BucketedDates = BucketedDates;
                payment.Multiplier = Multiplier;
            }

            if (modelData.MarketEnvironment is IFxLegEnvironment marketEnvironment)//TODO need to look at PricingStructureEvolutionType.
            {
                //Modify the second market.
                var curveName1 = marketEnvironment.GetExchangeCurrencyPaymentEnvironment1();
                var modelData1 = new InstrumentControllerData(modelData.AssetValuation, curveName1,
                                                              modelData.ValuationDate, modelData.ReportingCurrency);
                var curveName2 = marketEnvironment.GetExchangeCurrencyPaymentEnvironment2();
                var modelData2 = new InstrumentControllerData(modelData.AssetValuation, curveName2,
                                                              modelData.ValuationDate, modelData.ReportingCurrency);
                childValuations.Add(Currency1Payment.Calculate(modelData1));
                childValuations.Add(Currency2Payment.Calculate(modelData2));
            }
            else if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                var market = (MarketEnvironment)modelData.MarketEnvironment;
                if (delta0PDH)
                {
                    //Force building of the risk curves.
                    //market.SearchForPerturbedPricingStructures(FxIndexCurveName, "delta0PDH");//TODO Need to add this perturbation to fxCurve.
                }
                if (delta1PDH)
                {
                    //Force building of the risk curves.
                    market.SearchForPerturbedPricingStructures(Currency1DiscountCurveName, "delta1PDH");
                    market.SearchForPerturbedPricingStructures(Currency2DiscountCurveName, "delta1PDH");
                }
                //This defines the cross cureves from the payment currency to the reporting
                //currency for each leg of the fx trade.
                //
                if (modelData.ReportingCurrency.Value != Currency1.Value)
                {
                    ReportingCurrencyFxCurveName1 =
                        MarketEnvironmentHelper.ResolveFxCurveNames(Currency1.Value,
                                                                    modelData.ReportingCurrency.Value);
                }
                if (modelData.ReportingCurrency.Value != Currency2.Value)
                {
                    ReportingCurrencyFxCurveName2 =
                        MarketEnvironmentHelper.ResolveFxCurveNames(Currency2.Value,
                                                                    modelData.ReportingCurrency.Value);
                }
                childValuations.Add(Currency1Payment.Calculate(modelData));
                childValuations.Add(Currency2Payment.Calculate(modelData));
            }
            var paymentValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);
            var childControllerValuations = new List<AssetValuation> {paymentValuation};
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var rate = ExchangeRate.rate;
                if(modelData.MarketEnvironment is MarketEnvironment market)
                {
                    var fxCurve = (IFxCurve)market.SearchForPricingStructureType(FxIndexCurveName);
                    if (HybridValuation)
                    {
                        var curve1 = (IRateCurve) market.SearchForPricingStructureType(Currency1DiscountCurveName);
                        var curve2 = (IRateCurve) market.SearchForPricingStructureType(Currency2DiscountCurveName);
                        var flag = ExchangeRate.quotedCurrencyPair.quoteBasis == QuoteBasisEnum.Currency2PerCurrency1;
                        AnalyticsModel = new FxLegAnalytic(modelData.ValuationDate, RiskMaturityDate, fxCurve, curve1,
                                                           curve2, flag);
                    }
                    else
                    {
                        AnalyticsModel = new FxLegAnalytic(modelData.ValuationDate, RiskMaturityDate, fxCurve);
                    }
                }
                IFxLegParameters analyticModelParameters = new FxLegParameters
                {
                    Multiplier = Multiplier,
                    MarketQuote = rate,
                };
                CalculationResults =
                        AnalyticsModel.Calculate<IFxLegInstrumentResults, FxLegInstrumentResults>(
                            analyticModelParameters, streamControllerMetrics.ToArray());
                    // Now merge back into the overall stream valuation
                    var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                    streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                           childControllerValuations,
                                                                           ConvertMetrics(streamControllerMetrics),
                                                                           new List<string>(Metrics), PaymentCurrencies);
            }
            else
            {
                streamValuation = paymentValuation;
            }
            CalculationPerfomedIndicator = true;
            streamValuation.id = Id;
            return streamValuation;
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
        /// Aggregates the exchange metric.
        /// </summary>
        /// <param name="childValuations"> </param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        public Decimal AggregatePaymentMetric(List<AssetValuation> childValuations, string metric)
        {
            var enumMetric = EnumHelper.Parse<InstrumentMetrics>(metric);
            decimal result = AggregatePaymentMetric(childValuations, enumMetric);
            return result;
        }

        /// <summary>
        /// Aggregates the exchange metric.
        /// </summary>
        /// <param name="childValuations"> </param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected virtual Decimal AggregatePaymentMetric(List<AssetValuation> childValuations, InstrumentMetrics metric)
        {
            var result = 0.0m;
            if (Payments != null && Payments.Count > 0)
            {
                //var childValuations = GetChildValuations(Payments.ToArray(), new List<string>(metrics), ModelData);
                result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            }
            return result;
        }

        public void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName)
        {
            Currency1DiscountCurveName = currency1CurveName;
            Currency2DiscountCurveName = currency2CurveName;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">The controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
        {
            var result = new AssetValuation();
            var quotes = new List<Quotation>();
            foreach (var metric in controllerMetrics)
            {
                var quote = new Quotation();
                var measure = new AssetMeasureType { Value = metric };
                quote.measureType = measure;
                quote.value = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
                quote.valueSpecified = true;
                quotes.Add(quote);
            }
            result.quote = quotes.ToArray();
            return result;
        }

        #endregion

        #region Static Helpers

        internal static IList<InstrumentControllerBase> MapPayments(IList<PriceablePayment> payments)
        {
            return payments.Cast<InstrumentControllerBase>().ToList();
        }

        public DateTime LastDate()
        {
            var tempDate = Currency1Payment.RiskMaturityDate;
            return tempDate >= Currency2Payment.RiskMaturityDate ? tempDate : Currency2Payment.RiskMaturityDate;
        }


        /// <summary>
        /// Removes the duplicates
        /// </summary>
        /// <param name="inputList">The input list.</param>
        /// <returns></returns>
        protected static List<T> RemoveDuplicates<T>(List<T> inputList)
        {
            var uniqueStore = new List<T>();
            foreach (T currValue in inputList)
            {
                if (!uniqueStore.Contains(currValue))
                {
                    uniqueStore.Add(currValue);
                }
            }
            return uniqueStore;
        }

        /// <summary>
        /// Build the fx leg.
        /// </summary>
        /// <param name="exchangeCurrency1PayPartyReference"></param>
        /// <param name="exchangeCurrency2PayPartyReference"></param>
        /// <param name="exchangeCurrency1Amount"></param>
        /// <param name="exchangeCurrency1"></param>
        /// <param name="exchangeCurrency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="spotRate"></param>
        /// <param name="forwardRate"></param>
        /// <param name="forwardPoints"></param>
        /// <returns></returns>
        public static FxSingleLeg ParseForward(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
            DateTime valueDate, Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            var fxLeg = FxSingleLeg.CreateForward(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, forwardPoints);
            return fxLeg;
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
        public static FxSingleLeg ParseSpot(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference, decimal exchangeCurrency1Amount,
                string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis, DateTime valueDate, Decimal spotRate)
        {
            var fxLeg = FxSingleLeg.CreateSpot(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate);
            return fxLeg;
        }

        public static Trade CreateFxLeg(string tradeId, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
            DateTime valueDate, Decimal spotRate, Decimal? forwardRate, Decimal? forwardPoints)
        {
            var trade = new Trade {id = tradeId, tradeHeader = new TradeHeader()};
            var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
            var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate {Value = tradeDate};
            FxSingleLeg fxLeg;
            if (forwardRate==null)
            {
                fxLeg = ParseSpot(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate);
            }
            else
            {
                fxLeg = ParseForward(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1Amount,
            exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, (decimal)forwardRate, forwardPoints);
            }
            FpMLFieldResolver.TradeSetFxLeg(trade, fxLeg);
            return trade;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return MapPayments(Payments);
        }

        #endregion

    }
}