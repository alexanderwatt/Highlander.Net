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
using FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates;
using FpML.V5r10.Reporting.Models.Assets;
using FpML.V5r10.Reporting.Models.Rates.Bonds;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Serialisation;
using Orion.ValuationEngine.Factory;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers.Assets
{
    public class BondTransactionPricer : InstrumentControllerBase, IPriceableBondTransaction<IBondAssetParameters, IBondAssetResults>, IPriceableInstrumentController<BondTransaction>
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
        /// The base date from the trade pricer.
        /// </summary>
        public DateTime TradeDate { get; set; }

        ///<summary>
        /// THe settlement calendar.
        ///</summary>
        public IBusinessCalendar SettlementCalendar { get; set; }

        ///<summary>
        /// The payment calendar.
        ///</summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// The buyer reference
        /// </summary>
        public string BuyerReference { get; set; }

        /// <summary>
        /// The seller reference
        /// </summary>
        public string SellerReference { get; set; }

        /// <summary>
        /// THe quote Type.
        /// </summary>
        public BondPriceEnum QuoteType { get; set; }

        /// <summary>
        /// THe purchase price
        /// </summary>
        public BasicQuotation Quote { get; set; }

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// The type of bond: 
        /// Fixed,
        /// Float,
        /// Struct.
        /// </summary>
        public BondTypesEnum BondType { get; set; }

        /// <summary>
        /// The notional amount
        /// </summary>
        public Money NotionalAmount { get; set; }

        /// <summary>
        /// THe bond price information
        /// </summary>
        public BondPrice BondPrice { get; set; }

        /// <summary>
        /// The bond information.
        /// </summary>
        public BondNodeStruct BondTypeInfo { get; set; }
        
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
        public IPriceableBondAssetController UnderlyingBond { get; set; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool AdjustCalculationDatesIndicator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IBondAssetParameters, BondMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? 
        /// AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), 
        /// AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Bond";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public string DiscountCurveName { get; set; }
        //public string ForecastCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IBondAssetParameters AnalyticModelParameters { get; protected set; }

        #endregion

        #region Constructors

        protected BondTransactionPricer()
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="tradeDate"></param>
        /// <param name="settlementDate">The payment settlement date.</param>
        /// <param name="settlementCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="bondFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="bondType"></param>
        public BondTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate,
            DateTime settlementDate, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar,
            BondTransaction bondFpML, string basePartyReference, string bondType)
            : this(logger, cache, nameSpace, tradeDate, settlementDate, settlementCalendar, paymentCalendar, bondFpML, basePartyReference, bondType, false)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="tradeDate"></param>
        /// <param name="settlementDate">The payment settlement date.</param>
        /// <param name="settlementCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="bondFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="bondType"></param>
        /// <param name="forecastRateInterpolation"></param>
        public BondTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate,
            DateTime settlementDate, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar,
            BondTransaction bondFpML, string basePartyReference, string bondType, Boolean forecastRateInterpolation)
        {
            Multiplier = 1.0m;
            TradeDate = tradeDate;
            BondType = EnumHelper.Parse<BondTypesEnum>(bondType);
            logger.LogInfo("BondType set. Commence to build a bond transaction.");
            if (bondFpML == null) return;
            BuyerReference = bondFpML.buyerPartyReference.href;
            PaymentCurrencies = new List<string> {bondFpML.notionalAmount.currency.Value};
            SellerReference = bondFpML.sellerPartyReference.href;
            BasePartyBuyer = basePartyReference == bondFpML.buyerPartyReference.href;
            if (!BasePartyBuyer)
            {
                Multiplier = -1.0m;
            }
            ForecastRateInterpolation = forecastRateInterpolation;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = paymentCalendar;
            //Set the bond price information
            BondPrice = new BondPrice();
            if (bondFpML.price.accrualsSpecified)
            {
                BondPrice.accrualsSpecified = true;
                BondPrice.accruals = bondFpML.price.accruals;
            }
            if (bondFpML.price.dirtyPriceSpecified)
            {
                BondPrice.dirtyPriceSpecified = true;
                BondPrice.dirtyPrice = bondFpML.price.dirtyPrice;
            }
            BondPrice.cleanOfAccruedInterest = bondFpML.price.cleanOfAccruedInterest;
            BondPrice.cleanPrice = bondFpML.price.cleanPrice;
            //Set the notional information
            NotionalAmount = MoneyHelper.GetAmount(bondFpML.notionalAmount.amount, bondFpML.notionalAmount.currency.Value);
            //Determines the quotation and units
            QuoteType = BondPriceEnum.YieldToMaturity;
            //We need to get the ytm in until there is a bond market price/spread.
            if (BondPrice.dirtyPriceSpecified)
            {
                QuoteType = BondPriceEnum.DirtyPrice;
                Quote = BasicQuotationHelper.Create(BondPrice.dirtyPrice, RateQuotationType);
            }
            //Get the insturment configuration information.
            var assetIdentifier = bondFpML.bond.currency.Value + "-Bond-" + BondType;
            BondNodeStruct bondTypeInfo = null;
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
            if (instrument != null)
            {
                bondTypeInfo = instrument.InstrumentNodeItem as BondNodeStruct;
            }
            if (bondFpML.bond != null &&bondTypeInfo != null)
            {
                if (SettlementCalendar == null)
                {
                    SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, bondTypeInfo.SettlementDate.businessCenters, nameSpace);
                }
                if (PaymentCalendar == null)
                {
                    PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, bondTypeInfo.BusinessDayAdjustments.businessCenters, nameSpace);
                }
                //Preprocesses the data for the priceableasset.
                var bond = XmlSerializerHelper.Clone(bondFpML.bond);
                BondTypeInfo = XmlSerializerHelper.Clone(bondTypeInfo);
                BondTypeInfo.Bond = bond;
                //This is done because the config data is not stored in the ciorrect way. Need to add a price quote units.
                if (bond.couponRateSpecified)
                {
                    var coupon = bond.couponRate;
                    BondTypeInfo.Bond.couponRate = coupon;
                }
                BondTypeInfo.Bond.faceAmount = NotionalAmount.amount;
                if (BondTypeInfo.Bond.maturitySpecified)
                {
                    RiskMaturityDate = BondTypeInfo.Bond.maturity;
                }
                SettlementDate = settlementDate;
                if (!PaymentCurrencies.Contains(bondFpML.bond.currency.Value))
                {
                    PaymentCurrencies.Add(bondFpML.bond.currency.Value);
                }
                logger.LogInfo("Bond transaction has been successfully created.");
            }
            else
            {
                logger.LogInfo("Bond type data not available.");
            }
            //Add payments like the settlement price
            if (!BondPrice.dirtyPriceSpecified) return;
            var amount = BondPrice.dirtyPrice * NotionalAmount.amount / 100;
            var settlementPayment = PaymentHelper.Create("BondSettlemetAmount", BuyerReference, SellerReference, amount, SettlementDate);
            AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] { settlementPayment }, SettlementCalendar);
            if (!PaymentCurrencies.Contains(settlementPayment.paymentAmount.currency.Value))
            {
                PaymentCurrencies.Add(settlementPayment.paymentAmount.currency.Value);
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public BondTransaction Build()
        {
            var bond = BondTypeInfo.Bond;
            var buyerPartyReference = PartyReferenceHelper.Parse(BuyerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(SellerReference);
            var productType = new object[] {ProductTypeHelper.Create("BondTransaction")};
            var itemName = new[] {ItemsChoiceType2.productType};
            //TODO extend this
            var bondTransaction = new BondTransaction
                           {
                               notionalAmount = NotionalAmount,
                               bond = bond,
                               price = XmlSerializerHelper.Clone(BondPrice),
                               buyerPartyReference = buyerPartyReference,
                               sellerPartyReference = sellerPartyReference,
                               id = Id,
                               Items = productType,
                               ItemsElementName = itemName
                           };
            return bondTransaction;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new BondTransactionAnalytic();
            //1. Create the bond
            UnderlyingBond = new PriceableSimpleBond(modelData.ValuationDate, BondTypeInfo, SettlementCalendar, PaymentCalendar, Quote, QuoteType);
            if (BondPrice.dirtyPriceSpecified)
            {
                UnderlyingBond.PurchasePrice = BondPrice.dirtyPrice / 100; //PriceQuoteUnits
            }
            //Setting other relevant information          
            MaturityDate = UnderlyingBond.MaturityDate;
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var metricsAsString = metrics.Select(metric => metric.ToString()).ToList();
            var controllerData = PriceableAssetFactory.CreateAssetControllerData(metricsAsString.ToArray(), modelData.ValuationDate, modelData.MarketEnvironment);
            UnderlyingBond.Multiplier = Multiplier;
            UnderlyingBond.Calculate(controllerData);
            // store inputs and results from this run
            AnalyticModelParameters = ((PriceableBondAssetController)UnderlyingBond).AnalyticModelParameters;
            AnalyticsModel = ((PriceableBondAssetController)UnderlyingBond).AnalyticsModel;
            CalculationResults = ((PriceableBondAssetController)UnderlyingBond).CalculationResults;
            CalculationPerfomedIndicator = true;
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IBondAssetResults CalculationResults { get; protected set; }

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
