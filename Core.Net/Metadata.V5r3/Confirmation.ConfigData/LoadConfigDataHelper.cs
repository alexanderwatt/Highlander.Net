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
            itemProps.Set("Schema", "V5r3");
            string itemName = "Orion.FpML.V5r3.Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            return new Pair<string, NamedValueSet>(itemName, itemProps);
        }

        public static void LoadConfigurationData(ILogger logger, ICoreCache targetClient)
        {
            // load config data in external assemblies
            FpMLTradeLoader.LoadTrades1(logger, targetClient);
            FpMLTradeLoader.LoadTrades2(logger, targetClient);   //Trades only as all calculations are done in Reporting namespace        
        }
    }
}
