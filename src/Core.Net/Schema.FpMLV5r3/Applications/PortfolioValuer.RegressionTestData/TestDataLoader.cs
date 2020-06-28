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

using System;
using System.Collections.Generic;
using System.Reflection;
using Highlander.Codes.V5r3;
using Highlander.Confirmation.V5r3;
using Highlander.Core.Common;
using Highlander.Metadata.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;

namespace Highlander.PVRegression.TestData.V5r3
{
    public static class RegressionTestDataLoader
    {
        private const string ResourcePath = "Highlander.PVRegression.TestData.V5r3";
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
                throw new InvalidOperationException($"No {typeof(T).Name} files found!");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<T>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assembly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(T).Name);
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
            //EnumMaps enumMaps = client.LoadObject<EnumMaps>("Highlander.Configuration.Data.V5r3.GwmlEnumMaps");
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
                string fpmlTradeItemName = $"{sourceSystem}.Trade.{productType}.{trade.id}";
                //gwmlDocItemName = String.Format("{0}.GWML.{1}.{2}", sourceSystem, productType, trade.id);
                tradeProps.Set(TradeProp.TradeDate, tradeDate);
                tradeProps.Set(TradeProp.TradeId, trade.id);
                tradeProps.Set(TradeProp.ProductType, ProductTypeSimpleScheme.GetEnumString(productType));
                tradeProps.Set(TradeProp.TradeState, TradeState.Verified.ToString());
                client.SaveObject(trade, fpmlTradeItemName, tradeProps, false, TimeSpan.FromDays(30));
            } // foreach file
            logger.LogDebug("Loaded {0} FPML trades.", chosenFiles.Count);
        }

        public static void Load(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            //LoadFiles<Reporting.V5r3.Market>(logger, client, assembly, "Highlander.Market"); // These are 4.7
            //LoadFiles<Trade>(logger, client, assembly, "Calypso.Trade");
            //LoadFiles<Trade>(logger, client, assembly, "Murex.Trade");
            LoadFPMLTrades(logger, client, assembly, "FpMLTrades");
            //LoadGWMLTrades(logger, client, assembly, "Murex");
        }
    }
}
