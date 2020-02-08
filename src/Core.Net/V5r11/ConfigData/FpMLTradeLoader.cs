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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Exception = System.Exception;

namespace Highlander.Configuration.Data.V5r3
{
    public static class FpMLTradeLoader
    {
        private static string TradeTypeHelper(Confirmation.V5r3.Product product)
        {
            if (product is Confirmation.V5r3.Swap) return "swap";
            if (product is Confirmation.V5r3.TermDeposit) return "termDeposit";
            if (product is Confirmation.V5r3.BulletPayment) return "bulletPayment";
            if (product is Confirmation.V5r3.BondOption) return "bondOption";
            if (product is Confirmation.V5r3.BrokerEquityOption) return "brokerEquityOption";
            if (product is Confirmation.V5r3.CapFloor) return "capFloor";
            if (product is Confirmation.V5r3.CommodityForward) return "commodityForward";
            if (product is Confirmation.V5r3.CommodityOption) return "commodityOption";
            if (product is Confirmation.V5r3.CommoditySwaption) return "commoditySwaption";
            if (product is Confirmation.V5r3.CommoditySwap) return "commoditySwap";
            if (product is Confirmation.V5r3.CorrelationSwap) return "correlationSwap";
            if (product is Confirmation.V5r3.CreditDefaultSwap) return "creditDefaultSwap";
            if (product is Confirmation.V5r3.CreditDefaultSwapOption) return "creditDefaultSwapOption";
            if (product is Confirmation.V5r3.DividendSwapTransactionSupplement) return "dividendSwapTransactionSupplement";
            if (product is Confirmation.V5r3.EquityForward) return "equityForward";
            if (product is Confirmation.V5r3.EquityOption) return "equityOption";
            if (product is Confirmation.V5r3.EquityOptionTransactionSupplement) return "equityOptionTransactionSupplement";
            if (product is Confirmation.V5r3.ReturnSwap) return "returnSwap";
            if (product is Confirmation.V5r3.EquitySwapTransactionSupplement) return "equitySwapTransactionSupplement";
            if (product is Confirmation.V5r3.Fra ) return "fra";
            if (product is Confirmation.V5r3.FxDigitalOption) return "fxDigitalOption";
            if (product is Confirmation.V5r3.FxOption) return "fxOption";
            if (product is Confirmation.V5r3.FxSingleLeg) return "fxSingleLeg";
            if (product is Confirmation.V5r3.FxSwap) return "fxSwap";
            if (product is Confirmation.V5r3.Strategy) return "strategy";
            if (product is Confirmation.V5r3.Swaption) return "swaption";
            if (product is Confirmation.V5r3.VarianceOptionTransactionSupplement) return "varianceOptionTransactionSupplement";
            if (product is Confirmation.V5r3.VarianceSwap) return "varianceSwap";
            if (product is Confirmation.V5r3.VarianceSwapTransactionSupplement) return "varianceSwapTransactionSupplement";
            return "swap";
        }

