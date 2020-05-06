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
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.Models.V5r3.Rates.Bonds;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Reporting.V5r3;
using Highlander.Constants;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.CurveEngine.V5r3.Assets.Rates.Bonds;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.Reporting.ModelFramework.V5r3.Instruments.InterestRates;
using Highlander.Reporting.ModelFramework.V5r3.Instruments.Lease;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.Utilities.Serialisation;
using Highlander.ValuationEngine.V5r3.Factory;
using Highlander.ValuationEngine.V5r3.Instruments;

#endregion

namespace Highlander.ValuationEngine.V5r3.Pricers
{
    public class LeaseTransactionPricer : InstrumentControllerBase, IPriceableLeaseTransaction<ILeaseTransactionParameters, ILeaseTransactionResults>, IPriceableInstrumentController<LeaseTransaction>
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

        public DateTime PaymentDate { get; }

        /// <summary>
        /// The bond issuer code
        /// </summary>
        public string LeaseTenant { get; set; }

        /// <summary>
        /// The bond valuation curve.
        /// </summary>
        public string LeaseCurveName { get; set; }

        /// <summary>
        /// The payment currency.
        /// </summary>
        public Currency PaymentCurrency { get; set; }

        /// <summary>
        /// The coupon currency.
        /// </summary>
        public Currency CouponCurrency { get; set; }

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
        /// The type of lease: 
        /// Standard,
        /// Other.
        /// </summary>
        public string LeaseType { get; set; }

        /// <summary>
        /// The notional amount
        /// </summary>
        public Money NotionalAmount { get; set; }

        /// <summary>
        /// The bond price information
        /// </summary>
        public BondPrice LeasePrice { get; set; }

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
        public decimal QuoteValue { get; set; }

