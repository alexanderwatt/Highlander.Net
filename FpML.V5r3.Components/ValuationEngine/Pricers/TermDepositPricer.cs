#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Models.Rates.TermDeposit;
using Orion.Analytics.Schedulers;
using Orion.Util.Logging;
using Orion.ValuationEngine.Instruments;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.Utilities;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.ForeignExchange;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Factory;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class TermDepositPricer : InstrumentControllerBase, IPriceableInstrumentController<TermDeposit>, IPriceableTermDeposit<ITermDepositInstrumentParameters, ITermDepositInstrumentResults>
    {
        #region Properties

        #region Implementation of IPriceableTermDeposit<IProductParameters,IProductResults>

        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party paying fixed]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyPayingFixed { get; set; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <value>The effective date.</value>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        public DateTime TerminationDate { get; set; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool AdjustCalculationDatesIndicator { get; set; }

        #endregion

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        /// <summary>
        /// The fixed rate.
        /// </summary>
        public decimal FixedRate { get; set; }

        ////Prodcut parameters
        ////public FraInputRange FraInputRange { get; set; } 
        //public TermDeposit Deposit { get; set; }

        public List<AssetValuation> ChildValuations { get; set; }

        // Analytics
        public IModelAnalytic<ITermDepositInstrumentParameters, TermDepositInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "TermDeposit";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public Currency Currency { get; set; }

        // Requirements for pricing
        public DayCountFraction DayCountFraction { get; set; }

        // Requirements for pricing
        public string DiscountCurveName { get; set; }

        // Requirements for pricing
        public string ReportingCurrencyFxCurveName { get; set; }

        // BucketedCouponDates
        //protected Period BucketingInterval;
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ITermDepositInstrumentParameters AnalyticModelParameters { get; protected set; }

        public PriceableFixedRateCoupon InterestAmount { get; protected set; }

        public Money Principal { get; protected set; }

        //protected bool IsBaseLender { get; set; }

        public string BaseParty { get; protected set; }

        public string CounterParty { get; protected set; }

        protected List<PriceablePayment> Payments { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public TermDepositPricer()
        {
            Multiplier = 1.0m;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="baseParty">THe base party is either Party1 or Party2</param>
        /// <param name="depositFpML">THe FpML representation of the deposit correctly built.</param>
        public TermDepositPricer(ILogger logger, ICoreCache cache, TermDeposit depositFpML, string baseParty)
        {
            Multiplier = 1.0m;
            OrderedPartyNames = new List<string>();
            FixedRate = depositFpML.fixedRate;
            EffectiveDate = depositFpML.startDate;
            TerminationDate = depositFpML.maturityDate;
            DayCountFraction = new DayCountFraction {Value = depositFpML.dayCountFraction.Value};
            BasePartyPayingFixed = !IsBasePartyLender(baseParty, depositFpML);
            if (BasePartyPayingFixed)//From the viewpoint of the counterparty!
            {
                BaseParty = depositFpML.receiverPartyReference.href;
                CounterParty = depositFpML.payerPartyReference.href;
            }
            else
            {
                BaseParty = depositFpML.payerPartyReference.href;
                CounterParty = depositFpML.receiverPartyReference.href;
            }
            PaymentDate = TerminationDate;
            RiskMaturityDate = TerminationDate;
            Principal = MoneyHelper.GetMoney(depositFpML.principal);
            Payments = new List<PriceablePayment>();
            if (depositFpML.payment != null && depositFpML.payment.Length == 3)
            {
                //Modify to be princiap and interest.
                //A principal payment ->priceable payment
                //An interest payment -> priceablefixedcoupon
                var intialDate = AdjustableOrAdjustedDateHelper.Create(null, EffectiveDate, null);
                var maturityDate = AdjustableOrAdjustedDateHelper.Create(null, PaymentDate, null);
                depositFpML.payment[0].paymentDate = intialDate;
                depositFpML.payment[1].paymentDate = maturityDate;
                depositFpML.payment[2].paymentDate = maturityDate;//Remove this and replace with depositFpML.interest
                Payments.AddRange(PriceableInstrumentsFactory.CreatePriceablePayments(BaseParty, depositFpML.payment, null));
                AddCashFlow(depositFpML, BasePartyPayingFixed);
            }
            else//This will change the input FpML as well!
            {
                var intial = PaymentHelper.Create(depositFpML.payerPartyReference.href, depositFpML.receiverPartyReference.href, Principal.currency.Value, Principal.amount, EffectiveDate);
                Payments.Add(PriceableInstrumentsFactory.CreatePriceablePayment(BaseParty, intial, null));
                var final = PaymentHelper.Create(depositFpML.receiverPartyReference.href, depositFpML.payerPartyReference.href, depositFpML.principal.currency.Value, Principal.amount, TerminationDate);
                Payments.Add(PriceableInstrumentsFactory.CreatePriceablePayment(BaseParty, final, null));
                //Payments.Add(PaymentHelper.Create("interest payment");
                AddCashFlow(depositFpML, !BasePartyPayingFixed);             
            }
            //Set the product type.
            ProductType = ProductTypeSimpleEnum.TermDeposit;
            Currency = depositFpML.principal.currency;
            PaymentCurrencies = new List<string> { Currency.Value };
            //Set the default discount curve name.
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true);
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Builds a term deposit trade.
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="productType"></param>
        /// <param name="tradeDate"></param>
        /// <param name="startDate"></param>
        /// <param name="maturityDate"></param>
        /// <param name="currency"></param>
        /// <param name="notionalAmount"></param>
        /// <param name="fixedRate"></param>
        /// <param name="dayCount"></param>
        /// <returns></returns>
        public static Trade CreateSimpleTermDepositTrade(string tradeId, string productType, 
            DateTime tradeDate, DateTime startDate, DateTime maturityDate, string currency, 
            decimal notionalAmount, decimal fixedRate, string dayCount)
        {
            var trade = new Trade {id = tradeId, tradeHeader = new TradeHeader()};
            var party1 =  PartyTradeIdentifierHelper.Parse(tradeId, "party1") ;
            var party2 =  PartyTradeIdentifierHelper.Parse(tradeId, "party2") ;
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2};
            trade.tradeHeader.tradeDate = new IdentifiedDate {Value = tradeDate};
            var termDeposit = Parse(productType, tradeDate, startDate, 
                maturityDate, currency, notionalAmount, fixedRate, dayCount);
            FpMLFieldResolver.TradeSetTermDeposit(trade, termDeposit);
            return trade;
        }

        /// <summary>
        /// Builds a term deposit.
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="tradeDate"></param>
        /// <param name="startDate"></param>
        /// <param name="maturityDate"></param>
        /// <param name="currency"></param>
        /// <param name="notionalAmount"></param>
        /// <param name="fixedRate"></param>
        /// <param name="dayCount"></param>
        /// <returns></returns>
        public static TermDeposit Parse(string productType,  DateTime tradeDate, DateTime startDate, 
            DateTime maturityDate, string currency, decimal notionalAmount, decimal fixedRate, string dayCount)
        {
            var termDeposit = new TermDeposit
                {
                    dayCountFraction = DayCountFractionHelper.Parse(dayCount),
                    fixedRate = fixedRate,
                    fixedRateSpecified = true,
                    principal = MoneyHelper.GetPositiveMoney(notionalAmount, currency),
                    startDate = startDate,
                    startDateSpecified = true,
                    maturityDate = maturityDate, 
                    maturityDateSpecified = true,
                    payerPartyReference = PartyReferenceHelper.Parse("Party1"),
                    receiverPartyReference = PartyReferenceHelper.Parse("Party2"),
                    Items = new object[] { ProductTypeHelper.Create(ProductTypeSimpleEnum.TermDeposit.ToString()) },
                    ItemsElementName = new[] {ItemsChoiceType2.productType}
                };
            //Set the party information
            return termDeposit;
        }

        /// <summary>
        /// Determines if the base party is the lender.
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="depositFpML"></param>
        /// <returns></returns>
        public static bool IsBasePartyLender(string baseParty, TermDeposit depositFpML)
        {
            return baseParty == "Party1";
        }

        private void AddCashFlow(TermDeposit depositFpML, bool isLenderBase)
        {
            var fixedInterest = new PriceableFixedRateCoupon(depositFpML.id + "FixedCoupon_1"
                                                             , BasePartyPayingFixed
                                                             , EffectiveDate
                                                             , TerminationDate
                                                             , depositFpML.dayCountFraction
                                                             , depositFpML.fixedRate
                                                             , isLenderBase ? MoneyHelper.GetMoney(depositFpML.principal) : MoneyHelper.Neg(depositFpML.principal)
                                                             , null
                                                             , PaymentDate
                                                             , null
                                                             , null
                                                             , null
                                                             , null);

            InterestAmount = fixedInterest;
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public TermDeposit Build()
        {
            var deposit = new TermDeposit
                              {
                                  Items = new object[] { new ProductType { Value = ProductType.ToString() } },
                                  ItemsElementName = new[] { ItemsChoiceType2.productType },
                                  dayCountFraction = DayCountFraction,
                                  principal = MoneyHelper.GetPositiveMoney(Principal),
                                  maturityDate = TerminationDate,
                                  maturityDateSpecified = true,
                                  startDate = EffectiveDate,
                                  startDateSpecified = true,
                                  fixedRate = FixedRate,
                                  fixedRateSpecified = true
                              };
            if (BasePartyPayingFixed)//From the viewpoint of the counterparty!
            {
                deposit.receiverPartyReference = PartyReferenceHelper.Parse(BaseParty);
                deposit.payerPartyReference = PartyReferenceHelper.Parse(CounterParty);
            }
            else
            {
                deposit.payerPartyReference = PartyReferenceHelper.Parse(BaseParty);
                deposit.receiverPartyReference = PartyReferenceHelper.Parse(CounterParty);
            }
            //deposit payments. 
            if (deposit.payment?.Length == 3)
            {
                var intialDate = AdjustableOrAdjustedDateHelper.Create(null, EffectiveDate, null);
                var maturityDate = AdjustableOrAdjustedDateHelper.Create(null, PaymentDate, null);
                deposit.payment[0].paymentDate = intialDate;
                deposit.payment[1].paymentDate = maturityDate;
                deposit.payment[2].paymentDate = maturityDate;
            }
            var interest = System.Math.Abs(InterestAmount.AccruedInterest);
            deposit.interest = MoneyHelper.GetAmount(interest, deposit.principal.currency.Value);
            return deposit;
        }

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
                AnalyticsModel = new TermDepositInstrumentAnalytic();
            }
            var termDepositControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            //Allows the evolution type to be modified pre-calculation.
            //Set the children
            foreach (var cashflow in GetChildren())
            {
                if (cashflow is PriceableCashflow priceableCashflow)
                {
                    priceableCashflow.BucketedDates = BucketedDates;
                    priceableCashflow.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    priceableCashflow.Multiplier = Multiplier;
                }
            }
            var childControllers = new List<InstrumentControllerBase>(GetChildren());
            //The assetValuation list.
            ChildValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            ChildValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(ChildValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            childControllerValuations.id = Id + ".Cashflows";
            AssetValuation depositValuation;
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (termDepositControllerMetrics.Count > 0)
            {
                var reportingCurrency = ModelData.ReportingCurrency == null
                                            ? Currency.Value
                                            : ModelData.ReportingCurrency.Value;
                var marketQuote = FixedRate;
                ITermDepositInstrumentParameters analyticModelParameters = new TermDepositInstrumentParameters
                                                                       {   Multiplier = Multiplier,
                                                                           Currency = Currency.Value,
                                                                           ReportingCurrency = reportingCurrency,
                                                                           BreakEvenRate = InterestAmount.CalculationResults.ImpliedQuote,
                                                                           MarketQuote = marketQuote
                                                                       };
                CalculationResults = AnalyticsModel.Calculate<ITermDepositInstrumentResults, TermDepositInstrumentResults>(analyticModelParameters, termDepositControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                depositValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                    ChildValuations, ConvertMetrics(termDepositControllerMetrics), new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            }
            else
            {
                depositValuation = childControllerValuations;
            }
            CalculationPerfomedIndicator = true;
            depositValuation.id = Id;
            return depositValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ITermDepositInstrumentResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = new List<InstrumentControllerBase>();
            children.AddRange(Payments);
            children.Add(InterestAmount);
            return children;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">THe controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateCouponMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
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

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected virtual Decimal AggregateCouponMetric(InstrumentMetrics metric)
        {
            string[] metrics = { metric.ToString() };
            var childValuations = GetChildValuations(GetChildren().ToArray(), new List<string>(metrics), ModelData);
            decimal result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            return result;
        }

        ///// <summary>
        ///// Updates the bucketing interval.
        ///// </summary>
        ///// <param name="baseDate">The base date for bucketting.</param>
        ///// <param name="bucketingInterval">The bucketing interval.</param>
        //public void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval)
        //{
        //    BucketingInterval = bucketingInterval;
        //    BucketingDates = GetBucketingDates(baseDate, bucketingInterval);
        //    //foreach (var coupon in GetChildren())
        //    //{
        //    //    var priceableRateCoupon = coupon as PriceableCashflow;
        //    //    if (priceableRateCoupon != null)
        //    //    {
        //    //        priceableRateCoupon.BucketedDates = BucketingDates;
        //    //    }
        //    //}
        //}

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            return bucketDates.ToArray();
        }

        #endregion

        #region Implementation of IPriceable

        /// <summary>
        /// Prices the product.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="reportingParty"></param>
        /// <param name="deposit"></param>
        /// <param name="baseCurrency"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public static double GetNPV(ILogger logger, ICoreCache cache,
            string reportingParty, TermDeposit deposit, string baseCurrency,
            DateTime valuationDate, IMarketEnvironment market)
        {
            var agreement = new TermDepositPricer(logger, cache, deposit, reportingParty);
            var modelData = CreateInstrumentModelData(new[] { "NPV" }, valuationDate, market, baseCurrency);
            var av = agreement.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        /// <summary>
        /// Prices the product.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="reportingParty"></param>
        /// <param name="deposit"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public static double GetParRate(ILogger logger, ICoreCache cache,
            string reportingParty, TermDeposit deposit,
            DateTime valuationDate, IMarketEnvironment market)
        {
            var agreement = new TermDepositPricer(logger, cache, deposit, reportingParty);
            var modelData = CreateInstrumentModelData(new[] { "ImpliedQuote" }, valuationDate, market, agreement.Currency.Value);
            var av = agreement.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        #endregion
    }
}