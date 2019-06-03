using System;
using System.Linq;
using Core.Common;
using FpML.V5r3.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;

namespace Orion.Configuration
{
    public static class FpMLTradeLoader
    {
        ////Interest rate examples
        //private const string _swap_01_vanilla_swap_example = "interest_rate_derivatives.ird-ex01-vanilla-swap.xml";
        //private const string _swap_02_stub_amort_example = "interest_rate_derivatives.ird-ex02-stub-amort-swap.xml";
        //private const string _swap_03_AUD_example = "interest_rate_derivatives.ird-ex03-compound-swap.xml";
        //private const string _swap_04_stepup_fee_swap_example = "interest_rate_derivatives.ird-ex04-arrears-stepup-fee-swap.xml";
        //private const string _swap_05_long_stub_example = "interest_rate_derivatives.ird-ex05-long-stub-swap.xml";
        //private const string _swap_06_xccy_swap_example = "interest_rate_derivatives.ird-ex06-xccy-swap.xml";
        //private const string _swap_09_ois_example = "interest_rate_derivatives.ird-ex07-ois-swap.xml";
        //private const string _swap_10_fra_example = "interest_rate_derivatives.ird-ex08-fra.xml";
        //private const string _bullet_28_bullet_payments_example = "interest_rate_derivatives.ird-ex28-bullet-payments.xml";
        //private const string _cap_22_cap_example = "interest_rate_derivatives.ird-ex22-cap.xml";
        //private const string _floor_23_cap_example = "interest_rate_derivatives.ird-ex23-floor.xml";
        //private const string _collar_24_cap_example = "interest_rate_derivatives.ird-ex24-collar.xml";

        ////FX examples
        //private const string _fx_08_fx_swap_example = "fx_derivatives.fx-ex08-fx-swap.xml";
        //private const string _swap_07_fx_forward_example = "fx_derivatives.fx-ex03-fx-fwd.xml";
        //private const string _swap_08_fx_forward_example = "fx_derivatives.fx-ex01-fx-spot.xml";
        //private const string _td_01_simple_term_deposit_example = "fx_derivatives.td-ex02-term-deposit-w-settlement-etc.xml";
        //private const string _td_02_term_deposit_w_settlement_example = "fx_derivatives.td-ex01-simple-term-deposit.xml";

        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo MakeTradeProps(string type, string idSuffix, NamedValueSet extraProps)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(TradeProp.DataGroup, "Orion." + type);
            itemProps.Set("SourceSystem", "Orion");
            itemProps.Set("Function", "Trade");
            itemProps.Set("Type", type);
            itemProps.Set("Schema", "FpML.V5r3");
            string itemName = "Orion.V5r3.Confirmation.Products." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(TradeProp.UniqueIdentifier, itemName);
            //itemProps.Set(TradeProp.TradeDate, null);
            //itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static void SaveDocument(ILogger logger, ICoreCache targetClient, object fpMLProduct)
        {
            var extraProps = new NamedValueSet();
            extraProps.Set(TradeProp.TradingBookName, "Test");
            extraProps.Set(TradeProp.TradeState, "Confirmed");
            //extraProps.Set("TradeType", "Deposit");
            extraProps.Set(TradeProp.TradeSource, "FPMLSamples");
            extraProps.Set(TradeProp.AsAtDate, DateTime.Today);
            extraProps.Set("Namespace", "Confirmation");
            ItemInfo itemInfo = MakeTradeProps(fpMLProduct.GetType().ToString(), null, extraProps);
            logger.LogDebug("  {0} ...", itemInfo.ItemName);
            targetClient.SaveObject(fpMLProduct, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);
        }

        public static void LoadTrades(ILogger logger, ICoreCache targetClient)
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
                SaveDocument(logger, targetClient, document);
                //var result = TestHelper.DeserialiseXml(testLog, testId, typeDetector, sourcePath, counters.DeserialisationErrors)
                //var document = XmlSerializerHelper.DeserializeFromString<Document>(file);//This does not work!
                //if (document as DataDocument != null)
                //{
                //    var tradeData = (DataDocument)document;
                //    Trade tradeVersion = tradeData.trade[0];
                //    if (tradeVersion != null)
                //    {
                //        Party[] party = tradeData.party;
                //        BuildTrade(logger, targetClient, tradeVersion, party);
                //    }
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
            logger.LogDebug("Loaded {0} trades.", results.Count());
        }

        public static void BuildTrade(ILogger logger, ICoreCache targetClient, Trade tradeVersion, Party[] party)
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
            var tradeType = ProductTypeHelper.TradeTypeHelper(tradeVersion.Item);
            var currencies = tradeVersion.Item.GetRequiredCurrencies().ToArray();
            var curves = tradeVersion.Item.GetRequiredPricingStructures().ToArray();
            extraProps.Set(TradeProp.RequiredPricingStructures, curves);
            extraProps.Set(TradeProp.RequiredCurrencies, currencies);
            extraProps.Set(TradeProp.TradeType, tradeType.ToString());
            //extraProps.Set(TradeProp.ProductType, tradeType.ToString());
            string idSuffix = String.Format("{0}.{1}",
                                            tradeType,
                                            tradeId.Value);
            ItemInfo itemInfo = MakeTradeProps("Trade", idSuffix, extraProps);
            logger.LogDebug("  {0} ...", idSuffix);
            targetClient.SaveObject(tradeVersion, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);      
        }
    }
}
