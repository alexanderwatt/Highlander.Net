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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Schedulers;
using Orion.Constants;
using Orion.Models.ForeignExchange.FxOption;
using Orion.Models.ForeignExchange.FxOptionLeg;
using Orion.Util.Logging;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework.Instruments.ForeignExchange;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ValuationEngine.Factory;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    [Serializable]
    public class VanillaEuropeanFxOptionPricer : InstrumentControllerBase, IPriceableFxLeg<IFxOptionLegParameters, IFxOptionLegInstrumentResults>, IPriceableInstrumentController<FxOption>
    {
        #region Member Fields

        protected const string CModelIdentifier = "VanillaFxOption";
        //protected const string CDefaultBucketingInterval = "3M";

        protected IDayCounter CDefaultDayCounter = Actual365.Instance;

        // BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        #endregion

        #region Public Fields

        public bool IsCall()
        {
            return StrikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency;
        }

        // Analytics
        public IModelAnalytic<IFxOptionLegParameters, FxOptionLegInstrumentMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// The HybridValuation flag.
        /// </summary>
        public bool HybridValuation { get; protected set; }

        /// <summary>
        /// The option type.
        /// </summary>
        public FxOptionType FxOptionType { get; set; }

        /// <summary>
        /// The fx option strike rate.
        /// </summary>
        public decimal FxStrike { get; set; }

        /// <summary>
        /// The fpml product type.
        /// </summary>
        public string FpMLProductType { get; set; }

        /// <summary>
        /// The fx option strike quote basis.
        /// </summary>
        public StrikeQuoteBasisEnum StrikeQuoteBasis { get; set; }

        /// <summary>
        /// The fx option strike quote basis.
        /// </summary>
        public PutCallEnum SoldAs { get; set; }

        /// <summary>
        /// The cut name.
        /// </summary>
        public CutName CutName { get; set; }

        /// <summary>
        /// The exercise information.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// The expiry time.
        /// </summary>
        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// The expiry time business center.
        /// </summary>
        public string ExpiryTimeBusinessCenter { get; set; }

        /// <summary>
        /// The time to expiry.
        /// </summary>
        public Decimal TimeToExpiry { get; set; }

        /// <summary>
        /// The cash flow value date.
        /// </summary>
        public DateTime ValueDate { get; set; }

        /// <summary>
        /// The QuotedTenor.
        /// </summary>
        public string QuotedTenor { get; set; }

        /// <summary>
        /// The has been exercised flag.
        /// </summary>
        public bool HasBeenExercised { get; set; }

        /// <summary>
        /// Gets the seller party reference.
        /// </summary>
        /// <value>The seller party reference.</value>
        public string SellerPartyReference { get; set; }

        /// <summary>
        /// Gets the buyer party reference.
        /// </summary>
        /// <value>The buyer party reference.</value>
        public string BuyerPartyReference { get; set; }

        /// <summary>
        /// The put currency amount.
        /// </summary>
        public decimal PutCurrencyAmount { get; set; }

        /// <summary>
        /// The put currency.
        /// </summary>
        public string PutCurrency { get; set; }

        /// <summary>
        /// The call currency amount.
        /// </summary>
        public decimal CallCurrencyAmount { get; set; }

        /// <summary>
        /// The call currency.
        /// </summary>
        public string CallCurrency { get; set; }

        /// <summary>
        /// The volatility surface name to use.
        /// </summary>
        public string VolatilitySurfaceName { get; set; }

        /// <summary>
        /// Gets a value indicating whether [base party selling the option].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party selling the option]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyBuyer { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IFxOptionLegParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IFxOptionLegInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// Gets the fx leg if expired.
        /// </summary>
        /// <value>The fx leg.</value>
        public FxSingleLegPricer FxLeg{ get; protected set; }

        /// <summary>
        /// Gets the underlying fx option virtual cash flow..
        /// </summary>
        /// <value>The underlying fx option.</value>
        public PriceableVanillaFxOption FxOptionUnderlyer { get; protected set; }

        /// <summary>
        /// Gets the premia.
        /// </summary>
        /// <value>The premia.</value>
        public List<PriceableFxOptionPremium> Premia { get; protected set; }

        /// <summary>
        /// IsCashSettled
        /// </summary>
        public bool IsCashSettled { get; set; }

        /// <summary>
        /// The call currency amount.
        /// </summary>
        public decimal? SpotRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? FixingDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public QuotedCurrencyPair FixingQuotedCurrencyPair { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Currency CashSettlementCurrency { get; set; }

        #endregion

        #region Constructors

        public  VanillaEuropeanFxOptionPricer()
        {
            AnalyticsModel = new FxOptionLegAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VanillaEuropeanFxOptionPricer"/> class.  All the cashflows must be signed.
        /// </summary>
        /// <param name="paymentCalendar">The payment calendar</param>
        /// <param name="fxOptionLeg">The fxLeg.</param>
        /// <param name="baseParty">The the base party.</param>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="fixingCalendar">The fixing calendar</param>
        public VanillaEuropeanFxOptionPricer(ILogger logger, ICoreCache cache,
            String nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, FxOption fxOptionLeg, string baseParty)
        {
            OrderedPartyNames = new List<string>();
            Multiplier = 1.0m;
            Id = fxOptionLeg.id;
            AnalyticsModel = new FxOptionLegAnalytic();
            ProductType = ProductTypeSimpleEnum.FxOption;
            HybridValuation = true;
            BuyerPartyReference = fxOptionLeg.buyerPartyReference.href;
            SellerPartyReference = fxOptionLeg.sellerPartyReference.href;
            BasePartyBuyer = baseParty == BuyerPartyReference;
            FxOptionType = FxOptionType.Put;
            ProductType = ProductTypeSimpleEnum.FxOption;
            if (fxOptionLeg.spotRateSpecified)
            {
                SpotRate = fxOptionLeg.spotRate;
            }
            FpMLProductType = ProductTypeHelper.GetProductType(fxOptionLeg.Items, fxOptionLeg.ItemsElementName);
            //Temporary values used to build the fx and option trades.
            PutCurrencyAmount = fxOptionLeg.putCurrencyAmount.amount;
            PutCurrency = fxOptionLeg.callCurrencyAmount.currency.Value;
            CallCurrencyAmount = fxOptionLeg.callCurrencyAmount.amount;
            CallCurrency = fxOptionLeg.putCurrencyAmount.currency.Value;
            FxStrike = fxOptionLeg.strike.rate;
            StrikeQuoteBasis = fxOptionLeg.strike.strikeQuoteBasis;
            if (IsCall())
            {
                FxOptionType = FxOptionType.Call;
            }
            SoldAs = fxOptionLeg.soldAs;
            if (fxOptionLeg.tenorPeriod != null)
            {
                QuotedTenor = fxOptionLeg.tenorPeriod.ToString();
            }
            var vanillaOption = fxOptionLeg.Item as FxEuropeanExercise;
            if (vanillaOption != null)
            {
                ExpiryDate = vanillaOption.expiryDate;
                ExpiryTimeBusinessCenter = vanillaOption.expiryTime.businessCenter.Value;
                ExpiryTime = vanillaOption.expiryTime.hourMinuteTime; //TODO Not Implemented yet!
                CutName = vanillaOption.cutName;
                ValueDate = vanillaOption.valueDate;
            }
            var type = HasBeenExercised ? ProductTypeSimpleEnum.FxSpot : ProductTypeSimpleEnum.FxForward;
            //Create the fxleg. 
            //TODO Currently this does not handle non-deliverable forwards or a third settlement currency.
            var fxLeg = FxOption.CreateFxSingleLeg(HasBeenExercised, BuyerPartyReference, SellerPartyReference, PutCurrencyAmount, PutCurrency, CallCurrencyAmount,
                CallCurrency, StrikeQuoteBasis, ValueDate, FxStrike);
            FxLeg = new FxSingleLegPricer(fxLeg, baseParty, type);
            VolatilitySurfaceName = fxOptionLeg.GetRequiredVolatilitySurfaces()[0];
            //FxOptionType;
            //Get the currency.
            PaymentCurrencies = new List<string>();
            PaymentCurrencies.AddRange(FxLeg.PaymentCurrencies);
            //Add the premia
            Premia = new List<PriceableFxOptionPremium>();
            if (fxOptionLeg.premium != null)
            {
                foreach (var premium in fxOptionLeg.premium)
                {
                    var priceablePayment = PriceableInstrumentsFactory.CreatePriceableFxOptionPremium(cache, nameSpace, null, baseParty, premium, fixingCalendar, paymentCalendar);
                    Premia.Add(priceablePayment);
                    PaymentCurrencies.AddRange(priceablePayment.PaymentCurrencies);
                }
            }
            PaymentCurrencies = PaymentCurrencies.Distinct().ToList();
            if (fxOptionLeg.cashSettlement != null)
            {
                IsCashSettled = true;
                CashSettlementCurrency = fxOptionLeg.cashSettlement.settlementCurrency;
                //Only a single fixing date is currency implemented.
                if (fxOptionLeg.cashSettlement.fixing != null && fxOptionLeg.cashSettlement.fixing[0].fixingDateSpecified)
                {
                    FixingDate = fxOptionLeg.cashSettlement.fixing[0].fixingDate;
                    FixingQuotedCurrencyPair = fxOptionLeg.cashSettlement.fixing[0].quotedCurrencyPair;
                }
            }
            if (vanillaOption != null) RiskMaturityDate = vanillaOption.valueDate;
            logger.LogInfo("FxOption trade created :");
        }

        #endregion
         
        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public FxOption Build()
        {
            var fxOptionLeg = CreateVanillaOption(BuyerPartyReference, SellerPartyReference, null,
                QuotedTenor, ExpiryDate, ExpiryTime, ExpiryTimeBusinessCenter, CutName, PutCurrencyAmount, PutCurrency,
                CallCurrencyAmount, CallCurrency, StrikeQuoteBasis, ValueDate, FxStrike, SpotRate, IsCashSettled, CashSettlementCurrency, FixingDate, FixingQuotedCurrencyPair, null);
            return fxOptionLeg;
        }

        public static Trade CreateVanillaFxOption(string tradeId, DateTime tradeDate, string buyerPartyReference, string sellerPartyReference, //FxOptionType optionType,
            PutCallEnum? soldAs, string period, DateTime expiryDate, DateTime time, string expiryBusinessCenter,
            CutName cutName, decimal putCurrencyAmount, string putCurrency, decimal callCurrencyAmount,
            string callCurrency, StrikeQuoteBasisEnum strikeQuoteBasis, DateTime valueDate, Decimal strikePrice, decimal? spotRate,
            bool isCashSettled, Currency settlementCurrency, DateTime? fixingDate, QuotedCurrencyPair quotedCurrencyPair, List<FxOptionPremium> premia)
        {
            var trade = new Trade { id = tradeId, tradeHeader = new TradeHeader() };
            var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
            var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
            FxOption fxOption = CreateVanillaOption(buyerPartyReference, sellerPartyReference, soldAs,
            period, expiryDate, time, expiryBusinessCenter, cutName, putCurrencyAmount, putCurrency, callCurrencyAmount, callCurrency,
            strikeQuoteBasis, valueDate, strikePrice, spotRate, isCashSettled, settlementCurrency, fixingDate, quotedCurrencyPair, premia);
            FpMLFieldResolver.TradeSetFxOptionLeg(trade, fxOption);
            return trade;
        }

        /// <summary>
        /// Creates a vanilla fx option.
        /// </summary>
        /// <param name="buyerPartyReference"></param>
        /// <param name="sellerPartyReference"></param>
        /// <param name="soldAs"></param>
        /// <param name="period"> </param>
        /// <param name="expiryDate"></param>
        /// <param name="expiryBusinessCenter"> </param>
        /// <param name="cutName"> </param>
        /// <param name="isCashSettled"></param>
        /// <param name="settlementCurrency"></param>
        /// <param name="fixingDate"></param>
        /// <param name="quotedCurrencyPair"></param>
        /// <param name="putCurrencyAmount"></param>
        /// <param name="putCurrency"></param>
        /// <param name="callCurrencyAmount"></param>
        /// <param name="spotRate"></param>
        /// <param name="callCurrency"></param>
        /// <param name="strikeQuoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="strikePrice"></param>
        /// <param name="premia"></param>
        /// <param name="time"> </param>
        /// <returns></returns>
        public static FxOption CreateVanillaOption(string buyerPartyReference, string sellerPartyReference,  PutCallEnum? soldAs, string period, 
            DateTime expiryDate, DateTime time, string expiryBusinessCenter, CutName cutName, decimal putCurrencyAmount, string putCurrency, 
            decimal callCurrencyAmount, string callCurrency, StrikeQuoteBasisEnum strikeQuoteBasis, DateTime valueDate, Decimal strikePrice, decimal? spotRate, 
            bool isCashSettled, Currency settlementCurrency, DateTime? fixingDate, QuotedCurrencyPair quotedCurrencyPair, List<FxOptionPremium> premia)
        {
            CutName cut = null;
            if (cutName != null)
            {
                cut = new CutName { Value = cutName.Value };
            }
            var expiryDateTime = new FxEuropeanExercise
            {
                expiryDate = expiryDate,
                expiryTime =
                    new BusinessCenterTime
                    {
                        hourMinuteTime = time,
                        businessCenter = new BusinessCenter { Value = expiryBusinessCenter }
                    },
                cutName = cut,
                valueDate = valueDate,
                valueDateSpecified = true
            };
            FxOptionPremium[] premiumValues = null;
            if(premia != null)
            {
                premiumValues = premia.ToArray();
            }
            var fxOption = new FxOption
            {
                Items = new object[] { new ProductType { Value = ProductTypeSimpleEnum.FxOption.ToString() } },
                ItemsElementName = new[] { ItemsChoiceType2.productType },
                putCurrencyAmount = MoneyHelper.GetNonNegativeAmount(putCurrencyAmount, putCurrency),
                callCurrencyAmount = MoneyHelper.GetNonNegativeAmount(callCurrencyAmount, callCurrency),
                strike =
                    new FxStrikePrice { rate = strikePrice, rateSpecified = true, strikeQuoteBasis = strikeQuoteBasis, strikeQuoteBasisSpecified = true},
                buyerPartyReference = PartyReferenceFactory.Create(buyerPartyReference),
                sellerPartyReference =
                    PartyReferenceFactory.Create(sellerPartyReference),
                premium = premiumValues,
                Item = expiryDateTime,
            };
            if (spotRate != null)
            {
                fxOption.spotRate = (decimal)spotRate;
                fxOption.spotRateSpecified = true;
            }
            if(period!=null)
            {
                var tenorPeriod = PeriodHelper.Parse(period);
                fxOption.tenorPeriod = tenorPeriod;
            }
            if (soldAs != null)
            {
                fxOption.soldAs = (PutCallEnum)soldAs;
                fxOption.soldAsSpecified = true;
            }
            if (isCashSettled)
            {
                fxOption.cashSettlement = new FxCashSettlement {settlementCurrency = settlementCurrency};
                var fxFixing = new FxFixing();
                if (fixingDate != null)
                {
                    fxFixing.fixingDate = (DateTime) fixingDate;
                    fxFixing.fixingDateSpecified = true;
                }
                fxFixing.quotedCurrencyPair = quotedCurrencyPair;
                fxOption.cashSettlement.fixing = new [] { fxFixing };
            }
            return fxOption;
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
                AnalyticsModel = new FxOptionLegAnalytic();
            }
            var marketEnvironment = modelData.MarketEnvironment as IFxLegEnvironment;
            AssetValuation streamValuation;
            //Check if risk calc are required.
            bool delta0PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta0PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta0PDH.ToString()) != null;
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var childValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            foreach (var payment in FxLeg.Payments)
            {
                payment.Multiplier = Multiplier;
                payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                payment.BucketedDates = BucketedDates;
            }
            foreach (var premium in Premia)
            {
                premium.Multiplier = 1.0m;
                premium.PricingStructureEvolutionType = PricingStructureEvolutionType;
                premium.BucketedDates = BucketedDates;
            }
            if (marketEnvironment != null)
            {
                //Modify the second market.
                var modelData1 = new InstrumentControllerData(modelData.AssetValuation,
                                                              marketEnvironment.GetExchangeCurrencyPaymentEnvironment1(),
                                                              modelData.ValuationDate, modelData.ReportingCurrency);
                var modelData2 = new InstrumentControllerData(modelData.AssetValuation,
                                                              marketEnvironment.GetExchangeCurrencyPaymentEnvironment2(),
                                                              modelData.ValuationDate, modelData.ReportingCurrency);
                childValuations.Add(Currency1Payment.Calculate(modelData1));
                childValuations.Add(Currency2Payment.Calculate(modelData2));
                childValuations.AddRange(Premia.Select(premium => premium.Calculate(modelData2)));
            }
            else if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                var market = (MarketEnvironment)modelData.MarketEnvironment;
                if (delta0PDH)
                {
                    //    //Force building of the risk curves.
                    //    market.SearchForPerturbedPricingStructures(FxIndexCurveName, "delta0PDH");//TODO Need to add this perturbation to fxCurve.
                }
                if (delta1PDH)
                {
                    //Force building of the risk curves.
                    market.SearchForPerturbedPricingStructures(FxLeg.Currency1DiscountCurveName, "delta1PDH");
                    market.SearchForPerturbedPricingStructures(FxLeg.Currency2DiscountCurveName, "delta1PDH");
                }
                childValuations = EvaluateChildMetrics(GetChildren().ToList(), modelData, Metrics);
                //childValuations.Add(Currency1Payment.Calculate(modelData));
            }
            var paymentValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            var childControllerValuations = new List<AssetValuation> {paymentValuation};
            TimeToExpiry = GetPaymentYearFraction(ModelData.ValuationDate, ExpiryDate);
            var volatilityCurveNodeTime = GetPaymentYearFraction(ModelData.ValuationDate, ValueDate);
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                if (modelData.MarketEnvironment is MarketEnvironment market)
                {
                    var fxCurve = (IFxCurve)market.SearchForPricingStructureType(FxLeg.FxIndexCurveName);
                    var indexSurface = (IVolatilitySurface)market.SearchForPricingStructureType(VolatilitySurfaceName);
                    if (HybridValuation)
                    {
                        var curve1 = (IRateCurve)market.SearchForPricingStructureType(FxLeg.Currency1DiscountCurveName);
                        var curve2 = (IRateCurve)market.SearchForPricingStructureType(FxLeg.Currency2DiscountCurveName);
                        var flag = FxLeg.ExchangeRate.quotedCurrencyPair.quoteBasis == QuoteBasisEnum.Currency2PerCurrency1;
                        AnalyticsModel = new FxOptionLegAnalytic(modelData.ValuationDate, RiskMaturityDate, fxCurve, curve1,
                                                           curve2, flag, FxStrike, TimeToExpiry, volatilityCurveNodeTime, indexSurface, FxOptionType);
                    }
                    else
                    {
                        AnalyticsModel = new FxOptionLegAnalytic(modelData.ValuationDate, RiskMaturityDate, fxCurve, FxStrike, TimeToExpiry, volatilityCurveNodeTime, indexSurface, FxOptionType);
                    }
                }
                decimal? premium = null;
                if(Premia.Count > 1)
                {
                    premium = Premia[0].PaymentAmount.amount; // We assume only a single premium in the basic case.
                }
                IFxOptionLegParameters analyticModelParameters = new FxOptionLegParameters
                                                                {
                                                                    Premium = premium,
                                                                };
                CalculationResults =
                    AnalyticsModel.Calculate<IFxOptionLegInstrumentResults, FxOptionLegInstrumentResults>(
                        analyticModelParameters, streamControllerMetrics.ToArray());

                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                        childControllerValuations,
                                                                        ConvertMetrics(streamControllerMetrics),
                                                                        new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            }
            else
            {
                streamValuation = paymentValuation;
            }
            CalculationPerformedIndicator = true;
            streamValuation.id = Id;
            return streamValuation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="bucketInterval"></param>
        /// <returns></returns>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            //DateTime endDate = FixedCoupon.AccrualEndDate;
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

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = FxLeg.GetChildren();
            foreach(var premium in Premia)
            {
                children.Add(premium);
            }
            return children;
        }

        #endregion

        #region Implementation of IPriceableFxLeg<IFxOptionLegParameters,IFxOptionLegInstrumentResults>

        /// <summary>
        /// Gets or sets the payment in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        public InstrumentControllerBase Currency1Payment => FxLeg.Currency1Payment;

        /// <summary>
        /// Gets or sets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        public InstrumentControllerBase Currency2Payment => FxLeg.Currency2Payment;

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="currency1CurveName">New name of the currecny discount curve.</param>
        /// <param name="currency2CurveName">New name of the currecny discount curve.</param>
        public void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName)
        {
            FxLeg.UpdateDiscountCurveNames(currency1CurveName, currency2CurveName);
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        protected Decimal GetPaymentYearFraction(DateTime startDate, DateTime endDate)
        {
            return (Decimal)CDefaultDayCounter.YearFraction(startDate, endDate);
        }

        #endregion
    }
}