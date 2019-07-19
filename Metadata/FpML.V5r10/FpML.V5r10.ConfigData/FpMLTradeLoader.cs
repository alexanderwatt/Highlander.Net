/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

namespace FpML.V5r10.ConfigData
{
    public static class FpMLTradeLoader
    {
        private static string TradeTypeHelper(Confirmation.Product product)
        {
            if (product is Confirmation.Swap) return "swap";
            if (product is Confirmation.TermDeposit) return "termDeposit";
            if (product is Confirmation.BulletPayment) return "bulletPayment";
            if (product is Confirmation.BondOption) return "bondOption";
            if (product is Confirmation.BrokerEquityOption) return "brokerEquityOption";
            if (product is Confirmation.CapFloor) return "capFloor";
            if (product is Confirmation.CommodityForward) return "commodityForward";
            if (product is Confirmation.CommodityOption) return "commodityOption";
            if (product is Confirmation.CommoditySwaption) return "commoditySwaption";
            if (product is Confirmation.CommoditySwap) return "commoditySwap";
            if (product is Confirmation.CorrelationSwap) return "correlationSwap";
            if (product is Confirmation.CreditDefaultSwap) return "creditDefaultSwap";
            if (product is Confirmation.CreditDefaultSwapOption) return "creditDefaultSwapOption";
            if (product is Confirmation.DividendSwapTransactionSupplement) return "dividendSwapTransactionSupplement";
            if (product is Confirmation.EquityForward) return "equityForward";
            if (product is Confirmation.EquityOption) return "equityOption";
            if (product is Confirmation.EquityOptionTransactionSupplement) return "equityOptionTransactionSupplement";
            if (product is Confirmation.ReturnSwap) return "returnSwap";
            if (product is Confirmation.EquitySwapTransactionSupplement) return "equitySwapTransactionSupplement";
            if (product is Confirmation.Fra) return "fra";
            if (product is Confirmation.FxDigitalOption) return "fxDigitalOption";
            if (product is Confirmation.FxOption) return "fxOption";
            if (product is Confirmation.FxSingleLeg) return "fxSingleLeg";
            if (product is Confirmation.FxSwap) return "fxSwap";
            if (product is Confirmation.Strategy) return "strategy";
            if (product is Confirmation.Swaption) return "swaption";
            if (product is Confirmation.VarianceOptionTransactionSupplement) return "varianceOptionTransactionSupplement";
            if (product is Confirmation.VarianceSwap) return "varianceSwap";
            if (product is Confirmation.VarianceSwapTransactionSupplement) return "varianceSwapTransactionSupplement";
            return "swap";
        }

