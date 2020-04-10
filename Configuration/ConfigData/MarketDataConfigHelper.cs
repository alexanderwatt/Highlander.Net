using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

namespace Orion.Configuration
{
    public static class MarketDataConfigHelper
    {
        public static void LoadProviderRules(ILogger logger, ICoreCache targetClient)
        {
            logger.LogDebug("Loading market data provider rules...");
            const string itemType = "MarketData.ProviderRules";
            int count = 0;
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.ProviderRuleSets";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing  market data provider rules!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var ruleSet = XmlSerializerHelper.DeserializeFromString<ProviderRuleSet>(file.Value);
                MDSProviderId providerId = ruleSet.provider;
                logger.LogDebug("  Loading {0} ...", providerId);
                var itemProps = new NamedValueSet();
                itemProps.Set(CurveProp.DataGroup, "Orion.Configuration." + itemType);
                itemProps.Set("SourceSystem", "Orion");
                itemProps.Set("Function", "Configuration");
                itemProps.Set("Type", itemType);
                string itemName = String.Format("Orion.Configuration.{0}.{1}", itemType, providerId);
                targetClient.SaveObject(ruleSet, itemName, itemProps, false, TimeSpan.MaxValue);
                count++;
            }
            logger.LogDebug("Loaded {0} market data provider rule sets", count);
        }

        //public static void LoadProviderRules(ILogger logger, ICoreCache targetClient)
        //{
        //    logger.LogDebug("Loading market data provider rules...");
        //    const string itemType = "MarketData.ProviderRules";
        //    int count = 0;
        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    const string Prefix = "Orion.Configuration.ProviderRuleSets";
        //    Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, Prefix, "xml");
        //    if (chosenFiles.Count == 0)
        //        throw new InvalidOperationException("Missing  market data provider rules!");

        //    foreach (KeyValuePair<string, string> file in chosenFiles)
        //    {
        //        ProviderRuleSet ruleSet = XmlSerializerHelper.DeserializeFromString<ProviderRuleSet>(file.Value);
        //        MDSProviderId providerId = ruleSet.provider;
        //        logger.LogDebug("  Loading {0} ...", providerId);
        //        NamedValueSet itemProps = new NamedValueSet();
        //        itemProps.Set(CurveProp.DataGroup, "Orion.Configuration." + itemType);
        //        itemProps.Set("SourceSystem", "Orion");
        //        itemProps.Set("Function", "Configuration");
        //        itemProps.Set("Type", itemType);
        //        string itemName = String.Format("Orion.Configuration.{0}.{1}", itemType, providerId);
        //        targetClient.SaveObject<ProviderRuleSet>(ruleSet, itemName, itemProps, false, TimeSpan.MaxValue);
        //        count++;
        //    }
        //    logger.LogDebug("Loaded {0} market data provider rule sets", count);
        //}
    }
}
