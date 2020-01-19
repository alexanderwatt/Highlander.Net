using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Confirmation;
using FpML.V5r3.Codes;
using FpML.V5r3.Helpers;
using Orion.Util.Serialisation;

namespace Orion.Configuration
{
    public static class FpMLTradeLoader
    {
        private static string TradeTypeHelper(Product product)
        {
            if (product as Swap != null) return "swap";
            if (product as TermDeposit != null) return "termDeposit";
            if (product as BulletPayment != null) return "bulletPayment";
            if (product as BondOption != null) return "bondOption";
            if (product as BrokerEquityOption != null) return "brokerEquityOption";
            if (product as CapFloor != null) return "capFloor";
            if (product as CommodityForward != null) return "commodityForward";
            if (product as CommodityOption != null) return "commodityOption";
            if (product as CommoditySwap != null) return "commoditySwap";
            if (product as CorrelationSwap != null) return "correlationSwap";
            if (product as CreditDefaultSwap != null) return "creditDefaultSwap";
            if (product as CreditDefaultSwapOption != null) return "creditDefaultSwapOption";
            if (product as DividendSwapTransactionSupplement != null) return "dividendSwapTransactionSupplement";
            if (product as EquityForward != null) return "equityForward";
            if (product as EquityOption != null) return "equityOption";
            if (product as EquityOptionTransactionSupplement != null) return "equityOptionTransactionSupplement";
            if (product as ReturnSwap != null) return "returnSwap";
            if (product as EquitySwapTransactionSupplement != null) return "equitySwapTransactionSupplement";
            if (product as Fra != null) return "fra";
            if (product as FxDigitalOption != null) return "fxDigitalOption";
            if (product as FxOption != null) return "fxOption";
            if (product as FxSingleLeg != null) return "fxSingleLeg";
            if (product as FxSwap != null) return "fxSwap";
            if (product as Strategy != null) return "strategy";
            if (product as Swaption != null) return "swaption";
            if (product as VarianceOptionTransactionSupplement != null) return "varianceOptionTransactionSupplement";
            if (product as VarianceSwap != null) return "varianceSwap";
            if (product as VarianceSwapTransactionSupplement != null) return "varianceSwapTransactionSupplement";
            return "swap";
        }

        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo MakeTradeProps(string type, string idSuffix, NamedValueSet extraProps, string nameSpace)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(EnvironmentProp.DataGroup, "Orion.V5r3.Confirmation." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Orion");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.TradeData);
            itemProps.Set(EnvironmentProp.Type, type);
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Confirmation");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            string itemName = "Orion.V5r3.Confirmation.Products." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(TradeProp.UniqueIdentifier, itemName);
            //itemProps.Set(TradeProp.TradeDate, null);
            //itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static Trade GetTradeObject(string resourceAsString)
        {
            //string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/confirmation");
            XmlNode tradeNode = xmlDocument.SelectSingleNode("//fpml:trade", xmlNamespaceManager);
            var trade = XmlSerializerHelper.DeserializeNode<Trade>(tradeNode);
            trade.id = "TestTrade";
            return trade;
        }

        public static void LoadTrades1(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading trades...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.TradeData.";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing Trades");
            Party[] party = null;
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var tradeVersion = GetTradeObject(file.Value);
                if (tradeVersion != null)
                {
                    //party = tradeData.party;
                    BuildTrade(logger, targetClient, tradeVersion, party, nameSpace); 
                }
                //var document = XmlSerializerHelper.DeserializeFromString<Document>(file.Value);//This does not work!
                //if (document as RequestConfirmation != null)
                //{
                //    var data = (RequestConfirmation)document;
                //    var extraProps = new NamedValueSet();
                //    extraProps.Set(TradeProp.TradingBookName, "Test");
                //    extraProps.Set(TradeProp.TradeState, "Confirmed");
                //    //extraProps.Set("TradeType", "Deposit");
                //    extraProps.Set(TradeProp.TradeSource, "FPMLSamples");
                //    extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
                //    extraProps.Set("Namespace", "Confirmation");
                //    ItemInfo itemInfo = MakeTradeProps(data.GetType().ToString(), null, extraProps);
                //    logger.LogDebug("  {0} ...", itemInfo.ItemName);
                //    targetClient.SaveObject(data, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue); 
                //}
                //else if (document as TradeConfirmed != null)
                //{
                //    var tradeData = (TradeConfirmed)document;
                //    tradeVersion = tradeData.trade;
                //    party = tradeData.party;
                //    BuildTrade(logger, targetClient, tradeVersion, party);
                //}
                //else if (document as RequestTradeConfirmation != null)
                //{
                //    var tradeData = (RequestTradeConfirmation)document;
                //    tradeVersion = tradeData.trade;
                //    party = tradeData.party;
                //    BuildTrade(logger, targetClient, tradeVersion, party);
                //}
                //else if (document as TradeCreated != null)
                //{
                //    var tradeData = (TradeCreated)document;
                //    tradeVersion = tradeData.trade;
                //    party = tradeData.party;
                //    BuildTrade(logger, targetClient, tradeVersion, party);//TradeCancelled
                //}
                //else if (document as TradeCancelled != null)
                //{
                //    var tradeData = (TradeCancelled)document;
                //    tradeVersion = tradeData.Items[0] as Trade;
                //    party = tradeData.party;
                //    BuildTrade(logger, targetClient, tradeVersion, party);//
                //}
                //else if (document as LoanContractNotice != null)
                //{
                //    var tradeData = (LoanContractNotice)document;
                //    tradeVersion = tradeData.Items[0] as Trade;
                //    party = tradeData.party;
                //    BuildTrade(logger, targetClient, tradeVersion, party);
                //}
            } // foreach file
            //as TradeConfirmed
            logger.LogDebug("Loaded {0} trades.", chosenFiles.Count());
        }

