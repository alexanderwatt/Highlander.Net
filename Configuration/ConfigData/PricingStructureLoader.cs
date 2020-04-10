using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using nab.QDS.FpML.V47;

namespace Orion.Configuration
{
    public static class PricingStructureLoader
    {
        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo MakePricingStructureConfigProps(string type, string idSuffix, NamedValueSet extraProps)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration." + type);
            itemProps.Set("SourceSystem", "Orion");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", type);
            string itemName = "Orion.Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            var marketEnv = itemProps.GetValue<string>(CurveProp.Market, true);
            itemProps.Set(CurveProp.MarketDate, null);
            itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static void LoadPricingStructures(ILogger logger, ICoreCache targetClient)
        {
            logger.LogDebug("Loading pricing structures...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string Prefix = "Orion.Configuration.PricingStructures";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, Prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing Pricing Structures");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var market = XmlSerializerHelper.DeserializeFromString<Market>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var extraProps = new NamedValueSet(nvs);
                string idSuffix = String.Format("{0}.{1}.{2}",
                                                extraProps.GetValue<string>(CurveProp.Market),
                                                extraProps.GetValue<string>(CurveProp.PricingStructureType),
                                                extraProps.GetValue<string>(CurveProp.CurveName));
                ItemInfo itemInfo = MakePricingStructureConfigProps("PricingStructures", idSuffix, extraProps);
                logger.LogDebug("  {0} ...", idSuffix);
                targetClient.SaveObject(market, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} pricing structures.", chosenFiles.Count());
        }
    }
}