        private static string TradeTypeHelper(Product product)
        {
            if (product is Swap) return "swap";
            if (product is TermDeposit) return "termDeposit";
            if (product is BulletPayment) return "bulletPayment";
            if (product is BondOption) return "bondOption";
            if (product is BondTransaction) return "bondTransaction";
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
            if (product is EquityTransaction) return "equityTransaction";
            if (product is EquityForward) return "equityForward";
            if (product is EquityOption) return "equityOption";
            if (product is EquityOptionTransactionSupplement) return "equityOptionTransactionSupplement";
            if (product is FutureTransaction) return "futureTransaction";
            if (product is PropertyTransaction) return "propertyTransaction";
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
            itemProps.Set(EnvironmentProp.Schema, "V5r10.Confirmation");
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
            itemProps.Set(EnvironmentProp.Schema, "V5r10.Reporting");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            string itemName = nameSpace + "." + type + ".Reporting.Murex" + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(TradeProp.UniqueIdentifier, itemName);
            //itemProps.Set(TradeProp.TradeDate, null);
            //itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static Confirmation.Trade GetTradeObject(string resourceAsString)
        {
            //string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/confirmation");
            XmlNode tradeNode = xmlDocument.SelectSingleNode("//fpml:trade", xmlNamespaceManager);
            var trade = XmlSerializerHelper.DeserializeNode<Confirmation.Trade>(tradeNode);
            trade.id = resourceAsString;
            return trade;
        }

        //public static Confirmation.RequestConfirmation GetTradeObjectUsingCast(string resourceAsString)
        //{
        //    var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.RequestConfirmation>(resourceAsString);
        //    return confirmation;
        //}

        public static Pair<Confirmation.Party[], Confirmation.Trade> ExtractTradeInfo(string resourceAsString)
        {
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.RequestConfirmation>(resourceAsString);
                if (confirmation?.Items?[0] is Confirmation.Trade trade)
                {
                    var result =
                        new Pair<Confirmation.Party[], Confirmation.Trade>(confirmation.party, trade);
                    return result;
                }
            }
            catch
            {
            }
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.DataDocument>(resourceAsString);
                if (confirmation != null)
                {
                    var result = new Pair<Confirmation.Party[], Confirmation.Trade>(confirmation.party, confirmation.trade[0]);
                    return result;
                }
            }
            catch
            {}
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.ConfirmationAgreed>(resourceAsString);
                if (confirmation?.Items?[0] is Confirmation.Trade trade)
                {
                    var result = new Pair<Confirmation.Party[], Confirmation.Trade>(confirmation.party, trade);
                    return result;
                }
            }
            catch
            { }
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.ExecutionNotification>(resourceAsString);
                if (confirmation?.Items?[0] is Confirmation.Trade trade)
                {
                    var result = new Pair<Confirmation.Party[], Confirmation.Trade>(confirmation.party, trade);
                    return result;
                }
            }
            catch
            { }
            try
            {
                var confirmation = XmlSerializerHelper.DeserializeFromString<Confirmation.ExecutionRetracted>(resourceAsString);
                if (confirmation?.Items?[0] is Confirmation.Trade trade)
                {
                    var result = new Pair<Confirmation.Party[], Confirmation.Trade>(confirmation.party, trade);
                    return result;
                }
            }
            catch
            { }
            return null;
        }

        public static void LoadTrades1(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading trades...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "FpML.V5r10.ConfigData.TradeData.";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing Trades");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var confirmation = ExtractTradeInfo(file.Value);//TODO check this!
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

        private static void BuildTrade(ILogger logger, ICoreCache targetClient, string nameSpace, Trade tradeVersion, Party[] party)
        {
            var extraProps = new NamedValueSet();
            var party1 = party[0].Items?[0] as PartyId;
            var party2 = party[1].Items?[0] as PartyId;
            if (party1 != null) extraProps.Set(TradeProp.Party1, party1.Value);
            if (party2 != null) extraProps.Set(TradeProp.Party2, party2.Value);
            //if (party2 != null) extraProps.Set(TradeProp.CounterPartyId, party2.Value); //Redundant
            //if (party1 != null) extraProps.Set(TradeProp.OriginatingPartyId, party1.Value); //Redundant
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

        public static void BuildBothTrades(ILogger logger, ICoreCache targetClient, string nameSpace, Confirmation.Trade tradeVersion, Confirmation.Party[] party)
        {
            var extraProps = new NamedValueSet();
            var party1 = party[0].Items?[0] as PartyId;
            var party2 = party[1].Items?[0] as PartyId;
            if (party1 != null) extraProps.Set(TradeProp.Party1, party1.Value);
            if (party2 != null) extraProps.Set(TradeProp.Party2, party2.Value);
            extraProps.Set(TradeProp.TradeDate, tradeVersion.tradeHeader.tradeDate.Value);
            extraProps.Set(TradeProp.TradingBookName, "Test");
            Confirmation.TradeId tradeId;
            if (tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] is Confirmation.VersionedTradeId tempId)
            {
                tradeId = tempId.tradeId;
                extraProps.Set(TradeProp.TradeId, tradeId.Value);
            }
            else
            {
                tradeId = tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[1] as Confirmation.TradeId ??
                          new Confirmation.TradeId {Value = "temp001"};
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
