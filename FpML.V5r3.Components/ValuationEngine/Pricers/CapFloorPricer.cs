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
using System.Linq;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Helpers;
using Orion.Models.Rates.Swap;
using Orion.Models.Rates.CapFloor;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.PricingStructures;
using Orion.ValuationEngine.Valuations;
using Orion.ValuationEngine.Instruments;
using Orion.CurveEngine.Factory;
using Orion.ValuationEngine.Factory;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class CapFloorPricer : SwapPricer, IPriceableInstrumentController<CapFloor>
    {
        #region Properties

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

        public PriceableCapFloorStream CapFloorLeg 
        { 
            get
            { 
                var stream = Legs[0] as PriceableCapFloorStream;
                return stream;
            } 
        }

        /// <summary>
        /// Gets a value indicating whether [base party selling the option].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party selling the option]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyBuyer { get; set; }

        #endregion

        #region Constructors

        public CapFloorPricer()
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="capFloorFpML"></param>
        /// <param name="basePartyReference"></param>
        public CapFloorPricer(ILogger logger, ICoreCache cache, String nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
            CapFloor capFloorFpML, string basePartyReference)
            : this(logger, cache, nameSpace, fixingCalendar, paymentCalendar,
            capFloorFpML, basePartyReference, false)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="paymentCalendar"> </param>
        /// <param name="capFloorFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"> </param>
        public CapFloorPricer(ILogger logger, ICoreCache cache, String nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
            CapFloor capFloorFpML, string basePartyReference, Boolean forecastRateInterpolation)
        {
            if (capFloorFpML == null) return;
            //AnalyticsModel = new SimpleIRSwapInstrumentAnalytic();
            BusinessCentersResolver.ResolveBusinessCenters(capFloorFpML);
            ForecastRateInterpolation = forecastRateInterpolation;
            //We make the assumption that the termination date is the same for all legs..
            AdjustableDate adjustableTerminationDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(capFloorFpML.capFloorStream.calculationPeriodDates);
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableTerminationDate.dateAdjustments.businessCenters, nameSpace);
            }
            RiskMaturityDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustableTerminationDate);
            ProductType = ProductTypeSimpleEnum.CapFloor;
            PaymentCurrencies = new List<string>();
            //Resolve the payer
            if (capFloorFpML.capFloorStream == null) return;
            var calculation = capFloorFpML.capFloorStream.calculationPeriodAmount.Item as Calculation;
            var floatingRateCalculation = calculation?.Items[0] as FloatingRateCalculation;
            if (floatingRateCalculation == null) return;
            if (floatingRateCalculation.capRateSchedule != null)
            {
                var schedule = floatingRateCalculation.capRateSchedule[0];
                var buyerPartyReference = schedule.buyer.Value;
                if (buyerPartyReference == PayerReceiverEnum.Receiver)
                {
                    BuyerPartyReference = capFloorFpML.capFloorStream.receiverPartyReference.href;
                    SellerPartyReference = capFloorFpML.capFloorStream.payerPartyReference.href;
                }
                else
                {
                    BuyerPartyReference = capFloorFpML.capFloorStream.payerPartyReference.href;
                    SellerPartyReference = capFloorFpML.capFloorStream.receiverPartyReference.href;
                }
            }
            if (floatingRateCalculation.capRateSchedule == null && floatingRateCalculation.floorRateSchedule != null)
            {
                var schedule = floatingRateCalculation.floorRateSchedule[0];
                var buyerPartyReference = schedule.buyer.Value;
                if (buyerPartyReference == PayerReceiverEnum.Receiver)
                {
                    BuyerPartyReference = capFloorFpML.capFloorStream.receiverPartyReference.href;
                    SellerPartyReference = capFloorFpML.capFloorStream.payerPartyReference.href;
                }
                else
                {
                    BuyerPartyReference = capFloorFpML.capFloorStream.payerPartyReference.href;
                    SellerPartyReference = capFloorFpML.capFloorStream.receiverPartyReference.href;
                }
            }
            BasePartyBuyer = basePartyReference == BuyerPartyReference;//TODO add in the calendar functionality.
            //Set the id of the first stream. THe generator requires the flag: BasePartyPayer.
            var capFloorLeg = new PriceableCapFloorStream(logger, cache, nameSpace, !BasePartyBuyer, capFloorFpML.capFloorStream, ForecastRateInterpolation, fixingCalendar, paymentCalendar);
            Legs.Add(capFloorLeg);
            //Add the currencies for the trade pricer.
            if (!PaymentCurrencies.Contains(capFloorLeg.Currency.Value))
            {
                PaymentCurrencies.Add(capFloorLeg.Currency.Value);
            }
            if (capFloorFpML.additionalPayment != null)
            {
                AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, capFloorFpML.additionalPayment, null);
                foreach (var payment in capFloorFpML.additionalPayment)
                {
                    if (!PaymentCurrencies.Contains(payment.paymentAmount.currency.Value))
                    {
                        PaymentCurrencies.Add(payment.paymentAmount.currency.Value);
                    }
                }
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

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
            // NOTE: These take precedence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new CapFloorInstrumentAnalytic();
            }
            var swapControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation swapValuation;
            //Sets the evolution type for calculations.
            CapFloorLeg.PricingStructureEvolutionType = PricingStructureEvolutionType;
            CapFloorLeg.BucketedDates = BucketedDates;
            CapFloorLeg.Multiplier = Multiplier;
            if (AdditionalPayments != null)
            {
                foreach (var payment in AdditionalPayments)
                {
                    payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    payment.BucketedDates = BucketedDates;
                    payment.Multiplier = Multiplier;
                }
            }
            //The assetValuation list.
            var childValuations = new List<AssetValuation> {CapFloorLeg.Calculate(modelData)};
            // 2. Now evaluate only the child specific metrics (if any)
            if (GetAdditionalPayments() != null)
            {
                var paymentControllers = new List<InstrumentControllerBase>(GetAdditionalPayments());
                childValuations.AddRange(paymentControllers.Select(payment => payment.Calculate(modelData)));
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            childControllerValuations.id = Id + ".CapFloorStreams";
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (swapControllerMetrics.Count > 0)
            {
                //Generate the vectors
                var streamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.AccrualFactor.ToString());
                var streamFloatingNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.FloatingNPV.ToString());
                var streamNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.NPV.ToString());
                IIRSwapInstrumentParameters analyticModelParameters = new SwapInstrumentParameters
                {
                    AccrualFactor = streamAccrualFactor.value,
                    FloatingNPV = streamFloatingNPV.value,
                    NPV = streamNPV.value
                    //, MarketQuote = MarketQuote.
                };
                AnalyticModelParameters = analyticModelParameters;
                CalculationResults = AnalyticsModel.Calculate<IIRSwapInstrumentResults, SwapInstrumentResults>(analyticModelParameters, swapControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var swapControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                swapValuation = AssetValuationHelper.UpdateValuation(swapControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(swapControllerMetrics), new List<string>(Metrics));
            }
            else
            {
                swapValuation = childControllerValuations;
            }
            CalculationPerformedIndicator = true;
            swapValuation.id = Id;
            return swapValuation;
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }


        /// <summary>
        /// Builds this instance and returns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public new CapFloor Build()
        {
            var capFloor = new CapFloor
            {
                Items = new object[] { new ProductType { Value = ProductType.ToString() } },
                ItemsElementName = new[] { ItemsChoiceType2.productType },
                additionalPayment = MapToPayments(AdditionalPayments),
                capFloorStream = CapFloorLeg.Build()
            };

            return capFloor;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1PaymentCalendar"> </param>
        /// <param name="capStrikeSchedule"></param>
        /// <param name="floorStrikeSchedule"> </param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="leg1FixingCalendar"> </param>
        /// <returns></returns>
        public static Trade CreateTrade(CapFloorLegParametersRange leg1Parameters,
                                                       IBusinessCalendar leg1FixingCalendar,
                                                       IBusinessCalendar leg1PaymentCalendar,
                                                       Schedule capStrikeSchedule,
                                                       Schedule floorStrikeSchedule,
                                                       Schedule spreadSchedule,
                                                       NonNegativeAmountSchedule notionalSchedule)
        {
            var stream1 = GetCashflowsSchedule(leg1FixingCalendar, leg1PaymentCalendar, leg1Parameters);
            if (null != capStrikeSchedule && null != floorStrikeSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetCapRateSchedule(stream1, capStrikeSchedule, true);
                InterestRateStreamParametricDefinitionGenerator.SetFloorRateSchedule(stream1, floorStrikeSchedule, false);
            }
            if (null != capStrikeSchedule && null == floorStrikeSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetCapRateSchedule(stream1, capStrikeSchedule, true);
            }
            if (null == capStrikeSchedule && null != floorStrikeSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetFloorRateSchedule(stream1, floorStrikeSchedule, true);
            }
            if (null != spreadSchedule) //for float legs only
            {
                InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(stream1, spreadSchedule);
            }
            if (null != notionalSchedule)
            {
                //  Set notional schedule
                //
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream1, notionalSchedule);
            }
            var capFloor = CapFloorFactory.Create(stream1);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capFloor);
            return trade;
        }

        #endregion

        #region public (visible to ExcelAPI) methods

        public string CreateValuation(
            ILogger logger, ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            List<StringObjectRangeItem> valuationSet,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            CapFloorLegParametersRange_Old legParametersRange,
            List<InputCashflowRangeItem> legDetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> legPrincipleExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> legAdditionalPaymentListArray,
            List<PartyIdRangeItem>  partyIdList,//optional
            List<OtherPartyPaymentRangeItem> otherPartyPaymentList,    //optional
            List<FeePaymentRangeItem> feePaymentList   //optional
            )
        {
            Pair<ValuationResultRange, CapFloor> fpML = GetPriceAndGeneratedFpML(logger, cache, nameSpace, fixingCalendar, paymentCalendar, valuationRange, tradeRange, 
                                                                                 legParametersRange, legDetailedCashflowsListArray, legPrincipleExchangeCashflowListArray, 
                                                                                 legAdditionalPaymentListArray, feePaymentList);
            CapFloor capFloor = fpML.Second;
            string valuationReportAndProductId = tradeRange.Id ?? Guid.NewGuid().ToString();
            capFloor.id = valuationReportAndProductId;
            AssetValuation assetValuation = InterestRateProduct.CreateAssetValuationFromValuationSet(valuationSet);
            //Valuation valuation = new Valuation();
            //  TODO: add Trade Id & Trade data into valuation. (Trade.Id & Trade.TradeHeader.TradeDate)
            //
            string baseParty = valuationRange.BaseParty;
            var uniqueCurves = new List<IRateCurve>();                 
            foreach (string curveName in new[] { legParametersRange.ForecastCurve, legParametersRange.DiscountCurve})
            {
                if (!String.IsNullOrEmpty(curveName))
                {
                    var curve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, curveName);
                    if (!uniqueCurves.Contains(curve))
                    {
                        uniqueCurves.Add(curve);
                    }
                }
            }
            Market fpMLMarket = InterestRateProduct.CreateFpMLMarketFromCurves(uniqueCurves);
            ValuationReport valuationReport = ValuationReportGenerator.Generate(valuationReportAndProductId, baseParty, valuationReportAndProductId, tradeRange.TradeDate, capFloor, fpMLMarket, assetValuation);
            cache.SaveObject(valuationReport, valuationReportAndProductId, null);
            InterestRateProduct.ReplacePartiesInValuationReport(valuationReport, partyIdList);
            InterestRateProduct.AddOtherPartyPayments(valuationReport, otherPartyPaymentList);
            return valuationReportAndProductId;
        }

        public List<DetailedCashflowRangeItem> GetDetailedCashflowsWithNotionalSchedule(
            ILogger logger, ICoreCache cache, 
            String nameSpace,
            IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange_Old legParametersRange,
            List<DateTimeDoubleRangeItem> notionalValueItems,
            ValuationRange valuationRange)
        {
            //Check if the calendars are null. If not build them!
            var list1 = notionalValueItems.Select(item => new Pair<DateTime, decimal>(item.DateTime, Convert.ToDecimal(item.Value))).ToList();
            NonNegativeSchedule notionalScheduleFpML = NonNegativeScheduleHelper.Create(list1);
            Currency currency = CurrencyHelper.Parse(legParametersRange.Currency);
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalScheduleFpML, currency);
            InterestRateStream interestRateStream = GetCashflowsScheduleWithNotionalSchedule(fixingCalendar, paymentCalendar, legParametersRange, amountSchedule);
            //Add the principal exchanges to the cashflows.
            var principalExchangeList = list1.Select(cashflow => new PrincipalExchange
                                                                     {
                                                                         adjustedPrincipalExchangeDate = cashflow.First, adjustedPrincipalExchangeDateSpecified = true, principalExchangeAmount = cashflow.Second, principalExchangeAmountSpecified = true
                                                                     }).ToArray();
            interestRateStream.cashflows.principalExchange = principalExchangeList;
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<DetailedCashflowRangeItem>();
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                var detailedCashflowRangeItem =
                    new DetailedCashflowRangeItem
                    {
                        PaymentDate = paymentCalculationPeriod.adjustedPaymentDate,
                        StartDate =
                            PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod),
                        EndDate = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod),
                        NumberOfDays = PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod),
                        FutureValue = MoneyHelper.ToDouble(paymentCalculationPeriod.forecastPaymentAmount),
                        PresentValue = MoneyHelper.ToDouble(paymentCalculationPeriod.presentValueAmount),
                        DiscountFactor = (double) paymentCalculationPeriod.discountFactor,
                        NotionalAmount =
                            (double) PaymentCalculationPeriodHelper.GetNotionalAmount(paymentCalculationPeriod),
                        CouponType = GetCouponType(paymentCalculationPeriod),
                        Rate = (double) PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod)
                    };
                CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(paymentCalculationPeriod)[0];
                FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                switch(detailedCashflowRangeItem.CouponType.ToLower())
                {
                    case "cap":
                        {
                            Strike strike = floatingRateDefinition.capRate[0];
                            detailedCashflowRangeItem.StrikeRate = (double)strike.strikeRate;
                            break; 
                        }
                    case "floor":
                        {
                            Strike strike = floatingRateDefinition.floorRate[0];
                            detailedCashflowRangeItem.StrikeRate = (double)strike.strikeRate;
                            break;
                        }                 
                    default:
                        {
                            string message =
                                $"Specified coupon type : '{detailedCashflowRangeItem.CouponType.ToLower()}' is not supported. Please use one of these: 'cap, floor'";
                            throw new NotSupportedException(message);
                        }
                }             
                //  If  floating rate - retrieve the spread.
                //
                detailedCashflowRangeItem.Spread = (double)PaymentCalculationPeriodHelper.GetSpread(paymentCalculationPeriod);
                var fixingDate = new DateTime();
                var tempDate = PaymentCalculationPeriodHelper.GetFirstFloatingFixingDate(paymentCalculationPeriod);
                if (tempDate != null)
                {
                    fixingDate = (DateTime)tempDate;
                }
                detailedCashflowRangeItem.FixingDate = fixingDate;
                detailedCashflowRangeItem.Currency = "Not Specified";
                if (currency != null)
                {
                    detailedCashflowRangeItem.Currency = currency.Value;
                }
                list.Add(detailedCashflowRangeItem);
            }
            return list;
        }

        #endregion

        #region Valuation Methods

        public static CapFloor GeneratedFpMLCapFloor(
            ILogger logger, ICoreCache cache,
            CapFloorLegParametersRange capFloorParametersRange,
            List<InputCashflowRangeItem> capFloorDetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> capFloorPrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> capFloorAdditionalPaymentList,
            List<FeePaymentRangeItem> feePaymentList
            )
        {
            //Check if the calendars are null. If not build them!
            InterestRateStream stream1 = GetCashflowsSchedule(null, null, capFloorParametersRange);//parametric definition + cashflows schedule
            // Update FpML cashflows
            //
            stream1.cashflows = UpdateCashflowsWithDetailedCashflows(capFloorDetailedCashflowsList);
            if (null != capFloorPrincipalExchangeCashflowListArray)
            {
                // create principal exchanges
                //
                InterestRateSwapPricer.CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, capFloorPrincipalExchangeCashflowListArray);
            }
            //  Add bullet payments...
            //
            var bulletPaymentList = new List<Payment>();
            if (null != capFloorAdditionalPaymentList)
            {
                bulletPaymentList.AddRange(capFloorAdditionalPaymentList.Select(bulletPaymentRangeItem => 
                    new Payment{  payerPartyReference = PartyReferenceFactory.Create(capFloorParametersRange.Payer), receiverPartyReference =
                        PartyReferenceFactory.Create(capFloorParametersRange.Receiver),
                                  paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                                  paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                                                                                                              }));
            }
            CapFloor capFloor = CapFloorFactory.Create(stream1);
            capFloor.additionalPayment = bulletPaymentList.ToArray();
            var feeList = new List<Payment>();
            if (null != feePaymentList)
            {
                feeList.AddRange(feePaymentList.Select(feePaymentRangeItem =>
                    new Payment
                    {
                        paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(feePaymentRangeItem.PaymentDate),
                                paymentAmount = MoneyHelper.GetNonNegativeAmount(feePaymentRangeItem.Amount, feePaymentRangeItem.Currency), //TODO The currency needs to be added!
                        payerPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Payer), 
                        receiverPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Receiver)
                                                                                  }));
            }
            capFloor.premium = feeList.ToArray();
            return capFloor;
        }

        public static Pair<ValuationResultRange, CapFloor> GetPriceAndGeneratedFpML(
            ILogger logger, ICoreCache cache, 
            String nameSpace,
            IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar, 
            ValuationRange valuationRange, TradeRange tradeRange,
            CapFloorLegParametersRange_Old leg1ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> legPrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,
            List<FeePaymentRangeItem> feePaymentList
            )
        {
            //Check if the calendars are null. If not build them!
            InterestRateStream stream1 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg1ParametersRange);//parametric definition + cashflows schedule
            // Update FpML cashflows
            //
            stream1.cashflows = UpdateCashflowsWithDetailedCashflows(leg1DetailedCashflowsList);
            if (null != legPrincipalExchangeCashflowListArray)
            {
                // create principal exchanges
                //
                InterestRateSwapPricer.CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, legPrincipalExchangeCashflowListArray);
            }
            //  Add bullet payments...
            //
            var bulletPaymentList = new List<Payment>();
            if (null != leg1AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg1AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Payer), 
                    receiverPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Receiver),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            CapFloor capFloor = CapFloorFactory.Create(stream1);
            capFloor.additionalPayment = bulletPaymentList.ToArray();
            var feeList = new List<Payment>();
            if (null != feePaymentList)
            {
                feeList.AddRange(feePaymentList.Select(feePaymentRangeItem => new Payment
                {
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(feePaymentRangeItem.PaymentDate),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(feePaymentRangeItem.Amount, feePaymentRangeItem.Currency), 
                    payerPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Payer), 
                    receiverPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Receiver)
                }));
            }
            capFloor.premium = feeList.ToArray();                                          
            // Update FpML cashflows with DF,FV,PV, etc (LegParametersRange needed to access curve functionality)
            //
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            //  Update additional payments
            //
            var leg1DiscountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, leg1ParametersRange.DiscountCurve);
            CapFloorGenerator.UpdatePaymentsAmounts(paymentCalendar, capFloor, leg1ParametersRange, leg1DiscountCurve, valuationRange.ValuationDate);
            //~  Update additional payments
            string baseParty = valuationRange.BaseParty;
            return new Pair<ValuationResultRange, CapFloor>(CreateValuationRange(capFloor, baseParty), capFloor);
        }

        private static ValuationResultRange CreateValuationRange(CapFloor capFloor, string baseParty)
        {
            Money payPresentValue = CapFloorHelper.GetPayPresentValue(capFloor, baseParty);
            Money payFutureValue = CapFloorHelper.GetPayFutureValue(capFloor, baseParty);
            Money receivePresentValue = CapFloorHelper.GetReceivePresentValue(capFloor, baseParty);
            Money receiveFutureValue = CapFloorHelper.GetReceiveFutureValue(capFloor, baseParty);
            Money swapPresentValue = CapFloorHelper.GetPresentValue(capFloor, baseParty);
            Money swapFutureValue = CapFloorHelper.GetFutureValue(capFloor, baseParty);
            var resultRange = new ValuationResultRange
                                  {
                                      PresentValue = swapPresentValue.amount,
                                      FutureValue = swapFutureValue.amount,
                                      PayLegPresentValue = payPresentValue.amount,
                                      PayLegFutureValue = payFutureValue.amount,
                                      ReceiveLegPresentValue = receivePresentValue.amount,
                                      ReceiveLegFutureValue = receiveFutureValue.amount
                                  };

            return resultRange;
        }

        private static Cashflows UpdateCashflowsWithDetailedCashflows(IEnumerable<InputCashflowRangeItem> listDetailedCashflows/*, bool fixedLeg*/)
        {
            var cashflows = new Cashflows();
            var paymentCalculationPeriods = new List<PaymentCalculationPeriod>();
            foreach (var detailedCashflowRangeItem in listDetailedCashflows)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod();
                var calculationPeriod = new CalculationPeriod();
                paymentCalculationPeriod.Items = new object[] { calculationPeriod };
                paymentCalculationPeriod.adjustedPaymentDate = detailedCashflowRangeItem.PaymentDate;
                paymentCalculationPeriod.adjustedPaymentDateSpecified = true;
                PaymentCalculationPeriodHelper.SetCalculationPeriodStartDate(paymentCalculationPeriod, detailedCashflowRangeItem.StartDate);
                PaymentCalculationPeriodHelper.SetCalculationPeriodEndDate(paymentCalculationPeriod, detailedCashflowRangeItem.EndDate);
                // Update notional amount
                //
                PaymentCalculationPeriodHelper.SetNotionalAmount(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.NotionalAmount);
                switch(detailedCashflowRangeItem.CouponType.ToLower())
                {
                    case "cap":
                        {
                            var floatingRateDefinition = new FloatingRateDefinition();
                            calculationPeriod.Item1 = floatingRateDefinition;
                            // After the spread is reset - we need to update calculated rate.
                            //
                            PaymentCalculationPeriodHelper.SetSpread(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Spread);
                            floatingRateDefinition.capRate = new[] { new Strike() };
                            floatingRateDefinition.capRate[0].strikeRate = (decimal)detailedCashflowRangeItem.StrikeRate;//tODO There is no fixing date.
                            {
                                var rateObservation = new RateObservation
                                                          {
                                                              adjustedFixingDate =
                                                                  detailedCashflowRangeItem.FixingDate,
                                                              adjustedFixingDateSpecified = true
                                                          };
                                floatingRateDefinition.rateObservation = new[] {rateObservation};
                            }
                            break;
                        }
                    case "floor":
                        {
                            var floatingRateDefinition = new FloatingRateDefinition();
                            //XsdClassesFieldResolver.CalculationPeriod_SetFloatingRateDefinition(calculationPeriod, floatingRateDefinition);
                            calculationPeriod.Item1 = floatingRateDefinition;
                            // After the spread is reset - we need to update calculated rate.
                            //
                            PaymentCalculationPeriodHelper.SetSpread(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Spread);
                            floatingRateDefinition.capRate = new[] { new Strike() };
                            floatingRateDefinition.capRate[0].strikeRate = (decimal)detailedCashflowRangeItem.StrikeRate;

                            {
                                var rateObservation = new RateObservation
                                                          {
                                                              adjustedFixingDate =
                                                                  detailedCashflowRangeItem.FixingDate,
                                                              adjustedFixingDateSpecified = true
                                                          };
                                floatingRateDefinition.rateObservation = new[] { rateObservation };
                            }
                            break;
                        }
                    default:
                        {
                            string message =
                                $"Specified coupon type : '{detailedCashflowRangeItem.CouponType.ToLower()}' is not supported. Please use one of these: 'cap, floor'";
                            throw new NotSupportedException(message);
                        }
                }
                paymentCalculationPeriods.Add(paymentCalculationPeriod);
            }
            cashflows.cashflowsMatchParameters = true;
            cashflows.paymentCalculationPeriod = paymentCalculationPeriods.ToArray();
            return cashflows;
        }

        private static InterestRateStream GetCashflowsSchedule(IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        private static string GetCouponType(PaymentCalculationPeriod pcalculationPeriod)
        {
            CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(pcalculationPeriod)[0];
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
            //  If has a Cap rate, finalRate = MAX(0, FinalRate - CapRate)
            //
            if (null != floatingRateDefinition.capRate)
            {
                return "Cap";
            }
            //  If has a Floor rate, finalRate = MAX(0, FloorRate - FinalRate)
            //
            if (null != floatingRateDefinition.floorRate)
            {
                return "Floor";
            }
            throw new System.Exception("Invalid coupon type. Only Cap & Floor coupons are expected here.");
        }

        private static InterestRateStream GetCashflowsScheduleWithNotionalSchedule(
            IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange_Old legParametersRange, 
            NonNegativeAmountSchedule notionalSchedule)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream, notionalSchedule);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        private static void UpdateCashflowsWithAmounts(ILogger logger, ICoreCache cache, 
            String nameSpace, InterestRateStream stream,
            CapFloorLegParametersRange_Old legParametersRange, ValuationRange valuationRange)
        {
            //  Get a forecast curve
            //
            IRateCurve forecastCurve = null;
            if (!String.IsNullOrEmpty(legParametersRange.ForecastCurve) && legParametersRange.ForecastCurve.ToLower() != "none")
            {
                forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, legParametersRange.ForecastCurve);
            }
            //  Get a discount curve
            //
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, legParametersRange.DiscountCurve);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountCurve, valuationRange.ValuationDate);
        }

        #endregion

        #region Private methods

/*
        private decimal[] GetCouponNotionals()
        {
            var result = new List<decimal>();
            CapFloorLeg.GetCouponNotionals();
            return result.ToArray();
        }
*/

/*
        private decimal[] GetCouponAccrualFactors()
        {
            var result = new List<decimal>();
            CapFloorLeg.GetCouponAccrualFactors();
            return result.ToArray();
        }
*/

/*
        private decimal[] GetCouponDiscountFactors()
        {
            var result = new List<decimal>();
            CapFloorLeg.GetPaymentDiscountFactors();
            return result.ToArray();
        }
*/

        #endregion
    }
}