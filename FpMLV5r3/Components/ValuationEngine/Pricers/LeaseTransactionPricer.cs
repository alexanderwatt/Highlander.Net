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
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.Reporting.ModelFramework.V5r3.Instruments.Lease;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
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
        public bool BasePartyTenant{ get; set; }

        public List<DateTime> GetPaymentDates()
        {
            throw new NotImplementedException();
        }

        public List<decimal> GetPaymentAmounts()
        {
            throw new NotImplementedException();
        }

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
        /// The base date from the trade pricer.
        /// </summary>
        public DateTime TradeDate { get; set; }

        ///<summary>
        /// The payment calendar.
        ///</summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// The buyer reference
        /// </summary>
        public string ReceiverReference { get; set; }

        /// <summary>
        /// The seller reference
        /// </summary>
        public string PayerReference { get; set; }

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
        public Money Amount { get; set; }

        /// <summary>
        /// The business day adjustments for bond coupon and final exchange payments
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// The lease details.
        /// </summary>
        public Lease Lease { get; set; }

        /// <summary>
        /// Gets the settlement date.
        /// </summary>
        /// <value>The settlement date.</value>
        public DateTime? SettlementDate { get; set; }

        /// <summary>
        /// Gets the maturity date.
        /// </summary>
        /// <value>The maturity date.</value>
        public DateTime MaturityDate { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ILeaseTransactionParameters, LeaseTransactionMetrics> AnalyticsModel { get; set; }

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
        public PriceableLeasePaymentStream PaymentStream;

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
        /// <param name="settlementAmount"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="leaseFpMl"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="leaseType"></param>
        public LeaseTransactionPricer(ILogger logger, ICoreCache cache, string nameSpace, DateTime tradeDate,
            DateTime? settlementDate, decimal? settlementAmount, IBusinessCalendar paymentCalendar,
            LeaseTransaction leaseFpMl, string basePartyReference, string leaseType)
        {
            Multiplier = 1.0m;
            TradeDate = tradeDate;
            LeaseType = leaseType;
            logger.LogInfo("LeaseType set. Commence to build a lease transaction.");
            if (leaseFpMl == null) return;
            ReceiverReference = leaseFpMl.buyerPartyReference.href;
            PaymentCurrencies = new List<string> { leaseFpMl.lease.currency.Value};
            PayerReference = leaseFpMl.sellerPartyReference.href;
            BasePartyTenant = basePartyReference == leaseFpMl.buyerPartyReference.href;
            if (!BasePartyTenant)
            {
                Multiplier = -1.0m;
            }
            PaymentCalendar = paymentCalendar;
            //Set the currencies
            PaymentCurrency = leaseFpMl.lease.currency;//This could be another currency!
            //Set the notional information
            Amount = leaseFpMl.notionalAmount;
            //Get the instrument configuration information.
            var assetIdentifier = leaseFpMl.lease.currency.Value + "-Lease-" + LeaseType;
            LeaseNodeStruct leaseTypeInfo = null;
            //Gets the template lease type
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
            if (instrument != null)
            {
                leaseTypeInfo = instrument.InstrumentNodeItem as LeaseNodeStruct;
            }
            if (leaseFpMl.lease != null && leaseTypeInfo != null)
            {
                if (PaymentCalendar == null)
                {
                    PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, leaseTypeInfo.Lease.businessDayAdjustments.businessCenters, nameSpace);
                }
                //Pre-processes the data for the priceable asset.
                var lease = XmlSerializerHelper.Clone(leaseFpMl.lease);
                Lease = lease;
                //Set the curves to use for valuations.
                LeaseCurveName = CurveNameHelpers.GetLeaseCurveName(Lease.currency.Value, Lease.id);
                //THe discount curve is only for credit calculations.
                DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Lease.currency.Value, true);
                MaturityDate = Lease.leaseExpiryDate.Value;
                RiskMaturityDate = Lease.leaseExpiryDate.Value;
                SettlementDate = settlementDate;
                if (!PaymentCurrencies.Contains(Lease.currency.Value))
                {
                    PaymentCurrencies.Add(Lease.currency.Value);
                }
                logger.LogInfo("Lease transaction has been successfully created.");
            }
            else
            {
                logger.LogInfo("Lease type data not available.");
            }
            //Set the payments
            var leaseId = Lease.id;//Could use one of the instrumentIds
            PaymentStream = new PriceableLeasePaymentStream(logger, cache, nameSpace, leaseId, ReceiverReference, PayerReference, BasePartyTenant, tradeDate,
                leaseFpMl.lease, BusinessDayAdjustments, PaymentCalendar);
            //Add payments like the settlement price
            if (settlementAmount == null || SettlementDate == null) return;
            var payment = PaymentHelper.Create(ReceiverReference, PayerReference, PaymentCurrency.Value,
                (decimal)settlementAmount, (DateTime)SettlementDate);
            AdditionalPayments =
                PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, new[] {payment},
                    PaymentCalendar);
            if (!PaymentCurrencies.Contains(payment.paymentAmount.currency.Value))
            {
                PaymentCurrencies.Add(payment.paymentAmount.currency.Value);
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
            var buyerPartyReference = PartyReferenceHelper.Parse(PayerReference);
            var sellerPartyReference = PartyReferenceHelper.Parse(ReceiverReference);
            var productType = new object[] {ProductTypeHelper.Create("LeaseTransaction")};
            var itemName = new[] {ItemsChoiceType2.productType};
            //TODO extend this
            var leaseTransaction = new LeaseTransaction
            {
                               notionalAmount = Amount,
                               lease = lease,
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
                AnalyticsModel = new LeaseTransactionAnalytic();
            }
            var leaseControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation leaseValuation;
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, LeaseTransactionMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, LeaseTransactionMetrics.NPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            ModelData.AssetValuation.quote = quotes.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            //2. Sets the evolution type for coupon and payment calculations.
            PaymentStream.PricingStructureEvolutionType = PricingStructureEvolutionType;
            PaymentStream.BucketedDates = BucketedDates;
            PaymentStream.Multiplier = Multiplier;
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
            List<AssetValuation> childValuations = new List<AssetValuation> { PaymentStream?.Calculate(modelData)};
            if (GetAdditionalPayments() != null)
            {
                var paymentControllers = new List<InstrumentControllerBase>(GetAdditionalPayments());
                childValuations.AddRange(paymentControllers.Select(payment => payment.Calculate(modelData)));
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);
            childControllerValuations.id = Id + ".LeaseCouponRateStreams";
            //4. Calc the asset as a little extra
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            //5. Now do the lease calculations.
            if (leaseControllerMetrics.Count > 0)
            {
                CalculationResults = new LeaseTransactionResults();
                IRateCurve leaseCurve = null;
                if (marketEnvironment.GetType() == typeof(MarketEnvironment))
                {
                    leaseCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(DiscountCurveName);
                }
                //Generate the vectors
                var analyticModelParameters = new LeaseTransactionParameters
                {
                    Multiplier = Multiplier,
                    Amounts = PaymentStream.GetPaymentAmounts(),
                    PaymentDiscountFactors =
                        GetDiscountFactors(leaseCurve, PaymentStream.StreamPaymentDates.ToArray(), modelData.ValuationDate),
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

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ILeaseTransactionResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the additional controllers.
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
            result.AddRange(GetPayments());
            return result;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        private IList<InstrumentControllerBase> GetPayments()
        {
            return PaymentStream?.GetChildren();
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
            set => MarketQuote = value;
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
