#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Helpers;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    public class FloaterPricer
    {
        #region Excel interface methods

        public string CreateValuation(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            List<StringObjectRangeItem> valuationSet,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            SwapLegParametersRange_Old leg1ParametersRange,
            List<DetailedCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<PrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<PartyIdRangeItem>  partyIdList,//optional
            List<OtherPartyPaymentRangeItem> otherPartyPaymentList
            )
        {
            Swap floater = GetPriceAndGeneratedFpMLSwap(logger, cache, nameSpace, fixingCalendar, paymentCalendar, valuationRange, tradeRange, leg1ParametersRange, leg1DetailedCashflowsListArray, leg1PrincipalExchangeCashflowListArray, leg1AdditionalPaymentListArray).Second;
            string baseParty = valuationRange.BaseParty;
            string valuationReportAndProductId = tradeRange.Id ?? Guid.NewGuid().ToString();
            floater.id = valuationReportAndProductId;
            var uniqueCurves = GetUniqueCurves(logger, cache, nameSpace, leg1ParametersRange);
            Market fpMLMarket = InterestRateProduct.CreateFpMLMarketFromCurves(uniqueCurves);
            var valuation = new Valuations.Valuation();
            //  TODO: add Trade Id & Trade data into valuation. (Trade.Id & Trade.TradeHeader.TradeDate)
            //
            AssetValuation assetValuation = InterestRateProduct.CreateAssetValuationFromValuationSet(valuationSet);
            valuation.CreateSwapValuationReport(cache, nameSpace, valuationReportAndProductId, baseParty, tradeRange.Id, tradeRange.TradeDate, floater, fpMLMarket, assetValuation);
            ValuationReport valuationReport = valuation.Get(cache, nameSpace, valuationReportAndProductId);           
            InterestRateProduct.ReplacePartiesInValuationReport(valuationReport, partyIdList);
            InterestRateProduct.AddOtherPartyPayments(valuationReport, otherPartyPaymentList);

            return valuationReportAndProductId;
        }


        internal static Pair<ValuationResultRange, Swap> GetPriceAndGeneratedFpMLSwap(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            ValuationRange valuationRange,
            TradeRange tradeRange, 
            SwapLegParametersRange_Old leg1ParametersRange, 
            List<DetailedCashflowRangeItem> leg1DetailedCashflowsList, 
            List<PrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList)
        {
            InterestRateStream stream1 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg1ParametersRange);//parametric definiton + cashflows schedule
            var swap = SwapFactory.Create(stream1);
            // Update FpML cashflows
            //
            UpdateCashflowsWithDetailedCashflows(stream1.cashflows, leg1DetailedCashflowsList);
            //  Update PE
            //
            if (null != leg1PrincipalExchangeCashflowList)
            {
                CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, leg1PrincipalExchangeCashflowList);
            }
            //  Add bullet payments...
            //
            if (null != leg1AdditionalPaymentList)
            {
                swap.additionalPayment = leg1AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Payer), 
                    receiverPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Receiver), 
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount), 
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }).ToArray();           
            }         
            // Update FpML cashflows with DF,FV,PV, etc (LegParametersRange needed to access curve functionality)
            //
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            //  Update additional payments
            //
            var leg1DiscountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, leg1ParametersRange.DiscountCurve);
            SwapGenerator.UpdatePaymentsAmounts(logger, cache, nameSpace, swap, leg1ParametersRange, leg1DiscountCurve, valuationRange.ValuationDate, paymentCalendar);
            //~  Update additional payments          
            string baseParty = valuationRange.BaseParty;

            return new Pair<ValuationResultRange, Swap>(CreateValuationRange(swap, baseParty), swap);
        }

        public List<PrincipalExchangeCashflowRangeItem> GetPrincipalExchanges(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            SwapLegParametersRange_Old legParametersRange,             
            List<DateTimeDoubleRangeItem> notionalValueItems, 
            ValuationRange valuationRange)
        {
            InterestRateStream interestRateStream;
            if (notionalValueItems.Count > 0)
            {
                var tempList = notionalValueItems.Select(item => new Pair<DateTime, decimal>(item.DateTime, Convert.ToDecimal(item.Value))).ToList();
                NonNegativeSchedule notionalScheduleFpML = NonNegativeScheduleHelper.Create(tempList);
                Currency currency = CurrencyHelper.Parse(legParametersRange.Currency);
                NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalScheduleFpML, currency);
                interestRateStream = GetCashflowsScheduleWithNotionalSchedule(fixingCalendar, paymentCalendar, legParametersRange, amountSchedule);
            }
            else
            {
                interestRateStream = GetCashflowsSchedule(fixingCalendar, paymentCalendar, legParametersRange);
            }
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<PrincipalExchangeCashflowRangeItem>();
            //int periodNumber = 0;
            foreach (PrincipalExchange principleExchange in interestRateStream.cashflows.principalExchange)
            {
                var principalExchangeCashflowRangeItem = new PrincipalExchangeCashflowRangeItem();
                list.Add(principalExchangeCashflowRangeItem);
                principalExchangeCashflowRangeItem.PaymentDate = principleExchange.adjustedPrincipalExchangeDate;
                principalExchangeCashflowRangeItem.Amount = (double)principleExchange.principalExchangeAmount;
                //principalExchangeCashflowRangeItem.PresentValueAmount = MoneyHelper.ToDouble(principleExchange.presentValuePrincipalExchangeAmount);
                //principalExchangeCashflowRangeItem.DiscountFactor = (double)principleExchange.discountFactor;
            }
            return list;
        }

        public List<DetailedCashflowRangeItem> GetDetailedCashflowsWithNotionalSchedule(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            SwapLegParametersRange_Old legParametersRange,            
            List<DateTimeDoubleRangeItem> notionalValueItems, 
            ValuationRange valuationRange)
        {
            var tempList = notionalValueItems.Select(item => new Pair<DateTime, decimal>(item.DateTime, Convert.ToDecimal(item.Value))).ToList();
            NonNegativeSchedule notionalScheduleFpML = NonNegativeScheduleHelper.Create(tempList);
            Currency currency = CurrencyHelper.Parse(legParametersRange.Currency);
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalScheduleFpML, currency);
            InterestRateStream interestRateStream = GetCashflowsScheduleWithNotionalSchedule(fixingCalendar, paymentCalendar, legParametersRange, amountSchedule);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<DetailedCashflowRangeItem>();
            //int periodNumber = 1;
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                var detailedCashflowRangeItem = new DetailedCashflowRangeItem();
                list.Add(detailedCashflowRangeItem);
                detailedCashflowRangeItem.PaymentDate = paymentCalculationPeriod.adjustedPaymentDate;
                detailedCashflowRangeItem.StartDate = PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod);
                detailedCashflowRangeItem.EndDate = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod);
                //detailedCashflowRangeItem.NumberOfDays = PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod);
                //detailedCashflowRangeItem.FutureValue = MoneyHelper.ToDouble(paymentCalculationPeriod.forecastPaymentAmount);
                //detailedCashflowRangeItem.PresentValue = MoneyHelper.ToDouble(paymentCalculationPeriod.presentValueAmount);
                //detailedCashflowRangeItem.DiscountFactor = (double)paymentCalculationPeriod.discountFactor;
                detailedCashflowRangeItem.NotionalAmount = (double)PaymentCalculationPeriodHelper.GetNotionalAmount(paymentCalculationPeriod);
                detailedCashflowRangeItem.CouponType = GetCouponType(paymentCalculationPeriod);
                detailedCashflowRangeItem.Rate = (double)PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod);
                //  If  floating rate - retrieve the spread.
                //
                if (legParametersRange.IsFloatingLegType())
                {
                    detailedCashflowRangeItem.Spread = (double)PaymentCalculationPeriodHelper.GetSpread(paymentCalculationPeriod);
                }
            }

            return list;
        }
   
        #endregion

        #region old or obsolete methods

        internal static ValuationResultRange GetPriceOld(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            SwapLegParametersRange_Old leg1ParametersRange, 
            SwapLegParametersRange_Old leg2ParametersRange, 
            ValuationRange valuationRange)
        {
            string baseParty = valuationRange.BaseParty;

            InterestRateStream stream1 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg1ParametersRange);//pay leg
            InterestRateStream stream2 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg2ParametersRange);//receive leg
            var swap = SwapFactory.Create(stream1, stream2);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream2, leg2ParametersRange, valuationRange);
            ValuationResultRange resultRange = CreateValuationRange(swap, baseParty);

            return resultRange;
        }


        public static List<DetailedCashflowRangeItem> GetDetailedCashflowsTestOnly(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            SwapLegParametersRange_Old legParametersRange, 
            ValuationRange valuationRange)
        {
            InterestRateStream interestRateStream = GetCashflowsSchedule(fixingCalendar, paymentCalendar, legParametersRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);            
            var list = new List<DetailedCashflowRangeItem>();
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                var detailedCashflowRangeItem = new DetailedCashflowRangeItem();
                list.Add(detailedCashflowRangeItem);
                detailedCashflowRangeItem.PaymentDate    = paymentCalculationPeriod.adjustedPaymentDate;
                detailedCashflowRangeItem.StartDate      = PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod);
                detailedCashflowRangeItem.EndDate        = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod);
                //detailedCashflowRangeItem.NumberOfDays   = PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod);                
                //detailedCashflowRangeItem.FutureValue    = MoneyHelper.ToDouble(paymentCalculationPeriod.forecastPaymentAmount);
                //detailedCashflowRangeItem.PresentValue = MoneyHelper.ToDouble(paymentCalculationPeriod.presentValueAmount);
                //detailedCashflowRangeItem.DiscountFactor = (double)paymentCalculationPeriod.discountFactor;               
                detailedCashflowRangeItem.NotionalAmount        = (double)PaymentCalculationPeriodHelper.GetNotionalAmount(paymentCalculationPeriod);
                detailedCashflowRangeItem.CouponType = GetCouponType(paymentCalculationPeriod);
                detailedCashflowRangeItem.Rate           = (double)PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod);
                //  If  floating rate - retrieve a spread.
                //
                if (legParametersRange.IsFloatingLegType())
                {
                    detailedCashflowRangeItem.Spread = (double)PaymentCalculationPeriodHelper.GetSpread(paymentCalculationPeriod);
                }
            }

            return list;
        }


        #endregion

        #region Private methods

        private static ValuationResultRange CreateValuationRange(Swap swap, string baseParty)
        {
            Money payPresentValue     = SwapHelper.GetPayPresentValue(swap, baseParty);
            Money payFutureValue      = SwapHelper.GetPayFutureValue(swap, baseParty);
            Money receivePresentValue = SwapHelper.GetReceivePresentValue(swap, baseParty);
            Money receiveFutureValue  = SwapHelper.GetReceiveFutureValue(swap, baseParty);
            Money swapPresentValue    = SwapHelper.GetPresentValue(swap, baseParty);
            Money swapFutureValue     = SwapHelper.GetFutureValue(swap, baseParty);
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

        private static string GetCouponType(PaymentCalculationPeriod pcalculationPeriod)
        {
            CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(pcalculationPeriod)[0];
            return XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod) ? "Float" : "Fixed";
        }

        private static void UpdateCashflowsWithDetailedCashflows(Cashflows cashflows, IEnumerable<DetailedCashflowRangeItem> listDetailedCashflows/*, bool fixedLeg*/)
        {
            var paymentCalculationPeriods = new List<PaymentCalculationPeriod>();
            foreach (DetailedCashflowRangeItem detailedCashflowRangeItem in listDetailedCashflows)
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
                if (detailedCashflowRangeItem.CouponType == "Fixed")
                {
                    //CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(paymentCalculationPeriod)[0];

                    //if (XsdClassesFieldResolver.CalculationPeriod_HasFixedRate(calculationPeriod))
                    //{
                    //  Fixed->Fixed
                    //
                    //PaymentCalculationPeriodHelper.SetRate(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Rate);

                    XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriod, (decimal)detailedCashflowRangeItem.Rate);
                    //}
                    //else if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
//                    {
//                        //  Float->Fixed
//                        //
//                        PaymentCalculationPeriodHelper.ReplaceFloatingRateWithFixedRate(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Rate);
//                    }
//                    else
//                    {
//                        throw new NotImplementedException();
//                    }

                }
                else if (detailedCashflowRangeItem.CouponType == "Float")
                {
                    //  Create floating rate definiton...
                    //
                    var floatingRateDefinition = new FloatingRateDefinition();
                    //XsdClassesFieldResolver.CalculationPeriod_SetFloatingRateDefinition(calculationPeriod, floatingRateDefinition);
                    calculationPeriod.Item1 = floatingRateDefinition;
                    // After the spread is reset - we need to update calculated rate.
                    //
                    PaymentCalculationPeriodHelper.SetSpread(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Spread);
                }
                else
                {
                    string message = $"Unsupported coupon type '{detailedCashflowRangeItem.CouponType}";
                    throw new System.Exception(message);
                }
                paymentCalculationPeriods.Add(paymentCalculationPeriod);
            }
            cashflows.cashflowsMatchParameters = false;
            cashflows.paymentCalculationPeriod = paymentCalculationPeriods.ToArray();
        }
