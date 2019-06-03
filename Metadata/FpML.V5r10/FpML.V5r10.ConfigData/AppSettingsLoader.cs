using System.Collections.Generic;
using System.Reflection;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

namespace FpML.V5r10.ConfigData
{
    public static class AppSettingsLoader
    {
        public static void Load(ILogger logger, ICoreClient targetClient, string nameSpace)
        {
            logger.LogDebug("Loading App Settings...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            {
                var prefix = nameSpace + ".Configuration.AppSettings";
                Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "txt");
                int loadCount = 0;
                foreach (KeyValuePair<string, string> file in chosenFiles)
                {
                    string[] nameParts = file.Key.Split('.');
                    var properties = new NamedValueSet(file.Value);
                    string name = nameParts[5];
                    EnvId env = EnvHelper.ParseEnvName(nameParts[4]);
                    if (env == targetClient.ClientInfo.ConfigEnv)
                    {
                        targetClient.SaveAppSettings(properties, name, null, null, true, env);
                        loadCount++;
                    }
                }
                logger.LogDebug("Loaded {0} App Settings.", loadCount);
            }

            // load data exporter sql configs
            {
                // DEV default
                targetClient.SaveAppSettings(
                    GetPVSqlConnectionProps("sydwadqds01", null, "PortfolioValuer_DEV"),
                    "DataExportServer", null, null, true, EnvId.Dev_Development);
                // SIT default
                targetClient.SaveAppSettings(
                    GetPVSqlConnectionProps("sydwddqur02", null, "PortfolioValuer_SIT"),
                    "DataExportServer", null, null, true, EnvId.Sit_SystemTest);
                // STG default
                targetClient.SaveAppSettings(
                    GetPVSqlConnectionProps("sydwadbrl01", null, "PortfolioValuer_STG"),
                    "DataExportServer", null, null, true, EnvId.Stg_StagingLive);

                // -------------------- additional development machines --------------------
                // sydwcndb12s - Simon's machine
                targetClient.SaveAppSettings(
                    GetPVSqlConnectionProps("sydwadqds01", null, "PortfolioValuer_DEV"),
                    "DataExportServer", null, "sydwcndb12s", true, EnvId.Dev_Development);
                // sydw7jxjz1s - Mario's machine
                targetClient.SaveAppSettings(
                    GetPVSqlConnectionProps("sydwadqds01", null, "PortfolioValuer_DEV"),
                    "DataExportServer", null, "sydw7jxjz1s", true, EnvId.Dev_Development);
            }
        }
        private static NamedValueSet GetPVSqlConnectionProps(string dbComputer, string dbInstance, string dbDatabase)
        {
            string dataSource = dbComputer;
            if (dbInstance != null)
                dataSource += ("\\" + dbInstance);
             return new NamedValueSet(new NamedValue("SqlConnectionString",
                 $"Data Source={dataSource};Initial Catalog={dbDatabase};Integrated Security=True;MultipleActiveResultSets=True"));
        }
    }
}
