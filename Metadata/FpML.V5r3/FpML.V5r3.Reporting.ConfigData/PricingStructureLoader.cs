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
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;

namespace Orion.V5r3.Configuration
{
    public static class PricingStructureLoader
    {
        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo MakePricingStructureConfigProps(string type, string idSuffix, NamedValueSet extraProps, string nameSpace)
        {
            var itemProps = new NamedValueSet(extraProps);
            itemProps.Set(EnvironmentProp.DataGroup, "Orion.V5r3.Reporting.Configuration." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Orion");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.Configuration);
            itemProps.Set(EnvironmentProp.Type, type);
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Reporting");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            var identifier = "Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            string itemName = nameSpace + "." + identifier;
            itemProps.Set(CurveProp.UniqueIdentifier, identifier);
            var marketEnv = itemProps.GetValue<string>(CurveProp.Market, true);
            itemProps.Set(CurveProp.MarketDate, null);
            itemProps.Set(CurveProp.MarketAndDate, marketEnv);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static void LoadPricingStructures(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading pricing structures...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.V5r3.Configuration.PricingStructures";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing Pricing Structures");
            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var market = XmlSerializerHelper.DeserializeFromString<Market>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var extraProps = new NamedValueSet(nvs);
                string idSuffix =
                    $"{extraProps.GetValue<string>(CurveProp.Market)}.{extraProps.GetValue<string>(CurveProp.PricingStructureType)}.{extraProps.GetValue<string>(CurveProp.CurveName)}";
                ItemInfo itemInfo = MakePricingStructureConfigProps("PricingStructures", idSuffix, extraProps, nameSpace);
                logger.LogDebug("  {0} ...", idSuffix);
                targetClient.SaveObject(market, itemInfo.ItemName, itemInfo.ItemProps, false, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} pricing structures.", chosenFiles.Count);
        }
    }
}