        private static string TradeTypeHelper(Product product)
        {
            if (product is Swap) return "swap";
            if (product is TermDeposit) return "termDeposit";
            if (product is BulletPayment) return "bulletPayment";
            if (product is BondOption) return "bondOption";
            if (product is BrokerEquityOption) return "brokerEquityOption";
            if (product is CapFloor) return "capFloor";
            if (product is CommodityForward) return "commodityForward";
            if (product is CommodityOption) return "commodityOption";
            if (product is CommoditySwaption) return "commoditySwaption";
            if (product is CommoditySwap) return "commoditySwap";
            if (product is CorrelationSwap) return "correlationSwap";
            if (product is CreditDefaultSwap) return "creditDefaultSwap";
            if (product is CreditDefaultSwapOption) return "creditDefaultSwapOption";
            if (product is DividendSwapTransactionSupplement) return "dividendSwapTransactionSupplement";
            if (product is EquityForward) return "equityForward";
            if (product is EquityOption) return "equityOption";
            if (product is EquityOptionTransactionSupplement) return "equityOptionTransactionSupplement";
            if (product is ReturnSwap) return "returnSwap";
            if (product is EquitySwapTransactionSupplement) return "equitySwapTransactionSupplement";
            if (product is Fra) return "fra";
            if (product is FxDigitalOption) return "fxDigitalOption";
            if (product is FxOption) return "fxOption";
            if (product is FxSingleLeg) return "fxSingleLeg";
            if (product is FxSwap) return "fxSwap";
            if (product is Strategy) return "strategy";
            if (product is Swaption) return "swaption";
            if (product is VarianceOptionTransactionSupplement) return "varianceOptionTransactionSupplement";
            if (product is VarianceSwap) return "varianceSwap";
            if (product is VarianceSwapTransactionSupplement) return "varianceSwapTransactionSupplement";
            return "swap";
        }

        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo MakeConfirmationTradeProps(string type, string idSuffix, NamedValueSet extraProps, string nameSpace)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(EnvironmentProp.DataGroup, nameSpace + ".Confirmation." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Murex");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            itemProps.Set(EnvironmentProp.Type, type);
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Confirmation");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            string itemName = nameSpace + "." + type + ".Confirmation.Murex" + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(TradeProp.UniqueIdentifier, itemName);
            //itemProps.Set(TradeProp.TradeDate, null);
            //itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        private static ItemInfo MakeReportingTradeProps(string type, string idSuffix, NamedValueSet extraProps, string nameSpace)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(EnvironmentProp.DataGroup, nameSpace + ".Reporting." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Murex");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            itemProps.Set(EnvironmentProp.Type, type);
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Reporting");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            string itemName = nameSpace + "." + type + ".Reporting.Murex" + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(TradeProp.UniqueIdentifier, itemName);
            //itemProps.Set(TradeProp.TradeDate, null);
            //itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static Confirmation.V5r3.Trade GetTradeObject(string resourceAsString)
        {
            //string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/confirmation");
            XmlNode tradeNode = xmlDocument.SelectSingleNode("//fpml:trade", xmlNamespaceManager);
            var trade = XmlSerializerHelper.DeserializeNode<Confirmation.V5r3.Trade>(tradeNode);
            trade.id = resourceAsString;
            return trade;
        }

        //public static Confirmation.V5r3.RequestConfirmation GetTradeObjectUsingCast(string resourceAsString)
        //{
        //    var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.RequestConfirmation>(resourceAsString);
        //    return confirmation;
        //}

        public static Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade> ExtractTradeInfoFromRootNode(string resourceAsString)
        {
            try
            {
                var element =  XElement.Parse(resourceAsString);
                var localName = element.Name.LocalName;
                //switch (localName)
                //{
                    if(localName=="requestConfirmation")
                    {
                        var confirmation1 = XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.RequestConfirmation>(resourceAsString);
                        if (confirmation1 != null)
                        {
                            var result = new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation1.party, confirmation1.trade);
                            return result;
                        }
                    }
                    if (localName == "dataDocument")
                    {
                        var confirmation2 =
                            XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.DataDocument>(
                                resourceAsString);
                        if (confirmation2 != null)
                        {
                            var result =
                                new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(
                                    confirmation2.party,
                                    confirmation2.trade[0]);
                            return result;
                        }
                    }
                    if (localName == "confirmationAgreed")
                    {
                        var confirmation3 =
                            XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ConfirmationAgreed>(
                                resourceAsString);
                        if (confirmation3 != null)
                        {
                            var result =
                                new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(
                                    confirmation3.party,
                                    confirmation3.trade);
                            return result;
                        }
                    }
                    if (localName == "executionNotification")
                    {
                        var confirmation4 =
                            XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ExecutionNotification>(
                                resourceAsString);
                        if (confirmation4 != null)
                        {
                            var result =
                                new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(
                                    confirmation4.party,
                                    confirmation4.trade);
                            return result;
                        }
                    }
                    if (localName == "executionRetracted")
                    {
                        var confirmation5 =
                            XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ExecutionRetracted>(
                                resourceAsString);
                        if (confirmation5 != null)
                        {
                            var result =
                                new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(
                                    confirmation5.party,
                                    confirmation5.trade);
                            return result;
                        }
                    }
                    return null;
            }
            catch (Exception)
            {
                throw new Exception("This trade has not been serialized.");
            }
        }

