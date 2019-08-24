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
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
using Orion.Analytics.Schedulers;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework.Assets;
using Orion.Models.Rates.Bonds;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Analytics.Interpolations.Points;
using Orion.CurveEngine.Assets;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Models.Assets;
using Orion.Util.Serialisation;
using Orion.ValuationEngine.Factory;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class BondTransactionPricer : InstrumentControllerBase, IPriceableBondTransaction<IBondTransactionParameters, IBondTransactionResults>, IPriceableInstrumentController<BondTransaction>
    {
        #region Properties

        private const decimal CDefaultWeightingValue = 1.0m;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public BasicQuotation MarketQuote { get; set; }

        /// <summary>
        /// Gets a value indicating whether [base party buyer].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party buyer]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyBuyer { get; set; }

        /// <summary>
        /// The bond valuation curve.
        /// </summary>
        public string BondCurveName { get; set; }

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
        /// The bond price information
        /// </summary>
        public BondPrice BondPrice { get; set; }

        /// <summary>
        /// The settlement day convention.
        /// </summary>
        public  RelativeDateOffset SettlementDateConvention { get; set; }

        /// <summary>
        /// The ex-dividend date convention.
        /// </summary>
        public RelativeDateOffset ExDivDateConvention { get; set; }

        /// <summary>
        /// The business day adjustments for bond coupon and final exchange payments
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        ///<summary>
        ///</summary>
        public bool IsYTMQuote { get; set; }

        ///<summary>
        ///</summary>
        public decimal QuoteValue { get; set; }

        /// <summary>
        /// The bond details.
        /// </summary>
        public Bond Bond { get; set; }

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
        /// The final redemption amount
        /// </summary>
        public PriceablePayment FinalRedemption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IBondTransactionParameters, BondMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? 
        /// AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), 
        /// AccrualBusinessDayAdjustments);  
        /// </summary>
        public bool ForecastRateInterpolation { get; set; }

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
        public IBondTransactionParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public PriceableBondCouponRateStream Coupons = new PriceableBondCouponRateStream();

        #endregion

        #region Constructors

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
            BondTransaction bondFpML, string basePartyReference, string bondType, bool forecastRateInterpolation)
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
            //Get the instrument configuration information.
            var assetIdentifier = bondFpML.bond.currency.Value + "-Bond-" + BondType;
            BondNodeStruct bondTypeInfo = null;
            //Set the curve to use for valuations.
            BondCurveName = CurveNameHelpers.GetBondCurveName(Bond.currency.Value, Bond.id);
            //TODO Set the swap curves for asset swap valuation.
            //
            //Gets the template bond type
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
            if (instrument != null)
            {
                bondTypeInfo = instrument.InstrumentNodeItem as BondNodeStruct;
            }
            if (bondFpML.bond != null && bondTypeInfo != null)
            {
                if (SettlementCalendar == null)
                {
                    SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, bondTypeInfo.SettlementDate.businessCenters, nameSpace);
                }
                if (PaymentCalendar == null)
                {
                    PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, bondTypeInfo.BusinessDayAdjustments.businessCenters, nameSpace);
                }
                //Pre-processes the data for the priceable asset.
                var bond = XmlSerializerHelper.Clone(bondFpML.bond);
                Bond = bond;
                if (bond.maturitySpecified)
                {
                    MaturityDate = bond.maturity;
                }
                SettlementDateConvention = bondTypeInfo.SettlementDate;
                BusinessDayAdjustments = bondTypeInfo.BusinessDayAdjustments;
                ExDivDateConvention = bondTypeInfo.ExDivDate;
                //This is done because the config data is not stored in the correct way. Need to add a price quote units.
                if (bond.couponRateSpecified)
                {
                    var coupon = bond.couponRate;
                    Bond.couponRate = coupon;
                }

                bondTypeInfo.Bond.faceAmount = NotionalAmount.amount;
                bondTypeInfo.Bond.faceAmountSpecified = true;
                Bond.faceAmount = NotionalAmount.amount;
                if (Bond.maturitySpecified)
                {
                    RiskMaturityDate = Bond.maturity;
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
            //Set the underlying bond
            UnderlyingBond = new PriceableSimpleBond(tradeDate, bondTypeInfo, SettlementCalendar, PaymentCalendar, Quote, QuoteType);
            //Set the coupons
            var bondId = Bond.id;//Could use one of the instrumentIds
            //bondStream is an interest Rate Stream but needs to be converted to a bond stream.
            Coupons = new PriceableBondCouponRateStream(logger, cache, nameSpace, bondId, tradeDate,
                bondFpML.notionalAmount.amount, CouponStreamType.GenericFixedRate, Bond,
                BusinessDayAdjustments, ForecastRateInterpolation, null, PaymentCalendar);
            //Add payments like the settlement price
            if (!BondPrice.dirtyPriceSpecified) return;
            var amount = BondPrice.dirtyPrice * NotionalAmount.amount / 100;
            var settlementPayment = PaymentHelper.Create("BondSettlementAmount", BuyerReference, SellerReference, amount, SettlementDate);
            AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] { settlementPayment }, SettlementCalendar);
            //
            var finalPayment = PaymentHelper.Create("FinalRedemption", BuyerReference, SellerReference, NotionalAmount.amount, RiskMaturityDate);
            FinalRedemption =
                PriceableInstrumentsFactory.CreatePriceablePayment(basePartyReference, finalPayment, PaymentCalendar);
            AdditionalPayments.Add(FinalRedemption);
            if (!PaymentCurrencies.Contains(settlementPayment.paymentAmount.currency.Value))
            {
                PaymentCurrencies.Add(settlementPayment.paymentAmount.currency.Value);
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and returns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public BondTransaction Build()
        {
            var bond = Bond;
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
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precedence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new BondTransactionAnalytic();
            }
            var bondControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation bondValuation;
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.AccrualFactor.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.AccrualFactor.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.FloatingNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.FloatingNPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            ModelData.AssetValuation.quote = quotes.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve rateDiscountCurve = null;
            //2. Sets the evolution type for coupon and payment calculations.
            Coupons.PricingStructureEvolutionType = PricingStructureEvolutionType;
            Coupons.BucketedDates = BucketedDates;
            Coupons.Multiplier = Multiplier;
            if (AdditionalPayments != null)
            {
                foreach (var payment in AdditionalPayments)
                {
                    payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    payment.BucketedDates = BucketedDates;
                    payment.Multiplier = Multiplier;
                }
            }
            //3. Aggregate the child metrics.
            List<AssetValuation> childValuations = new List<AssetValuation> {Coupons?.Calculate(modelData)};
            if (GetAdditionalPayments() != null)
            {
                var paymentControllers = new List<InstrumentControllerBase>(GetAdditionalPayments());
                childValuations.AddRange(paymentControllers.Select(payment => payment.Calculate(modelData)));
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);
            childControllerValuations.id = Id + ".BondCouponRateStreams";
            //4. Now do the bond calculations.
            if (bondControllerMetrics.Count > 0)
            {
                CalculationResults = new BondTransactionResults();
                if (marketEnvironment.GetType() == typeof(MarketEnvironment))
                {
                    var bondCurve = (IBondCurve)modelData.MarketEnvironment.GetPricingStructure(BondCurveName);
                    if (bondCurve != null)
                    {
                        var marketDataType =
                            bondCurve.GetPricingStructureId().Properties.GetValue<string>(AssetMeasureEnum.MarketQuote.ToString(), false);
                        if (marketDataType != null && marketDataType == BondPriceEnum.YieldToMaturity.ToString())
                        {
                            IsYTMQuote = true;
                        }
                        //TODO handle the other cases like: AssetSwapSpread; DirtyPrice and ZSpread.
                        var mq = (decimal)bondCurve.GetYieldToMaturity(modelData.ValuationDate, SettlementDate);
                        Quote = BasicQuotationHelper.Create(mq, AssetMeasureEnum.MarketQuote.ToString(),
                                                            PriceQuoteUnitsEnum.DecimalRate.ToString());
                    }
                    rateDiscountCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(BondCurveName);//SwapCurve
                }
                //Generate the vectors
                //var accrualFactorArray = GetCouponAccrualFactors();
                const bool isBuyerInd = true;
                var analyticModelParameters = new BondTransactionParameters
                {
                    IsBuyerInd = isBuyerInd,
                    AccrualYearFractions = GetCouponAccrualFactors(),
                    Multiplier = Multiplier,
                    Quote = QuoteValue,
                    CouponRate = UnderlyingBond.GetCouponRate(),
                    NotionalAmount = UnderlyingBond.Notional,
                    Frequency = UnderlyingBond.Frequency,
                    IsYTMQuote = IsYTMQuote,
                    AccruedFactor = UnderlyingBond.GetAccruedFactor(),
                    RemainingAccruedFactor = UnderlyingBond.GetRemainingAccruedFactor(),
                    PaymentDiscountFactors =
                        GetDiscountFactors(rateDiscountCurve, Coupons.StreamPaymentDates.ToArray(), modelData.ValuationDate),
                };
                //5. Get the Weightings
                analyticModelParameters.Weightings =
                    CreateWeightings(CDefaultWeightingValue, analyticModelParameters.PaymentDiscountFactors.Length);
                //6. Set the analytic input parameters and Calculate the respective metrics 
                AnalyticModelParameters = analyticModelParameters;
                CalculationResults = AnalyticsModel.Calculate<IBondTransactionResults, BondTransactionResults>(analyticModelParameters, bondControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var bondControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                bondValuation = AssetValuationHelper.UpdateValuation(bondControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(bondControllerMetrics), new List<string>(Metrics));
            }
            else
            {
                bondValuation = childControllerValuations;
            }
            // store inputs and results from this run
            CalculationPerformedIndicator = true;
            bondValuation.id = Id;
            return bondValuation;
        }

        //public override AssetValuation Calculate(IInstrumentControllerData modelData)
        //{
        //    ModelData = modelData;
        //    AnalyticModelParameters = null;
        //    AnalyticsModel = new BondTransactionAnalytic();
        //    //1. Create the bond
        //    var bondTypeInfo = new BondNodeStruct
        //    {
        //        Bond = Bond,
        //        SettlementDate = SettlementDateConvention,
        //        BusinessDayAdjustments = BusinessDayAdjustments,
        //        ExDivDate = ExDivDateConvention
        //    };
        //    UnderlyingBond = new PriceableSimpleBond(modelData.ValuationDate, bondTypeInfo, SettlementCalendar, PaymentCalendar, Quote, QuoteType);
        //    if (BondPrice.dirtyPriceSpecified)
        //    {
        //        UnderlyingBond.PurchasePrice = BondPrice.dirtyPrice / 100; //PriceQuoteUnits
        //    }
        //    //Setting other relevant information          
        //    MaturityDate = UnderlyingBond.MaturityDate;
        //    var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
        //    var metricsAsString = metrics.Select(metric => metric.ToString()).ToList();
        //    var controllerData = PriceableAssetFactory.CreateAssetControllerData(metricsAsString.ToArray(), modelData.ValuationDate, modelData.MarketEnvironment);
        //    UnderlyingBond.Multiplier = Multiplier;
        //    UnderlyingBond.Calculate(controllerData);
        //    // store inputs and results from this run
        //    AnalyticModelParameters = ((PriceableBondAssetController)UnderlyingBond).AnalyticModelParameters;
        //    AnalyticsModel = ((PriceableBondAssetController)UnderlyingBond).AnalyticsModel;
        //    CalculationResults = ((PriceableBondAssetController)UnderlyingBond).CalculationResults;
        //    CalculationPerformedIndicator = true;
        //    return GetValue(CalculationResults, modelData.ValuationDate);
        //}

        private decimal[] GetCouponAccrualFactors()
        {
            var result = Coupons.GetCouponAccrualFactors();
            return result.ToArray();
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IBondTransactionResults CalculationResults { get; protected set; }

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

        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public static decimal[] GetDiscountFactors(IRateCurve discountFactorCurve, DateTime[] periodDates,
                                            DateTime valuationDate)
        {
            return periodDates.Select(periodDate => GetDiscountFactor(discountFactorCurve, periodDate, valuationDate)).ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public static decimal GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="marketQuote">The marketQuote.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (string.Compare(marketQuote.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Quote = marketQuote;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
        }

        /// <summary>
        /// Creates the weightings.
        /// </summary>
        /// <param name="weightingValue">The weighting value.</param>
        /// <param name="noOfInstances">The no of instances.</param>
        /// <returns></returns>
        private static decimal[] CreateWeightings(Decimal weightingValue, int noOfInstances)
        {
            var weights = new List<decimal>();
            for (var index = 0; index < noOfInstances; index++)
            {
                weights.Add(weightingValue);
            }
            return weights.ToArray();
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
                if (value.quoteUnits.Value == "DecimalRate")
                {
                    IsYTMQuote = true;
                    QuoteValue = value.value;
                }
                if (value.quoteUnits.Value == "Rate")
                {
                    IsYTMQuote = true;
                    QuoteValue = value.value / 100.0m;
                }
                if (value.quoteUnits.Value == "DirtyPrice")
                {
                    IsYTMQuote = false;
                    QuoteValue = value.value / 100.0m;
                }
                if (value.quoteUnits.Value == "CleanPrice")
                {
                    IsYTMQuote = false;
                    QuoteValue = value.value / 100.0m;
                }
            }
        }

        ///<summary>
        ///</summary>
        public DateTime GetMaturityDate()
        {
            return MaturityDate;
        }

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
