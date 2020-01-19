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

#region Usings

using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Configuration.Data.V5r3
{
    public static class LoadConfigDataHelper
    {
        public static Pair<string, NamedValueSet> StandardConfigProps(NamedValueSet initProps, string type, string idSuffix, string nameSpace)
        {
            var itemProps = new NamedValueSet(initProps);
            itemProps.Set(EnvironmentProp.DataGroup, nameSpace + ".Configuration.V5r3." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Highlander");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.Configuration.ToString());
            itemProps.Set(EnvironmentProp.Type, type);
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Reporting");
            itemProps.Set(EnvironmentProp.NameSpace, nameSpace);
            var identifier = "Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            string itemName = nameSpace + "." + identifier;
            itemProps.Set(CurveProp.UniqueIdentifier, identifier);
            return new Pair<string, NamedValueSet>(itemName, itemProps);
        }

        public static void LoadConfigurationData(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            //Load new format config
            MarketDataConfigHelper.LoadProviderRules(logger, targetClient, nameSpace);
            ServiceHostRulesLoader.Load(logger, targetClient, nameSpace);
            FileImportRuleLoader.Load(logger, targetClient, nameSpace);
            TradeImportRuleLoader.Load(logger, targetClient, nameSpace);
            AlertRulesLoader.Load(logger, targetClient, nameSpace);
            //ConfigDataLoader.LoadBoundaryRider(logger, targetClient, nameSpace);
            PricingStructureLoader.LoadPricingStructures(logger, targetClient, nameSpace);
            StressDefinitionLoader.LoadStressDefinitions(logger, targetClient, nameSpace);
            StressDefinitionLoader.LoadScenarioDefinitions(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadInstrumentsConfig(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadDateRules(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadNewHolidayDates(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadPricingStructureAlgorithm(logger, targetClient, nameSpace);
            MarketLoader.Load(logger, targetClient, nameSpace);
            MarketLoader.LoadFixedIncomeData(logger, targetClient, nameSpace);
            MarketLoader.LoadQasData(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadFpMLCodes(logger, targetClient, nameSpace);
            //TODO The serializer crashes!
            FpMLTradeLoader.LoadTrades1(logger, targetClient, nameSpace);
            //FpMLTradeLoader.LoadTrades2(logger, targetClient, nameSpace); 
        }
    }
}
