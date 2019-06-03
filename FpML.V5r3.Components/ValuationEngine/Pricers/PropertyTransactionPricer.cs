#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CurveEngine.Factory;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments.Equity;
using Orion.Models.Property;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.Util.Serialisation;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class PropertyTransactionPricer : InstrumentControllerBase, IPriceablePropertyTransaction<IPropertyAssetParameters, IPropertyAssetResults>, IPriceableInstrumentController<PropertyTransaction>
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether [base party buyer].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party buyer]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyBuyer { get; set; }

        public decimal PreviousSettlementValue { get; set; }
        public decimal CurrentSettlementValue { get; set; }
        public decimal TradePrice { get; set; }
        //Add the type of futures underlyer.
        public IPriceableFuturesAssetController UnderlyingFuture { get; set; }

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
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// The purchase price
        /// </summary>
        public Money PurchasePrice { get; set; }

        /// <summary>
        /// The reference future in the format: AUD-IRFuture-ED1
        /// </summary>
        public String ReferenceContract { get; set; }

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
        public PropertyAsset PropertyInfo { get; set; }
        
        /// <summary>
        /// Gets the payment date.
        /// </summary>
        /// <value>The payment date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// The underlying bond.
        /// </summary>
        public IPriceablePropertyAssetController UnderlyingProperty{ get; set; }

        // Analytics
        public IModelAnalytic<IPropertyAssetParameters, PropertyMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : 
        /// AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Property";

        // Requirements for pricing
        public string DiscountCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IPropertyAssetParameters AnalyticModelParameters { get; protected set; }

        #endregion

        #region Constructors

        protected PropertyTransactionPricer()
        {}

        public PropertyTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime paymentDate,
            String referenceContract, IBusinessCalendar settlementCalendar, PropertyTransaction propertyFpML, string basePartyReference)
            : this(logger, cache, nameSpace, paymentDate, referenceContract, settlementCalendar, propertyFpML, basePartyReference, false)
        {}

        public PropertyTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime paymentDate, String referenceContract,
            IBusinessCalendar settlementCalendar, PropertyTransaction propertyFpML, string basePartyReference, Boolean forecastRateInterpolation)
        {
            logger.LogInfo("PropertyType set. Commence to build a property transaction.");
            if (propertyFpML == null) return;
            //
            //Set the multiplier
            //
            Multiplier = 1.0m;
            //
            //Set the trade information
            //
            BuyerReference = propertyFpML.buyerPartyReference.href;
            PaymentCurrencies = new List<string> { propertyFpML.purchasePrice.currency.Value };
            SellerReference = propertyFpML.sellerPartyReference.href;
            BasePartyBuyer = basePartyReference == propertyFpML.buyerPartyReference.href;
            ForecastRateInterpolation = forecastRateInterpolation;
            SettlementCalendar = settlementCalendar;
            PaymentDate = paymentDate;
            //
            //Set the issuer indormation
            //
            ReferenceContract = referenceContract;
            //
            //Set the notional information
            //
            PurchasePrice = MoneyHelper.GetAmount(propertyFpML.purchasePrice.amount, propertyFpML.purchasePrice.currency.Value);
            PaymentCurrencies = new List<string> { propertyFpML.purchasePrice.currency.Value };
            //
            //Get the insturment configuration information.
            //
            var propertyTypeInfo = propertyFpML.property;
            if (propertyFpML.property != null && propertyTypeInfo != null)
            {
                if (SettlementCalendar == null)
                {
                    SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, propertyTypeInfo.businessDayAdjustments.businessCenters, nameSpace);
                }
                //Preprocesses the data for the priceableasset.
                PropertyInfo = XmlSerializerHelper.Clone(propertyTypeInfo);
                //This is done because the config data is not stored in the ciorrect way. Need to add a price quote units.
                //TODO Set other relevant bond information
                //PropertyTypeInfo.Property.faceAmount = NotionalAmount.amount;
                if (!PaymentCurrencies.Contains(propertyFpML.purchasePrice.currency.Value))
                {
                    PaymentCurrencies.Add(propertyFpML.purchasePrice.currency.Value);
                }
                logger.LogInfo("Property transaction has been successfully created.");
                
            }
            else
            {
                logger.LogInfo("Property type data not available.");
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
        public PropertyTransaction Build()
        {
            //var equity = UnderlyingEquity.();
            var buyerPartyReference = PartyReferenceHelper.Parse(BuyerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(SellerReference);
            var productType = new object[] {ProductTypeHelper.Create("PropertyTransaction")};
            //var productId = new ProductId {Value = "BondTransation"};
            var itemName = new[] {ItemsChoiceType2.productType};
            //TODO extend this
            //var productIds = new[] {productId};
            var property = XmlSerializerHelper.Clone(PropertyInfo);
            var purchasePrice = XmlSerializerHelper.Clone(PurchasePrice);
            var propertyTransaction = new PropertyTransaction
                           {
                               buyerPartyReference = buyerPartyReference,
                               sellerPartyReference = sellerPartyReference,
                               id = Id,
                               //productId = productIds,
                               Items = productType,
                               ItemsElementName = itemName,
                               purchasePrice = purchasePrice,
                               property = property
                           };
            return propertyTransaction;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new PropertyTransactionAnalytic();
            //1. Create the property
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var metricsAsString = metrics.Select(metric => metric.ToString()).ToList();
            //TODO Find the correct curve.
            var controllerData = PriceableAssetFactory.CreateAssetControllerData(metricsAsString.ToArray(), modelData.ValuationDate, modelData.MarketEnvironment);
            //UnderlyingFutures.EquityCurveName = CurveNameHelpers.GetEquityCurveName(FuturesTypeInfo.Future.currency.Value, ReferenceContract);
            //UnderlyingFutures.Multiplier = Multiplier;
            UnderlyingProperty.Calculate(controllerData);
            // store inputs and results from this run
            //AnalyticModelParameters = ((PriceableFutureAssetController)UnderlyingFutures).AnalyticModelParameters;
            //AnalyticsModel = ((PriceableIRFuturesAssetController)UnderlyingFutures).AnalyticsModel;
            //CalculationResults = ((PriceableEquityAssetController)UnderlyingFutures).CalculationResults;
            CalculationPerfomedIndicator = true;
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IPropertyAssetResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
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
