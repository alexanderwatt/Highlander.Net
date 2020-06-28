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
        public void CreateValuationFloatStream()
        {
            DateTime valuationDate = DateTime.Today;
            var irFloaterPricer = new FloaterPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old receiveFloat = ExcelAPITests.CreateFloatingAUD6MSwapLegParametersRange(ExcelAPITests.CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = ExcelAPITests.CreateValuationRangeForNAB(valuationDate);
            List<DetailedCashflowRangeItem> receiveCFRangeItemList = FloaterPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, receiveFloat, valuationRange);
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg2PrincipalExchangeCashflowList = new List<PrincipalExchangeCashflowRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            List<PartyIdRangeItem> partyList = ExcelAPITests.GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = ExcelAPITests.GetOtherPartyPaymentList("counterparty", "cost center");
            string valuationId = irFloaterPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                                ExcelAPITests.CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                receiveFloat,
                                                                receiveCFRangeItemList,
                                                                leg2PrincipalExchangeCashflowList,
                                                                leg2BulletPaymentList,
                                                                partyList, otherPartyPaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuationId);
            Debug.Print(XmlSerializerHelper.SerializeToString((object) valuationReport));
        }

        [TestMethod]
        public void CreateValuationFixedStream()
        {
            DateTime valuationDate = DateTime.Today;
            var irFloaterPricer = new FloaterPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            //string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old receiveFixedLegParameters = ExcelAPITests.CreateFixedAUD6MSwapLegParametersRange(ExcelAPITests.CounterParty, _NAB, valuationDate, 0.08m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            ValuationRange valuationRange = ExcelAPITests.CreateValuationRangeForNAB(valuationDate);
            List<DetailedCashflowRangeItem> receiveCFRangeItemList = FloaterPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, receiveFixedLegParameters, valuationRange);
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg2PrincipleExchangeCashflowList = new List<PrincipalExchangeCashflowRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            List<PartyIdRangeItem> partyList = ExcelAPITests.GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = ExcelAPITests.GetOtherPartyPaymentList("counterparty", "cost center");
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valuationId = irFloaterPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                                ExcelAPITests.CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                receiveFixedLegParameters,
                                                                receiveCFRangeItemList,
                                                                leg2PrincipleExchangeCashflowList,
                                                                leg2BulletPaymentList,
                                                                partyList, otherPartyPaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuationId);
            Debug.Print(XmlSerializerHelper.SerializeToString((object) valuationReport));

        }
    }
}
