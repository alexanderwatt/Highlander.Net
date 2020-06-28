/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Diagnostics;
using Highlander.Constants;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;
using Highlander.ValuationEngine.V5r3.Pricers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    public partial class ExcelAPITests
    {
        [TestMethod]
        public void CreateSwaptionValuation()
        {
            DateTime valuationDate = DateTime.Today;

            SwaptionPricer irSwaptionPricer = new InterestRateSwaptionPricer();

            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;

            SwapLegParametersRange_Old payFixed = ExcelAPITests.CreateFixedAUD6MSwapLegParametersRange(_NAB, ExcelAPITests.CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = ExcelAPITests.CreateFloatingAUD6MSwapLegParametersRange(ExcelAPITests.CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = ExcelAPITests.CreateValuationRangeForNAB(valuationDate);
            var payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            payCFRangeItemList[0].CouponType = "fixed";// that should test case insensitive nature of coupons
            payCFRangeItemList[1].CouponType = "Fixed";//
            var receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            receiveCFRangeItemList[0].CouponType = "float";// that should test case insensitive nature of coupons
            receiveCFRangeItemList[1].CouponType = "Float";//          
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg1PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg2PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var swaptionParametersRange = new SwaptionParametersRange
                {
                    Premium = 456789.12m,
                    PremiumCurrency = "AUD",
                    PremiumPayer = ExcelAPITests.CounterParty,
                    PremiumReceiver = _NAB,
                    ExpirationDate = valuationDate.AddDays(10),
                    ExpirationDateCalendar = "AUSY-GBLO",
                    ExpirationDateBusinessDayAdjustments = "FOLLOWING",
                    PaymentDate = valuationDate.AddDays(20),
                    PaymentDateCalendar = "USNY-GBLO",
                    PaymentDateBusinessDayAdjustments = "MODFOLLOWING",
                    EarliestExerciseTime = new TimeSpan(10, 0, 0).TotalDays,
                    ExpirationTime = new TimeSpan(11, 0, 0).TotalDays,
                    AutomaticExcercise = false
                };
            List<PartyIdRangeItem> partyList = ExcelAPITests.GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = ExcelAPITests.GetOtherPartyPaymentList("counterparty", "cost center");
            List<FeePaymentRangeItem> feePaymentRangeItems = ExcelAPITests.GetFeeList("counterparty", "book");          
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valuatonId = irSwaptionPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null,
                swaptionParametersRange,
                ExcelAPITests.CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                payFixed, receiveFloat,
                payCFRangeItemList, receiveCFRangeItemList,
                leg1PrincipalExchangeCashflowList, leg2PrincipalExchangeCashflowList,
                leg1BulletPaymentList, leg2BulletPaymentList,
                partyList, otherPartyPaymentRangeItems,
                feePaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuatonId);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }
    }
}