//        private static void UpdateCashflowsWithDetailedCashflows(Cashflows cashflows, List<DetailedCashflowRangeItem> listDetailedCashflows, bool fixedLeg)
//        {
//            for (int i = 0; i < cashflows.paymentCalculationPeriod.Length; ++i)
//            {
//                PaymentCalculationPeriod paymentCalculationPeriod = cashflows.paymentCalculationPeriod[i];
//                DetailedCashflowRangeItem detailedCashflowRangeItem = listDetailedCashflows[i];
//
//                paymentCalculationPeriod.adjustedPaymentDate = detailedCashflowRangeItem.PaymentDate;
//                PaymentCalculationPeriodHelper.SetCalculationPeriodStartDate(paymentCalculationPeriod, detailedCashflowRangeItem.StartDate);
//                PaymentCalculationPeriodHelper.SetCalculationPeriodEndDate(paymentCalculationPeriod, detailedCashflowRangeItem.EndDate);
//                //PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod, );
//
//                // Update notional amount
//                //
//                PaymentCalculationPeriodHelper.SetNotionalAmount(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.NotionalAmount);
//
//                if (detailedCashflowRangeItem.CouponType == "Fixed")
//                {
//                    CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(paymentCalculationPeriod)[0];
//
//                    if (XsdClassesFieldResolver.CalculationPeriod_HasFixedRate(calculationPeriod))
//                    {
//                        //  Fixed->Fixed
//                        //
//                        PaymentCalculationPeriodHelper.SetRate(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Rate);
//                    }
//                    else if (XsdClassesFieldResolver.CalculationPeriod_HasFloatingRateDefinition(calculationPeriod))
//                    {
//                        //  Float->Fixed
//                        //
//                        PaymentCalculationPeriodHelper.ReplaceFloatingRateWithFixedRate(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Rate);
//                    }
//                    else
//                    {
//                        throw new NotImplementedException();
//                    }
//
//                }
//                else if (detailedCashflowRangeItem.CouponType == "Float")
//                {
//                    // After the spread is reset - we need to update calculated rate.
//                    //
//                    PaymentCalculationPeriodHelper.SetSpread(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Spread);
//                }
//                else
//                {
//                    throw new NotImplementedException();
//                }
//            }
//        }


        private static void CreatePrincipalExchangesFromListOfRanges(Cashflows cashflows, IEnumerable<PrincipalExchangeCashflowRangeItem> principalExchangeRangeList)
        {
            cashflows.principalExchange = (from item in principalExchangeRangeList
                                           where 0 != item.Amount
                                           select PrincipalExchangeHelper.Create(item.PaymentDate, (decimal) item.Amount)).ToArray();
        }

        private static InterestRateStream GetCashflowsSchedule(
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            SwapLegParametersRange_Old legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;            
        }

        private static InterestRateStream GetCashflowsScheduleWithNotionalSchedule(
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            SwapLegParametersRange_Old legParametersRange,          
            NonNegativeAmountSchedule notionalSchedule)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream, notionalSchedule);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        private static List<IRateCurve> GetUniqueCurves(
            ILogger logger, 
            ICoreCache cache, 
            String nameSpace,
            SwapLegParametersRange_Old payLegParametersRange)
        {
            var uniqueCurves = new List<IRateCurve>();
            var curveNames = new[]
                                 {
                                     payLegParametersRange.ForecastCurve,
                                     payLegParametersRange.DiscountCurve
                                 };
            foreach (string curveName in curveNames)
            {
                if (!String.IsNullOrEmpty(curveName) && curveName.ToLower() != "none")
                {
                    var curve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, curveName);
                    if (!uniqueCurves.Contains(curve))
                    {
                        uniqueCurves.Add(curve);
                    }
                }
            }

            return uniqueCurves;
        }

        private static void UpdateCashflowsWithAmounts(
            ILogger logger, 
            ICoreCache cache, 
            String nameSpace,
            InterestRateStream stream, 
            SwapLegParametersRange_Old legParametersRange, 
            ValuationRange valuationRange)
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
            //  Update cashflows & principal exchanges
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountCurve, valuationRange.ValuationDate);
        }

        #endregion

    }
}