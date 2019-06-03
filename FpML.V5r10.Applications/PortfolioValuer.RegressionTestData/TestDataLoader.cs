using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using FpML.V5r3.Codes;
using FpML.V5r3.Confirmation;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;


namespace PortfolioValuer.Regression.Data
{
    public static class RegressionTestDataLoader
    {
        private const string ResourcePath = "PortfolioValuer.Regression.Data";
        public static string GetExpectedResults(string resourceFileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return ResourceHelper.GetResource(assembly, ResourcePath + ".ExpectedResults." + resourceFileName);
        }

        private static void LoadFiles<T>(
            ILogger logger, 
            ICoreCache client,
            Assembly assembly,
            string filenamePrefix) where T : class
        {
            logger.LogDebug("Loading {0} files...", typeof(T).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, ResourcePath + "." + filenamePrefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException(String.Format("No {0} files found!", typeof(T).Name));

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<T>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count(), typeof(T).Name);
        }

        private static void LoadFPMLTrades(
            ILogger logger,
            ICoreCache client,
            Assembly assembly,
            string sourceSystem)
        {
            logger.LogDebug("Loading FPML trades...");
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, ResourcePath + "." + sourceSystem, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException(String.Format("No trades found!"));

            // load gwml-to-fpml mappings
            //EnumMaps enumMaps = client.LoadObject<EnumMaps>("Orion.Configuration.GwmlEnumMaps");
            //GwmlFpmlEnumMap gwmlFpmlEnumMap = new GwmlFpmlEnumMap(enumMaps);
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                string fpmlText = file.Value;
                // convert gwml
                //var fpmlDoc = XmlSerializerHelper.DeserializeFromString<DataDocument>(fpmlText);
                var tradeConfirmed = XmlSerializerHelper.DeserializeFromString<RequestConfirmation>(fpmlText);
                var trade = tradeConfirmed.trade;
                //Converter imp = new Converter(gwmlFpmlEnumMap);
                var tradeProps = new NamedValueSet();
                tradeProps.Set(TradeProp.AsAtDate, DateTime.Today);
                tradeProps.Set(TradeProp.TradeSource, sourceSystem);
               // Document fpmlDoc = imp.ToFpml(gwmlDoc, tradeProps);
                // get trade properties
                //Trade trade = (Trade)(((DataDocument)fpmlDoc).Items[0]);
                ProductTypeSimpleEnum productType = ProductTypeSimpleScheme.ParseEnumString(trade.Item.Items[0].ToString());//TODO fix this
                DateTime tradeDate = trade.tradeHeader.tradeDate.Value;
                // now we can set the actual item name correctly
                string fpmlTradeItemName = String.Format("{0}.Trade.{1}.{2}", sourceSystem, productType, trade.id);
                //gwmlDocItemName = String.Format("{0}.GWML.{1}.{2}", sourceSystem, productType, trade.id);
                tradeProps.Set(TradeProp.TradeDate, tradeDate);
                tradeProps.Set(TradeProp.TradeId, trade.id);
                tradeProps.Set(TradeProp.ProductType, ProductTypeSimpleScheme.GetEnumString(productType));
                tradeProps.Set(TradeProp.TradeState, TradeState.Verified.ToString());
                client.SaveObject(trade, fpmlTradeItemName, tradeProps, false, TimeSpan.FromDays(30));
            } // foreach file
            logger.LogDebug("Loaded {0} FPML trades.", chosenFiles.Count());
        }

        public static void Load(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            LoadFiles<FpML.V5r3.Reporting.Market>(logger, client, assembly, "Orion.Market");
            //LoadFiles<Trade>(logger, client, assembly, "Calypso.Trade");
            //LoadFiles<Trade>(logger, client, assembly, "Murex.Trade");

            LoadFPMLTrades(logger, client, assembly, "FpMLTrades");
            //LoadGWMLTrades(logger, client, assembly, "Murex");

        }

    }
}