        /// <summary>
        /// The lease details.
        /// </summary>
        public Lease Lease { get; set; }

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
        public IPriceableLeaseAssetController UnderlyingLease { get; set; }

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
        public IModelAnalytic<ILeaseTransactionParameters, LeaseMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? 
        /// AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), 
        /// AccrualBusinessDayAdjustments);  
        /// </summary>
        public bool ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Lease";
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
        public ILeaseTransactionParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public PriceableLeaseCouponRateStream Coupons = new PriceableLeaseCouponRateStream();

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
        /// <param name="leaseFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="leaseType"></param>
        /// <param name="forecastRateInterpolation"></param>
        public LeaseTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate,
            DateTime settlementDate, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar,
            LeaseTransaction leaseFpML, string basePartyReference, string leaseType, bool forecastRateInterpolation)
        {
            Multiplier = 1.0m;
            TradeDate = tradeDate;
            LeaseType = leaseType;
            logger.LogInfo("LeaseType set. Commence to build a bond transaction.");
            if (leaseFpML == null) return;
            BuyerReference = leaseFpML.buyerPartyReference.href;
            PaymentCurrencies = new List<string> { leaseFpML.notionalAmount.currency.Value};
            SellerReference = leaseFpML.sellerPartyReference.href;
            BasePartyBuyer = basePartyReference == leaseFpML.buyerPartyReference.href;
            if (!BasePartyBuyer)
            {
                Multiplier = -1.0m;
            }
            ForecastRateInterpolation = forecastRateInterpolation;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = paymentCalendar;
            //Set the bond price information
            BondPrice = new BondPrice();
            if (leaseFpML.price.accrualsSpecified)
            {
                BondPrice.accrualsSpecified = true;
                BondPrice.accruals = leaseFpML.price.accruals;
            }
            if (leaseFpML.price.dirtyPriceSpecified)
            {
                BondPrice.dirtyPriceSpecified = true;
                BondPrice.dirtyPrice = leaseFpML.price.dirtyPrice;
            }
            BondPrice.cleanOfAccruedInterest = leaseFpML.price.cleanOfAccruedInterest;
            BondPrice.cleanPrice = leaseFpML.price.cleanPrice;
            //Set the currencies
            CouponCurrency = leaseFpML.notionalAmount.currency;
            PaymentCurrency = leaseFpML.notionalAmount.currency;//This could be another currency!
            //Set the notional information
            NotionalAmount = MoneyHelper.GetAmount(leaseFpML.notionalAmount.amount, leaseFpML.notionalAmount.currency.Value);
            //Determines the quotation and units
            QuoteType = BondPriceEnum.YieldToMaturity;
            //We need to get the ytm in until there is a bond market price/spread.
            if (BondPrice.dirtyPriceSpecified)
            {
                QuoteType = BondPriceEnum.DirtyPrice;
                Quote = BasicQuotationHelper.Create(BondPrice.dirtyPrice, RateQuotationType);
            }
            //Get the instrument configuration information.
            var assetIdentifier = leaseFpML.lease.currency.Value + "-Lease-" + LeaseType;
            LeaseNodeStruct leaseTypeInfo = null;
            //TODO Set the swap curves for asset swap valuation.
            //
            //Gets the template bond type
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
            if (instrument != null)
            {
                leaseTypeInfo = instrument.InstrumentNodeItem as LeaseNodeStruct;
            }
            if (leaseFpML.lease != null && leaseTypeInfo != null)
            {
                if (SettlementCalendar == null)
                {
                    SettlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, leaseTypeInfo.SettlementDate.businessCenters, nameSpace);
                }
                if (PaymentCalendar == null)
                {
                    PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, leaseTypeInfo.BusinessDayAdjustments.businessCenters, nameSpace);
                }
                //Pre-processes the data for the priceable asset.
                var lease = XmlSerializerHelper.Clone(leaseFpML.lease);
                Lease = lease;
                leaseTypeInfo.Lease = Lease;
                //Set the curves to use for valuations.
                LeaseCurveName = CurveNameHelpers.GetBondCurveName(Lease.currency.Value, Lease.id);
                //THe discount curve is only for credit calculations.
                DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Lease.currency.Value, true);
                if (lease.maturitySpecified)
                {
                    MaturityDate = lease.maturity;
                }
                SettlementDateConvention = leaseTypeInfo.SettlementDate;
                BusinessDayAdjustments = leaseTypeInfo.BusinessDayAdjustments;
                ExDivDateConvention = leaseTypeInfo.ExDivDate;
                //This is done because the config data is not stored in the correct way. Need to add a price quote units.
                if (lease.couponRateSpecified)
                {
                    var coupon = lease.couponRate;
                    Bond.couponRate = coupon;
                }
                leaseTypeInfo.Lease.faceAmount = NotionalAmount.amount;
                leaseTypeInfo.Lease.faceAmountSpecified = true;
                Bond.faceAmount = NotionalAmount.amount;
                if (Bond.maturitySpecified)
                {
                    RiskMaturityDate = Bond.maturity;
                }
                SettlementDate = settlementDate;
                if (!PaymentCurrencies.Contains(leaseFpML.bond.currency.Value))
                {
                    PaymentCurrencies.Add(leaseFpML.lease.currency.Value);
                }
                logger.LogInfo("Lease transaction has been successfully created.");
            }
            else
            {
                logger.LogInfo("Bond type data not available.");
            }
            //Set the underlying bond
            UnderlyingLease = new PriceableSimpleBond(tradeDate, leaseTypeInfo, SettlementCalendar, PaymentCalendar, Quote, QuoteType);
            //LeaseTenant = UnderlyingLease.Tenant;
            if (BondPrice.dirtyPriceSpecified)
            {
                UnderlyingLease.PurchasePrice = BondPrice.dirtyPrice / 100; //PriceQuoteUnits
            }
            //Set the coupons
            var leaseId = Lease.id;//Could use one of the instrumentIds
            //bondStream is an interest Rate Stream but needs to be converted to a bond stream.
            //It automatically contains the coupon currency.
            Coupons = new PriceableLeaseCouponRateStream(logger, cache, nameSpace, leaseId, tradeDate,
                leaseFpML.notionalAmount.amount, CouponStreamType.GenericFixedRate, Bond,
                BusinessDayAdjustments, ForecastRateInterpolation, null, PaymentCalendar);
            //Add payments like the settlement price
            if (!BondPrice.dirtyPriceSpecified) return;
            var amount = BondPrice.dirtyPrice * NotionalAmount.amount / 100;
            var settlementPayment = PaymentHelper.Create(BuyerReference, SellerReference, PaymentCurrency.Value, amount, SettlementDate);
            AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] { settlementPayment }, SettlementCalendar);
            //
            var finalPayment = PaymentHelper.Create(LeaseTenant, BuyerReference, CouponCurrency.Value, NotionalAmount.amount, RiskMaturityDate);
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
        public LeaseTransaction Build()
        {
            var lease = Lease;
            var buyerPartyReference = PartyReferenceHelper.Parse(BuyerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(SellerReference);
            var productType = new object[] {ProductTypeHelper.Create("LeaseTransaction")};
            var itemName = new[] {ItemsChoiceType2.productType};
            //TODO extend this
            var leaseTransaction = new LeaseTransaction
            {
                               notionalAmount = NotionalAmount,
                               lease = lease,
                               //price = XmlSerializerHelper.Clone(BondPrice),
                               buyerPartyReference = buyerPartyReference,
                               sellerPartyReference = sellerPartyReference,
                               id = Id,
                               Items = productType,
                               ItemsElementName = itemName
                           };
            return leaseTransaction;
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
            var leaseControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation leaseValuation;
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
            childControllerValuations.id = Id + ".LeaseCouponRateStreams";
            //4. Calc the asset as a little extra
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var metricsAsString = metrics.Select(metric => metric.ToString()).ToList();
            var controllerData = PriceableAssetFactory.CreateAssetControllerData(metricsAsString.ToArray(), modelData.ValuationDate, modelData.MarketEnvironment);
            UnderlyingLease.Multiplier = Multiplier;
            UnderlyingLease.Calculate(controllerData);
            //5. Now do the lease calculations.
            if (leaseControllerMetrics.Count > 0)
            {
                CalculationResults = new LeaseTransactionResults();
                if (marketEnvironment.GetType() == typeof(MarketEnvironment))
                {
                    var leaseCurve = (IBondCurve)modelData.MarketEnvironment.GetPricingStructure(LeaseCurveName);
                    if (leaseCurve != null)
                    {
                        var marketDataType =
                            leaseCurve.GetPricingStructureId().Properties.GetValue<string>(AssetMeasureEnum.MarketQuote.ToString(), false);
                        //TODO handle the other cases like: AssetSwapSpread; DirtyPrice and ZSpread.
                        var mq = (decimal)leaseCurve.GetYieldToMaturity(modelData.ValuationDate, SettlementDate);
                        Quote = BasicQuotationHelper.Create(mq, AssetMeasureEnum.MarketQuote.ToString(),
                                                            PriceQuoteUnitsEnum.DecimalRate.ToString());
                    }
                    rateDiscountCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(DiscountCurveName);
                }
                //Generate the vectors
                const bool isBuyerInd = true;
                var analyticModelParameters = new LeaseTransactionParameters
                {
                    IsBuyerInd = isBuyerInd,
                    AccrualYearFractions = GetCouponAccrualFactors(),
                    Multiplier = Multiplier,
                    Quote = QuoteValue,
                    CouponRate = UnderlyingLease.GetCouponRate(),
                    NotionalAmount = UnderlyingLease.Notional,
                    Frequency = UnderlyingLease.Frequency,
                    IsYTMQuote = IsYTMQuote,
                    AccruedFactor = UnderlyingLease.GetAccruedFactor(),
                    RemainingAccruedFactor = UnderlyingLease.GetRemainingAccruedFactor(),
                    PaymentDiscountFactors =
                        GetDiscountFactors(rateDiscountCurve, Coupons.StreamPaymentDates.ToArray(), modelData.ValuationDate),
                };
                //5. Get the Weightings
                analyticModelParameters.Weightings =
                    CreateWeightings(CDefaultWeightingValue, analyticModelParameters.PaymentDiscountFactors.Length);
                //6. Set the analytic input parameters and Calculate the respective metrics 
                AnalyticModelParameters = analyticModelParameters;
                CalculationResults = AnalyticsModel.Calculate<ILeaseTransactionResults, LeaseTransactionResults>(analyticModelParameters, metrics.ToArray());
                // Now merge back into the overall stream valuation
                var leaseControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                leaseValuation = AssetValuationHelper.UpdateValuation(leaseControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(leaseControllerMetrics), new List<string>(Metrics));
            }
            else
            {
                leaseValuation = childControllerValuations;
            }
            // store inputs and results from this run
            CalculationPerformedIndicator = true;
            leaseValuation.id = Id;
            return leaseValuation;
        }

        private decimal[] GetCouponAccrualFactors()
        {
            var result = Coupons.GetCouponAccrualFactors();
            return result.ToArray();
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ILeaseTransactionResults CalculationResults { get; protected set; }

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
            var result = new List<InstrumentControllerBase>();
            result.AddRange(GetAdditionalPayments());
            result.AddRange(GetCoupons());
            return result;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        private IList<InstrumentControllerBase> GetCoupons()
        {
            return Coupons?.GetChildren();
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
        private static decimal[] CreateWeightings(decimal weightingValue, int noOfInstances)
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
                //if (value.quoteUnits.Value == "DecimalRate")
                //{
                //    IsYTMQuote = true;
                //    QuoteValue = value.value;
                //}
                //if (value.quoteUnits.Value == "Rate")
                //{
                //    IsYTMQuote = true;
                //    QuoteValue = value.value / 100.0m;
                //}
                //if (value.quoteUnits.Value == "DirtyPrice")
                //{
                //    IsYTMQuote = false;
                //    QuoteValue = value.value / 100.0m;
                //}
                //if (value.quoteUnits.Value == "CleanPrice")
                //{
                //    IsYTMQuote = false;
                //    QuoteValue = value.value / 100.0m;
                //}
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
