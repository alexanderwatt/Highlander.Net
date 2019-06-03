using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using Orion.Analytics.Helpers;

namespace Orion.ValuationEngine.Valuations
{
    public class Valuation
    {
        public static string AppendValuationIdToFilename(string filename, string valuationId)
        {
            string result = filename.Insert(filename.LastIndexOf('.'), "_" + valuationId);
            return result;
        }

        public List<ValuationInfoRangeItem> GetInfo(ICoreCache cache, string nameSpace, string valuationId)
        {
            var list = new List<ValuationInfoRangeItem>();
            var item = cache.LoadItem<ValuationReport>(nameSpace + "." + valuationId);
            if (item != null)
            {
                var valutionReport = (ValuationReport)item.Data;
                var envelope = new ValuationInfoRangeItem
                                   {
                                       Id = valutionReport.header.messageId.Value,
                                       Description = "envelope"
                                   };
                list.Add(envelope);
                foreach (TradeValuationItem tradeValuationItem in valutionReport.tradeValuationItem)
                {
                    foreach (Trade trade in tradeValuationItem.Items)
                    {
                        var tradeId = trade.tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId;
                        if (tradeId != null)
                        {
                            var product = new ValuationInfoRangeItem
                                {
                                    Id = tradeId.Value
                                };
                            var swap1 = trade.Item as Swap;
                            if (swap1 != null)
                            {
                                product.Description = "swap";
                                var swap = swap1;
                                string leg1Type = GetInterestRateStreamType(swap.swapStream[0]);
                                string leg2Type = GetInterestRateStreamType(swap.swapStream[1]);

                                product.Description += $"({leg1Type}/{leg2Type})";
                            }
                            else if (trade.Item is Swaption)
                            {
                                product.Description = "swaption";
                            }
                            else
                            {
                                var floor = trade.Item as CapFloor;
                                if (floor != null)//could be cap, floor, or collar
                                {
                                    var capFloor = floor;
                                    Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(
                                        capFloor.capFloorStream.calculationPeriodAmount);
                                    FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
                                    if (null != floatingRateCalculation.capRateSchedule & null != floatingRateCalculation.floorRateSchedule)
                                    {
                                        product.Description = "collar";
                                    }
                                    else if (null != floatingRateCalculation.capRateSchedule)
                                    {
                                        product.Description = "cap";
                                    }
                                    else product.Description = null != floatingRateCalculation.floorRateSchedule ? "floor" : "unknown product";
                                }
                                else
                                {
                                    product.Description = "unknown product";
                                }
                            }
                            list.Add(product);
                        }
                    }
                }
            }
            return list;
        }

