#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Constants;
//using Orion.CurveEngine.Tests.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
//using Orion.ModelFramework;
//using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ValuationEngine.Pricers;
//using Orion.CurveEngine.Tests;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
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
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD_6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<DetailedCashflowRangeItem> receiveCFRangeItemList = FloaterPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, receiveFloat, valuationRange);
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg2PrincipalExchangeCashflowList = new List<PrincipalExchangeCashflowRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            List<PartyIdRangeItem> partyList = GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = GetOtherPartyPaymentList("counterparty", "cost center");
            string valuatonId = irFloaterPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                                CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                receiveFloat,
                                                                receiveCFRangeItemList,
                                                                leg2PrincipalExchangeCashflowList,
                                                                leg2BulletPaymentList,
                                                                partyList, otherPartyPaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuatonId);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        [TestMethod]
        public void CreateValuationFixedStream()
        {
            DateTime valuationDate = DateTime.Today;
            var irFloaterPricer = new FloaterPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            //string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old receiveFixedLegParameters = CreateFixedAUD_6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0.08m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<DetailedCashflowRangeItem> receiveCFRangeItemList = FloaterPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, receiveFixedLegParameters, valuationRange);
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg2PrincipleExchangeCashflowList = new List<PrincipalExchangeCashflowRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            List<PartyIdRangeItem> partyList = GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = GetOtherPartyPaymentList("counterparty", "cost center");
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valuatonId = irFloaterPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                                CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                receiveFixedLegParameters,
                                                                receiveCFRangeItemList,
                                                                leg2PrincipleExchangeCashflowList,
                                                                leg2BulletPaymentList,
                                                                partyList, otherPartyPaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuatonId);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));

        }
    }
}
