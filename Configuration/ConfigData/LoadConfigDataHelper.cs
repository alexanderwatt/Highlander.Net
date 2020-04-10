#region Usings

using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

#endregion

namespace Orion.Configuration
{
    public static class LoadConfigDataHelper
    {
        public static Pair<string, NamedValueSet> StandardConfigProps(NamedValueSet initProps, string type, string idSuffix)
        {
            var itemProps = new NamedValueSet(initProps);
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration." + type);
            itemProps.Set("SourceSystem", "Orion");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", type);
            string itemName = "Orion.Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            return new Pair<string, NamedValueSet>(itemName, itemProps);
        }

        public static void LoadConfigurationData(ILogger logger, ICoreCache targetClient)
        {
            // load config data in external assemblies
            MarketDataConfigHelper.LoadProviderRules(logger, targetClient);
            PricingStructureLoader.LoadPricingStructures(logger, targetClient);
            StressDefinitionLoader.LoadStressDefinitions(logger, targetClient);
            StressDefinitionLoader.LoadScenarioDefinitions(logger, targetClient);

            //ServiceHostRulesLoader.Load(logger, targetClient);
            FileImportRuleLoader.Load(logger, targetClient);
            TradeImportRuleLoader.Load(logger, targetClient);
            AlertRulesLoader.Load(logger, targetClient);
            ConfigDataLoader.LoadInstrumentsConfig(logger, targetClient);
            ConfigDataLoader.LoadDateRules(logger, targetClient);
            //ConfigDataLoader.LoadHolidayDates(logger, targetClient);
            ConfigDataLoader.LoadNewHolidayDates(logger, targetClient);
            ConfigDataLoader.LoadBoundaryRider(logger, targetClient);

            // removed - incompatible with older clients
            ConfigDataLoader.LoadPricingStructureAlgorithm(logger, targetClient);
            ConfigDataLoader.LoadFpml(logger, targetClient);
            //ConfigDataLoader.LoadGwml(logger, targetClient);
            MarketLoader.Load(logger, targetClient);
            MarketLoader.LoadFixedIncomeData(logger, targetClient);
            MarketLoader.LoadTradeData(logger, targetClient);
            FpMLTradeLoader.LoadTrades(logger, targetClient);   //PROBLEM         
        }
    }
}
