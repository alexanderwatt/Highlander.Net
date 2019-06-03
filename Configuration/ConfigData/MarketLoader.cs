using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using nab.QDS.FpML.V47;
//using nab.QDS.GWML.V65;
//using nab.QDS.GWML.V65.Adapter;

namespace Orion.Configuration
{
    public static class MarketLoader
    {
        private const string ResourcePath = "Orion.Configuration";

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
                itemName = "Orion." + itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count(), typeof(T).Name);
        }

        public static void Load(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            LoadFiles<Market>(logger, client, assembly, "Market");
        }

        public static void LoadTradeData(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.Trade";
            logger.LogDebug("Loading {0} files...", typeof(Trade).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException(String.Format("No {0} files found!", typeof(Trade).Name));

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Trade>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = "Orion." + itemName.Substring(19, itemName.Length - 23);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count(), typeof(Bond).Name);
        }

        public static void LoadFixedIncomeData(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.FixedIncome";
            logger.LogDebug("Loading {0} files...", typeof(Bond).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException(String.Format("No {0} files found!", typeof(Bond).Name));

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<Bond>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = "Orion.ReferenceData." + itemName.Substring(19, itemName.Length - 23);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count(), typeof(Bond).Name);
        }
    }
}