        public static void LoadTrades2(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading trades...");
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadAndReturnConfirmationExamples(
                schemaSet, FpMLViewHelpers.AutoDetectType,
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                false, false, false);
            foreach (var document in results)
            {
                var extraProps = new NamedValueSet();
                extraProps.Set(TradeProp.TradingBookName, "Test");
                extraProps.Set(TradeProp.TradeState, "Confirmed");
                //extraProps.Set("TradeType", "Deposit");
                extraProps.Set(TradeProp.TradeSource, "FPMLSamples");
                extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
                extraProps.Set("Namespace", "Confirmation");
                ItemInfo itemInfo = MakeTradeProps(document.GetType().ToString(), null, extraProps, nameSpace);
                logger.LogDebug("  {0} ...", itemInfo.ItemName);
                targetClient.SaveObject(document, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue); 
            } // foreach file
            //as TradeConfirmed
            logger.LogDebug("Loaded {0} trades.", results.Count());
        }

        public static void BuildTrade(ILogger logger, ICoreCache targetClient, Trade tradeVersion, Party[] party, string nameSpace)
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
            var tempId = tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
            if (tempId as VersionedTradeId == null)
            {
                tradeId = (TradeId)tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
            }
            else
            {
                var id = (VersionedTradeId)tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
                tradeId = id.tradeId;
            }
            //tradeId = (TradeId)tradeVersion.tradeHeader.partyTradeIdentifier[0].Items[0];
            extraProps.Set(TradeProp.TradeState, "Pricing");
            //extraProps.Set("TradeType", "Deposit");
            extraProps.Set(TradeProp.TradeSource, "FPMLSamples");
            extraProps.Set(TradeProp.TradeId, tradeId.Value);
            extraProps.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
            extraProps.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
            extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
            var tradeType = TradeTypeHelper(tradeVersion.Item);
            extraProps.Set(TradeProp.TradeType, tradeType);
            //extraProps.Set(TradeProp.ProductType, tradeType.ToString());
            string idSuffix = String.Format("{0}.{1}",
                                            tradeType,
                                            tradeId.Value);
            ItemInfo itemInfo = MakeTradeProps("Trade", idSuffix, extraProps, nameSpace);
            logger.LogDebug("  {0} ...", idSuffix);
            targetClient.SaveObject(tradeVersion, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);      
        }
    }
}
