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
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;

namespace Highlander.Configuration.Data.V5r3
{
    public static class MarketLoader
    {
        private const string ResourcePath = "Highlander.Configuration.Data.V5r3";

        private static void LoadFiles<T>(
            ILogger logger, 
            ICoreCache client,
            Assembly assembly,
            string filenamePrefix, 
            string nameSpace) where T : class
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
                itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
                itemProps.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
                itemName = nameSpace + "." + itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(T).Name);
        }

        public static void Load(ILogger logger, ICoreCache client, string nameSpace)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            LoadFiles<Market>(logger, client, assembly, "Market", nameSpace);
        }

        public static void LoadTradeData(ILogger logger, ICoreCache client, string nameSpace)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Highlander.Configuration.Data.V5r3.Config.Trade";
            logger.LogDebug("Loading {0} files...", typeof(Trade).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(Trade).Name} files found!");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Document>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assembly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
                itemProps.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                itemName = nameSpace + itemName.Substring(19, itemName.Length - 23);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(Bond).Name);
        }

        public static void LoadFixedIncomeData(ILogger logger, ICoreCache client, string nameSpace)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Highlander.Configuration.Data.V5r3.Config.FixedIncome";
            logger.LogDebug("Loading {0} files...", typeof(Bond).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(Bond).Name} files found!");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Bond>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assembly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
                itemProps.Set(EnvironmentProp.Function, FunctionProp.ReferenceData.ToString());
                var identifier = "ReferenceData." + itemName.Substring(19, itemName.Length - 23).Replace("-","/");
                itemName = nameSpace + "." + identifier;
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(Bond).Name);
        }

        public static void LoadQasData(ILogger logger, ICoreCache client, string nameSpace)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Highlander.Configuration.Data.V5r3.Config.QuotedAssetSet";
            logger.LogDebug("Loading {0} files...", typeof(Bond).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(QuotedAssetSet).Name} files found!");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assembly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
                itemProps.Set(EnvironmentProp.Function, FunctionProp.QuotedAssetSet.ToString());
                var identifier = itemName.Substring(22, itemName.Length - 26);
                itemName = nameSpace + "." + identifier;
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(QuotedAssetSet).Name);
        }
    }
}
