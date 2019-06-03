using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Common;
using FpML.V5r10.Reporting;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

namespace FpML.V5r10.ConfigData
{
    public static class MarketLoader
    {
        private const string ResourcePath = "FpML.V5r10.ConfigData";

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
                // strip assempbly prefix
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
            const string prefix = "FpML.V5r10.ConfigData.Config.Trade";
            logger.LogDebug("Loading {0} files...", typeof(Trade).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(Trade).Name} files found!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Document>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
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
            const string prefix = "FpML.V5r10.ConfigData.Config.FixedIncome";
            logger.LogDebug("Loading {0} files...", typeof(Bond).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(Bond).Name} files found!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Bond>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
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
            const string prefix = "FpML.V5r10.ConfigData.Config.QuotedAssetSet";
            logger.LogDebug("Loading {0} files...", typeof(Bond).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(QuotedAssetSet).Name} files found!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
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
