#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Instruments.Equity;
using FpML.V5r10.Reporting.Models.Equity;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Assets.Equity;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.Util.Logging;
using Orion.Util.Serialisation;
using Orion.ValuationEngine.Factory;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers.Assets
{
    public class EquityTransactionPricer : InstrumentControllerBase, IPriceableEquityTransaction<IEquityAssetParameters, IEquityAssetResults>, IPriceableInstrumentController<EquityTransaction>
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether [base party buyer].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party buyer]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyBuyer { get; set; }

        /// <summary>
        /// The buyer reference
        /// </summary>
        public string BuyerReference { get; set; }

        /// <summary>
        /// The seller reference
        /// </summary>
        public string SellerReference { get; set; }

        ///<summary>
        /// THe settlement calendar.
        ///</summary>
        public IBusinessCalendar SettlementCalendar { get; set; }

        ///<summary>
        /// The payment calendar.
        ///</summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// The purchase price
        /// </summary>
        public Money PurchasePrice { get; set; }

        /// <summary>
        /// The reference equity in the format: AAD.AU
        /// </summary>
        public String ReferenceEquity { get; set; }

        /// <summary>
        /// The number of shares
        /// </summary>
        public int NumberOfShares { get; set; }

        /// <summary>
        /// THe purchase as a quote
        /// </summary>
        public BasicQuotation Quote
        {
            get
            {
                var quote = new BasicQuotation
                    {
                        value = PurchasePrice.amount,
                        valueSpecified = true,
                        currency = PurchasePrice.currency,
                        measureType = AssetMeasureTypeHelper.Parse(RateQuotationType),
                        quoteUnits = PriceQuoteUnitsHelper.Parse("Price")
                    };
                return quote;
            }
        }

        /// <summary>
        /// The equity information.
        /// </summary>
        public EquityNodeStruct EquityTypeInfo { get; set; }

        /// <summary>
        /// Gets the trade date.
        /// </summary>
        /// <value>The trade date.</value>
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Gets the settlement date.
        /// </summary>
        /// <value>The settlement date.</value>
        public DateTime SettlementDate { get; set; }

        /// <summary>
        /// Gets the maturity date.
        /// </summary>
        /// <value>The maturity date.</value>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// The underlying bond.
        /// </summary>
        public IPriceableEquityAssetController UnderlyingEquity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IEquityAssetParameters, EquityMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : 
        /// AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate),
        /// AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Equity";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public string DiscountCurveName { get; set; }
        //public string ForecastCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketDividendDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IEquityAssetParameters AnalyticModelParameters { get; protected set; }

        #endregion

        #region Constructors

        protected EquityTransactionPricer()
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="tradeDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="referenceEquity"></param>
        /// <param name="settlementCalendar"></param>
        /// <param name="equityFpML"></param>
        /// <param name="basePartyReference"></param>
        public EquityTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate, DateTime effectiveDate,
            String referenceEquity, IBusinessCalendar settlementCalendar, EquityTransaction equityFpML, string basePartyReference)
            : this(logger, cache, nameSpace, tradeDate, effectiveDate, referenceEquity, settlementCalendar, equityFpML, basePartyReference, false)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="tradeDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="referenceEquity"></param>
        /// <param name="settlementCalendar"></param>
        /// <param name="equityFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        public EquityTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate, DateTime effectiveDate, 
            String referenceEquity, IBusinessCalendar settlementCalendar, EquityTransaction equityFpML, string basePartyReference, Boolean forecastRateInterpolation)
        {
            logger.LogInfo("EquityType set. Commence to build a equity transaction.");
            if (equityFpML == null) return;
            SettlementDate = effectiveDate;
            TradeDate = tradeDate;
            Multiplier = 1.0m;
            BuyerReference = equityFpML.buyerPartyReference.href;
            PaymentCurrencies = new List<string> { equityFpML.unitPrice.currency.Value };
            SellerReference = equityFpML.sellerPartyReference.href;
            BasePartyBuyer = basePartyReference == equityFpML.buyerPartyReference.href;
            ForecastRateInterpolation = forecastRateInterpolation;
            SettlementCalendar = settlementCalendar;
            ReferenceEquity = referenceEquity;
            NumberOfShares = Convert.ToInt16(equityFpML.numberOfUnits);
            PurchasePrice = MoneyHelper.GetAmount(equityFpML.unitPrice.amount, equityFpML.unitPrice.currency.Value);
            PaymentCurrencies = new List<string> { equityFpML.unitPrice.currency.Value };
            var exchangeMIC = equityFpML.equity.exchangeId;
            var exchangeMICData = InstrumentDataHelper.CreateEquityExchangeKey(nameSpace, exchangeMIC.Value);
            var exchangeData = cache.LoadItem<ExchangeConfigData>(exchangeMICData);
            if (exchangeData?.Data is ExchangeConfigData)
            {
                var exchange = (ExchangeConfigData) exchangeData.Data;
                var equityTypeInfo = new EquityNodeStruct {SettlementDate = exchange.SettlementDate};
                if (equityFpML.equity != null)
                {
                    if (SettlementCalendar == null)
                    {
                        SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache,
                                                                                     equityTypeInfo.SettlementDate
                                                                                                   .businessCenters,
                                                                                     nameSpace);
                    }
                    if (PaymentCalendar == null)
                    {
                        PaymentCalendar = SettlementCalendar;
                    }
                    var equity = XmlSerializerHelper.Clone(equityFpML.equity);
                    EquityTypeInfo = XmlSerializerHelper.Clone(equityTypeInfo);
                    EquityTypeInfo.Equity = equity;;
                    RiskMaturityDate = SettlementDate;
                    MaturityDate = SettlementDate;
                    if (!PaymentCurrencies.Contains(equityFpML.equity.currency.Value))
                    {
                        PaymentCurrencies.Add(equityFpML.equity.currency.Value);
                    }
                    logger.LogInfo("Equity transaction has been successfully created.");
                }
            }
            else
            {
                logger.LogInfo("Equity type data not available.");
            }
            //Add payments like the settlement price
            if (PurchasePrice == null || !PurchasePrice.amountSpecified) return;
            var amount = PurchasePrice.amount * NumberOfShares;
            var settlementPayment = PaymentHelper.Create("EquitySettlemetAmount", BuyerReference, SellerReference, amount, SettlementDate);
            AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] { settlementPayment }, PaymentCalendar);
            if (!PaymentCurrencies.Contains(settlementPayment.paymentAmount.currency.Value))
            {
                PaymentCurrencies.Add(settlementPayment.paymentAmount.currency.Value);
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        ///<summary>
        ///</summary>
        public DateTime GetSettlementDate(DateTime baseDate, IBusinessCalendar settlementCalendar, RelativeDateOffset settlementDateOffset)
        {
            try
            {
                return settlementCalendar.Advance(baseDate, settlementDateOffset, settlementDateOffset.businessDayConvention);
            }
            catch (System.Exception)
            {
                throw new System.Exception("No settlement calendar set.");
            }
        }

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public EquityTransaction Build()
        {
            //var equity = UnderlyingEquity.();
            var buyerPartyReference = PartyReferenceHelper.Parse(BuyerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(SellerReference);
            var productType = new object[] {ProductTypeHelper.Create("EquityTransaction")};
            var itemName = new[] {ItemsChoiceType2.productType};
            var equity = XmlSerializerHelper.Clone(EquityTypeInfo.Equity);
            var equityTransaction = new EquityTransaction
                           {
                               buyerPartyReference = buyerPartyReference,
                               sellerPartyReference = sellerPartyReference,
                               id = Id,
                               Items = productType,
                               ItemsElementName = itemName,
                               numberOfUnits = NumberOfShares,
                               unitPrice = XmlSerializerHelper.Clone(PurchasePrice),
                               equity = equity
                           };
            return equityTransaction;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new EquityTransactionAnalytic();
            //1. Create the equity
            UnderlyingEquity = new PriceableEquitySpot(modelData.ValuationDate, NumberOfShares, EquityTypeInfo, SettlementCalendar, Quote);
            //Setting other releavnt information
            SettlementDate = UnderlyingEquity.SettlementDate;
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var metricsAsString = metrics.Select(metric => metric.ToString()).ToList();
            var controllerData = PriceableAssetFactory.CreateAssetControllerData(metricsAsString.ToArray(), modelData.ValuationDate, modelData.MarketEnvironment);
            UnderlyingEquity.EquityCurveName = CurveNameHelpers.GetEquityCurveName(EquityTypeInfo.Equity.currency.Value, ReferenceEquity);
            UnderlyingEquity.Multiplier = Multiplier;
            UnderlyingEquity.Calculate(controllerData);
            // store inputs and results from this run
            AnalyticModelParameters = ((PriceableEquityAssetController)UnderlyingEquity).AnalyticModelParameters;
            AnalyticsModel = ((PriceableEquityAssetController)UnderlyingEquity).AnalyticsModel;
            CalculationResults = ((PriceableEquityAssetController)UnderlyingEquity).CalculationResults;
            CalculationPerfomedIndicator = true;
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IEquityAssetResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetAdditionalPayments()
        {
            return AdditionalPayments?.Cast<InstrumentControllerBase>().ToList();
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return GetAdditionalPayments();
        }

        #endregion

        #region Static Helpers

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate));
            return bucketDates.ToArray();
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
    }
}
