﻿/*
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
using Highlander.Metadata.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;

namespace Highlander.Configuration.Data.V5r3
{
    public static class MarketDataConfigHelper
    {
        public static void LoadProviderRules(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading market data provider rules...");
            const string itemType = "MarketData.ProviderRules";
            int count = 0;
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Highlander.Configuration.Data.V5r3.ProviderRuleSets";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException("Missing  market data provider rules!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var ruleSet = XmlSerializerHelper.DeserializeFromString<ProviderRuleSet>(file.Value);
                MDSProviderId providerId = ruleSet.provider;
                logger.LogDebug("  Loading {0} ...", providerId);
                var itemProps = new NamedValueSet();
                itemProps.Set(EnvironmentProp.DataGroup, nameSpace + ".Configuration." + itemType);
                itemProps.Set(EnvironmentProp.SourceSystem, "Highlander");
                itemProps.Set(EnvironmentProp.Function, FunctionProp.Configuration.ToString());
                itemProps.Set(EnvironmentProp.Type, itemType);
                itemProps.Set(EnvironmentProp.Schema, "V5r3.Reporting");
                itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
                string itemName = string.Format(nameSpace + ".Configuration.{0}.{1}", itemType, providerId);
                targetClient.SaveObject(ruleSet, itemName, itemProps, false, TimeSpan.MaxValue);
                count++;
            }
            logger.LogDebug("Loaded {0} market data provider rule sets", count);
        }
    }
}
