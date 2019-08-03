/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments.Futures;
using Orion.Models.Futures;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.CurveEngine.Assets.Helpers;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Orion.ValuationEngine.Factory;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class FutureTransactionPricer : InstrumentControllerBase, IPriceableFutureTransaction<IFuturesAssetParameters, IFuturesAssetResults>, IPriceableInstrumentController<FutureTransaction>
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
        /// 
        /// </summary>
        public DateTime LastMarginPaymentDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime NextMarginPaymentDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExchangeConfigData Exchange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal PreviousSettlementValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal CurrentSettlementValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal TradePrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IPriceableFuturesAssetController UnderlyingFuture { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

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
        /// The future type
        /// </summary>
        public ExchangeContractTypeEnum FuturesType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IFuturesAssetResults AnalyticResults { get; protected set; }

        /// <summary>
        /// The number of contracts
        /// </summary>
        public int NumberOfContracts { get; set; }

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
        public FutureNodeStruct FuturesTypeInfo { get; set; }

        /// <summary>
        /// Gets the last trade date.
        /// </summary>
        /// <value>The last trade date.</value>
        public DateTime LastTradeDate { get; set; }

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

        // Analytics
        public IModelAnalytic<IFuturesAssetParameters, FuturesMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : 
        /// AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Future";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public string DiscountCurveName { get; set; }

        /// <summary>
        /// The futures curve name to value the contract.
        /// </summary>
        public string FuturesCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IFuturesAssetParameters AnalyticModelParameters { get; protected set; }

        #endregion

        #region Constructors

        protected FutureTransactionPricer()
        {}

        public FutureTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate,
            ExchangeContractTypeEnum futuresType, IBusinessCalendar settlementCalendar, FutureTransaction futureFpML, string basePartyReference)
            : this(logger, cache, nameSpace, tradeDate, futuresType, settlementCalendar, futureFpML, basePartyReference, false)
        {}

        public FutureTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate, ExchangeContractTypeEnum futuresType,
            IBusinessCalendar settlementCalendar, FutureTransaction futureFpML, string basePartyReference, Boolean forecastRateInterpolation)
        {
            logger.LogInfo("FuturesType set. Commence to build a future transaction.");
            if (futureFpML == null) return;
            Multiplier = 1.0m;
            BuyerReference = futureFpML.buyerPartyReference.href;
            PaymentCurrencies = new List<string> { futureFpML.unitPrice.currency.Value };
            SellerReference = futureFpML.sellerPartyReference.href;
            BasePartyBuyer = basePartyReference == futureFpML.buyerPartyReference.href;
            if (!BasePartyBuyer)
            {
                Multiplier = -1.0m;
            }
            ForecastRateInterpolation = forecastRateInterpolation;
            SettlementCalendar = settlementCalendar;
            FuturesType = futuresType;
            ReferenceContract = futureFpML.future.id;
            var futuresCode = ReferenceContract.Split('-')[2];
            NumberOfContracts = Convert.ToInt16(futureFpML.numberOfUnits);
            PurchasePrice = MoneyHelper.GetAmount(futureFpML.unitPrice.amount, futureFpML.unitPrice.currency.Value);
            var exchangeMIC = futureFpML.future.exchangeId;
            FuturesCurveName = CurveNameHelpers.GetExchangeTradedCurveName(futureFpML.unitPrice.currency.Value, exchangeMIC.Value, futuresCode);
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(futureFpML.unitPrice.currency, true);
            FuturesTypeInfo = new FutureNodeStruct();
            var exchangeMICData = InstrumentDataHelper.CreateEquityExchangeKey(nameSpace, exchangeMIC.Value);
            var exchangeData = cache.LoadItem<ExchangeConfigData>(exchangeMICData);
            if (exchangeData?.Data is ExchangeConfigData data)
            {
                Exchange = data;
                FuturesTypeInfo.SpotDate = Exchange.SettlementDate;           
            }
            if (futureFpML.future != null)
            {
                if (SettlementCalendar == null)
                {
                    SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache,
                        FuturesTypeInfo.SpotDate
                            .businessCenters,
                        nameSpace);
                }
                var future = XmlSerializerHelper.Clone(futureFpML.future);
                FuturesTypeInfo.Future = future;
                if(FuturesTypeInfo.SpotDate!=null)
                {
                    SettlementDate = GetSettlementDate(tradeDate, SettlementCalendar,
                    FuturesTypeInfo.SpotDate);
                }
                //Instantiate the priceable future.
                NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, FuturesTypeInfo.Future.id, tradeDate);
                var asset = AssetHelper.Parse(FuturesTypeInfo.Future.id, 0.0m, 0.0m);
                UnderlyingFuture = PriceableAssetFactory.Create(logger, cache, nameSpace, asset.Second, namedValueSet, null, null) as IPriceableFuturesAssetController;
                if (UnderlyingFuture != null)
                {
                    RiskMaturityDate = UnderlyingFuture.GetRiskMaturityDate();
                    MaturityDate = RiskMaturityDate;
                    LastTradeDate = UnderlyingFuture.LastTradeDate;
                }             
                if (!PaymentCurrencies.Contains(futureFpML.future.currency.Value))
                {
                    PaymentCurrencies.Add(futureFpML.future.currency.Value);
                }
                logger.LogInfo("Futures transaction has been successfully created.");
            }
            else
            {
                logger.LogInfo("Futures type data not available.");
            }
            //Add payments like the settlement price
            if (!PurchasePrice.amountSpecified) return;
            var amount = PurchasePrice.amount * NumberOfContracts / 100;
            var settlementPayment = PaymentHelper.Create("FuturesSettlementAmount", BuyerReference, SellerReference, amount, SettlementDate);
            AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] { settlementPayment }, SettlementCalendar);
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
        /// Builds this instance and returns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public FutureTransaction Build()
        {
            var buyerPartyReference = PartyReferenceHelper.Parse(BuyerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(SellerReference);
            var productType = new object[] {ProductTypeHelper.Create("FuturesTransaction")};
            var itemName = new[] {ItemsChoiceType2.productType};
            var future = XmlSerializerHelper.Clone(FuturesTypeInfo.Future);
            var futureTransaction = new FutureTransaction
                           {
                               buyerPartyReference = buyerPartyReference,
                               sellerPartyReference = sellerPartyReference,
                               id = Id,
                               Items = productType,
                               ItemsElementName = itemName,
                               numberOfUnits = NumberOfContracts,
                               unitPrice = XmlSerializerHelper.Clone(PurchasePrice),
                               future = future
                           };
            return futureTransaction;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = new FuturesAssetParameters();
            AnalyticsModel = new FuturesTransactionAnalytic();
            var marketEnvironment = modelData.MarketEnvironment;
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var metricsAsArray = metrics.Select(metric => metric).ToArray();
            IExchangeTradedCurve futuresCurve = null;
            IRateCurve discountCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                discountCurve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
                futuresCurve = (IExchangeTradedCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                FuturesCurveName = futuresCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                futuresCurve = (IExchangeTradedCurve)modelData.MarketEnvironment.GetPricingStructure(FuturesCurveName);
                discountCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(DiscountCurveName);
            }
            var settlementDF = 1.0;
            if (discountCurve != null)
            {
                settlementDF = discountCurve.GetDiscountFactor(SettlementDate);
            }
            //var interval = FuturesTypeInfo.;
            AnalyticModelParameters.SettlementDiscountFactor = Convert.ToDecimal(settlementDF);
            AnalyticModelParameters.Multiplier = Multiplier;
            AnalyticModelParameters.AccrualPeriod = 0.25m;
            AnalyticModelParameters.NumberOfContracts = NumberOfContracts;
            AnalyticModelParameters.TradePrice = PurchasePrice.amount;
            AnalyticModelParameters.ContractNotional = Convert.ToDecimal(FuturesTypeInfo.Future.multiplier);
            //Get the discount factor to the settlement date
            if (futuresCurve != null)
                AnalyticModelParameters.Quote = Convert.ToDecimal(futuresCurve.GetForward(LastTradeDate));
            AnalyticResults =
                AnalyticsModel.Calculate<IFuturesAssetResults, FuturesAssetResults>(AnalyticModelParameters,
                    metricsAsArray);
            // store inputs and results from this run
            CalculationPerformedIndicator = true;
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IFuturesAssetResults CalculationResults { get; protected set; }

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
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
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
