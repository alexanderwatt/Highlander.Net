#region Usings

using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

#endregion

namespace Orion.V5r3.Configuration
{
    public static class LoadConfigDataHelper
    {
        public static Pair<string, NamedValueSet> StandardConfigProps(NamedValueSet initProps, string type, string idSuffix, string nameSpace)
        {
            var itemProps = new NamedValueSet(initProps);
            itemProps.Set(EnvironmentProp.DataGroup, "Orion.V5r3.Reporting.Configuration." + type);
            itemProps.Set(EnvironmentProp.SourceSystem, "Orion");
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
            ConfigDataLoader.LoadBoundaryRider(logger, targetClient, nameSpace);
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