        private static string GetInterestRateStreamType(InterestRateStream interestRateStream)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);          
            return XsdClassesFieldResolver.CalculationHasFloatingRateCalculation(calculation) ? "float" : "fixed";
        }


        public string Merge(ICoreCache cache, string nameSpace, string valuationId1, string valuationId2, 
                            string valuationId3, string valuationId4, string valuationId5, string valuationId6, 
                            string valuationId7, string valuationId8, string valuationId9, string valuationId10)
        {
            var valuationIds = new List<string>();
            if (!String.IsNullOrEmpty(valuationId1)) valuationIds.Add(valuationId1);
            if (!String.IsNullOrEmpty(valuationId2)) valuationIds.Add(valuationId2);
            if (!String.IsNullOrEmpty(valuationId3)) valuationIds.Add(valuationId3);
            if (!String.IsNullOrEmpty(valuationId4)) valuationIds.Add(valuationId4);
            if (!String.IsNullOrEmpty(valuationId5)) valuationIds.Add(valuationId5);
            if (!String.IsNullOrEmpty(valuationId6)) valuationIds.Add(valuationId6);
            if (!String.IsNullOrEmpty(valuationId7)) valuationIds.Add(valuationId7);
            if (!String.IsNullOrEmpty(valuationId8)) valuationIds.Add(valuationId8);
            if (!String.IsNullOrEmpty(valuationId9)) valuationIds.Add(valuationId9);
            if (!String.IsNullOrEmpty(valuationId10)) valuationIds.Add(valuationId10);
            var item = cache.LoadItem<ValuationReport>(nameSpace + "." + valuationIds[0]);
            var result = (ValuationReport)item.Data;
            valuationIds.RemoveAt(0);
            if (0 == valuationIds.Count)
            {
                string resultValuationIdOnlyOne = result.header.messageId.Value;
                return resultValuationIdOnlyOne;
            }
            result = valuationIds.Select(valuationId => cache.LoadItem<ValuationReport>(nameSpace + "." + valuationId).Data as ValuationReport).Aggregate(result, ValuationReportGenerator.Merge);
            string resultValuationId = result.header.messageId.Value;
            cache.SaveObject(result, nameSpace + "." + resultValuationId, null);//TODO add properties here.
            return resultValuationId;
        }

        public void ReplacePartiesInValuationReport(ICoreCache cache, string nameSpace, string valuationId, List<Party> partyList)
        {
            var valuationReport = cache.LoadItem<ValuationReport>(nameSpace + "." + valuationId).Data as ValuationReport;
            if (valuationReport != null) valuationReport.party = partyList.ToArray();
        }

        public ValuationReport Get(ICoreCache cache, string nameSpace, string valuationId)
        {
            var valuationReport = cache.LoadItem<ValuationReport>(nameSpace + "." + valuationId).Data as ValuationReport;
            return valuationReport;
        }

        public string CreateSwapValuationReport(ICoreCache cache, string nameSpace, string valuationId, string baseParty,
                                                string tradeId, DateTime tradeDate,
                                                Swap swap, Market market, AssetValuation assetValuation)
        {
            ValuationReport valuationReport = ValuationReportGenerator.Generate(valuationId, baseParty, 
                                                                                tradeId, tradeDate,
                                                                                swap, market, assetValuation);
            cache.SaveObject(valuationReport, nameSpace + "." + valuationId, null);
            return valuationId;
        }

        public string CreateFraValuationReport(ICoreCache cache, string nameSpace, string id, string baseParty, Fra fra, Market market, AssetValuation assetValuation)
        {
            ValuationReport valuationReport = ValuationReportGenerator.Generate(id, baseParty, fra, market, assetValuation);
            cache.SaveObject(valuationReport, nameSpace + "." + id, null);
            return id;
        }

        public string CreateFraValuationReport(ICoreCache cache, string nameSpace, string id, string baseParty, Fra fra, Market market, AssetValuation assetValuation, NamedValueSet properties)
        {
            ValuationReport valuationReport = ValuationReportGenerator.Generate(id, baseParty, fra, market, assetValuation);
            cache.SaveObject(valuationReport, nameSpace + "." + id, properties);
            return id;
        }

        public string CreateTradeValuationReport(ICoreCache cache, string nameSpace, string id, string party1, string party2, bool isParty1Base, Trade trade, Market market, AssetValuation assetValuation, NamedValueSet properties)
        {
            ValuationReport valuationReport = ValuationReportGenerator.Generate(id, party1, party2, isParty1Base, trade, market, assetValuation);
            cache.SaveObject(valuationReport, nameSpace + "." + id, properties);
            return id;
        }

        public string LoadValuationReports(ICoreCache cache, string nameSpace)
        {
            var results = cache.LoadItems(typeof(ValuationReport), Expr.ALL);
            foreach (var item in results)
            {
                try
                {
                    var valuationReport = (ValuationReport)item.Data;
                    string valuationId = valuationReport.header.messageId.Value;
                    cache.SaveObject(valuationReport, nameSpace + "." + valuationId, null);
                }
                catch (System.Exception excp)
                {
                    cache.Logger.Log(excp);
                }
            }
            return String.Format("ValuationReports retrieved");
        }

        public string SaveToFile(ICoreCache cache, string nameSpace, string valuationId, string filename)
        {
            var valuationReport = cache.LoadItem<ValuationReport>(nameSpace + "." + valuationId).Data as ValuationReport;
            string actualFilename = AppendValuationIdToFilename(filename, valuationId);
            XmlSerializerHelper.SerializeToFile(typeof(Document), valuationReport, actualFilename);

            return $"FpML document '{valuationId}' successfully saved to '{actualFilename}'";
        }

        public string LoadFromFile(ICoreCache cache, string filename)
        {
            var valuationReport = 
                XmlSerializerHelper.DeserializeFromFile<ValuationReport>(typeof(Document), filename);
            cache.SaveObject(valuationReport, "someId", null);
            return "Valuation successfully loaded.";
        }
    }
}