        public static Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade> ExtractTradeInfo(string resourceAsString)
        {
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.RequestConfirmation>(resourceAsString);
                if (confirmation != null)
                {
                    var result = new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation.party, confirmation.trade);
                    return result;
                }
            }
            catch
            {
                throw new Exception("This trade has not been serialized.");
            }
            try
            {
                var confirmation =
                    XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.DataDocument>(resourceAsString);
                if (confirmation != null)
                {
                    var result =
                        new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation.party,
                            confirmation.trade[0]);
                    return result;
                }
            }
            catch
            {
                throw new Exception("This trade has not been serialized.");
            }
            try
            {
                var confirmation =
                    XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ConfirmationAgreed>(
                        resourceAsString);
                if (confirmation != null)
                {
                    var result =
                        new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation.party,
                            confirmation.trade);
                    return result;
                }
            }
            catch
            {
                throw new Exception("This trade has not been serialized.");
            }
            try
            {
                var confirmation =
                    XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ExecutionNotification>(
                        resourceAsString);
                if (confirmation != null)
                {
                    var result =
                        new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation.party,
                            confirmation.trade);
                    return result;
                }
            }
            catch
            {
                throw new Exception("This trade has not been serialized.");
            }
            try
            {
                var confirmation =
                    XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.ExecutionRetracted>(
                        resourceAsString);
                if (confirmation != null)
                {
                    var result =
                        new Pair<Confirmation.V5r3.Party[], Confirmation.V5r3.Trade>(confirmation.party,
                            confirmation.trade);
                    return result;
                }
            }
            catch
            {
                throw new Exception("This trade has not been serialized.");
            }
            return null;
        }

        public static void LoadTrades1(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading trades...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Highlander.Configuration.Data.V5r3.TradeData";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing Trades");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var confirmation = ExtractTradeInfoFromRootNode(file.Value);//TODO check ExtractTradeInfo! 
                var tradeVersion = confirmation.Second;//GetTradeObject(file.Value);
                if (tradeVersion != null)
                {
                    var party = confirmation.First;
                    BuildBothTrades(logger, targetClient, nameSpace, tradeVersion, party); 
                }
            } // foreach file
            //as TradeConfirmed
            logger.LogDebug("Loaded {0} trades.", chosenFiles.Count);
        }

        public static void BuildTrade(ILogger logger, ICoreCache targetClient, string nameSpace, Trade tradeVersion, Party[] party)
        {
            var extraProps = new NamedValueSet();
            var party1 = party[0].partyId[0].Value;
            var party2 = party[1].partyId[0].Value;
            extraProps.Set(TradeProp.Party1, party1);
            extraProps.Set(TradeProp.Party2, party2);
            //extraProps.Set(TradeProp.CounterPartyId, party2);//Redundant
            //extraProps.Set(TradeProp.OriginatingPartyId, party1);//Redundant
            extraProps.Set(TradeProp.TradeDate, tradeVersion.tradeHeader.tradeDate.Value);
            extraProps.Set(TradeProp.TradingBookName, "Test");
            TradeId tradeId;
            if (tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] is VersionedTradeId tempId)
            {
                tradeId = tempId.tradeId;
                extraProps.Set(TradeProp.TradeId, tradeId.Value);
            }
            else
            {
                tradeId = tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] as TradeId ??
                          new TradeId { Value = "temp001" };
                extraProps.Set(TradeProp.TradeId, tradeId.Value);
            }
            //tradeId = (TradeId)tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
            extraProps.Set(TradeProp.TradeState, "Pricing");
            //extraProps.Set("TradeType", "Deposit");
            extraProps.Set(TradeProp.TradeSource, "FPMLSamples");
            extraProps.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
            extraProps.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
            extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
            var product = tradeVersion.Item;
            var tradeType = TradeTypeHelper(product);
            extraProps.Set(TradeProp.TradeType, tradeType);
            extraProps.Set(TradeProp.ProductType, tradeType);//TODO this should be a product type...
            //Get the required currencies
            var currencies = tradeVersion.Item.GetRequiredCurrencies().ToArray();
            var curveNames = tradeVersion.Item.GetRequiredPricingStructures().ToArray();
            extraProps.Set(TradeProp.RequiredCurrencies, currencies);
            extraProps.Set(TradeProp.RequiredPricingStructures, curveNames);
            string idSuffix = $"{tradeType}.{tradeId.Value}";
            ItemInfo itemInfo2 = MakeReportingTradeProps("Trade", idSuffix, extraProps, nameSpace);
            logger.LogDebug("  {0} ...", idSuffix);
            targetClient.SaveObject(tradeVersion, itemInfo2.ItemName, itemInfo2.ItemProps, false, TimeSpan.MaxValue);
        } 

        public static void BuildBothTrades(ILogger logger, ICoreCache targetClient, string nameSpace, Confirmation.V5r3.Trade tradeVersion, Confirmation.V5r3.Party[] party)
        {
            var extraProps = new NamedValueSet();
            var party1 = party[0].partyId[0].Value;
            var party2 = party[1].partyId[0].Value;
            extraProps.Set(TradeProp.Party1, party1);
            extraProps.Set(TradeProp.Party2, party2);
            extraProps.Set(TradeProp.TradeDate, tradeVersion.tradeHeader.tradeDate.Value);
            extraProps.Set(TradeProp.TradingBookName, "Test");
            Confirmation.V5r3.TradeId tradeId;
            if (tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] is Confirmation.V5r3.VersionedTradeId tempId)
            {
                tradeId = tempId.tradeId;
                extraProps.Set(TradeProp.TradeId, tradeId.Value);
            }
            else
            {
                tradeId = tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] as Confirmation.V5r3.TradeId ??
                          new Confirmation.V5r3.TradeId {Value = "temp001"};
                extraProps.Set(TradeProp.TradeId, tradeId.Value);
            }
            //tradeId = (TradeId)tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
            extraProps.Set(TradeProp.TradeState, "Pricing");
            //extraProps.Set("TradeType", "Deposit");
            extraProps.Set(TradeProp.TradeSource, "FPMLSamples");         
            extraProps.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
            extraProps.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
            extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
            var product = tradeVersion.Item;
            var tradeType = TradeTypeHelper(product);
            extraProps.Set(TradeProp.TradeType, tradeType);
            extraProps.Set(TradeProp.ProductType, tradeType);//TODO this should be a product type...
            //Get the required currencies
            //1. need to convert to Reporting namespace, where the functionality exists.
            var xml = XmlSerializerHelper.SerializeToString(tradeVersion); 
            var newxml = xml.Replace("FpML-5/confirmation", "FpML-5/reporting");
            var reportingTrade = XmlSerializerHelper.DeserializeFromString<Trade>(newxml);
            var currencies = reportingTrade.Item.GetRequiredCurrencies().ToArray();
            var curveNames = reportingTrade.Item.GetRequiredPricingStructures().ToArray();
            extraProps.Set(TradeProp.RequiredCurrencies, currencies);
            extraProps.Set(TradeProp.RequiredPricingStructures, curveNames);
            string idSuffix = $"{tradeType}.{tradeId.Value}";
            ItemInfo itemInfo = MakeConfirmationTradeProps("Trade", idSuffix, extraProps, nameSpace);
            logger.LogDebug("  {0} ...", idSuffix);
            targetClient.SaveObject(tradeVersion, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);
            ItemInfo itemInfo2 = MakeReportingTradeProps("Trade", idSuffix, extraProps, nameSpace);
            logger.LogDebug("  {0} ...", idSuffix);
            targetClient.SaveObject(reportingTrade, itemInfo2.ItemName, itemInfo2.ItemProps, false, TimeSpan.MaxValue); 
        }
    }
}
