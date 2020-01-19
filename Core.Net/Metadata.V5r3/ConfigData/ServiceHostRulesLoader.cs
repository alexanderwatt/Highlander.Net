/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using Highlander.Core.Common;
using Highlander.Utilities.Logging;
using HostConfigRule = Highlander.Metadata.Common.HostConfigRule;

namespace Highlander.Configuration.Data.V5r3
{
    public static class ServiceHostRulesLoader
    {
        private static HostConfigRule MakeRuleGenericServer(string buildCfg, string fileName, string className,
            int? totalInstanceCount = null, int? localInstanceCount = null, int? localInstanceStart = null)
        {
            var assemblyPaths = new List<string>
                {
                    $@"..\..\..\..\..\Services\{fileName}\bin\{buildCfg}"
                };
            return new HostConfigRule
                {
                buildConfig = buildCfg,
                serverApplName = className,
                serverImplType = "Highlander.Server." + fileName + "." + className,
                serverAssmName = "Highlander.Server." + fileName,
                serverAssmPath = assemblyPaths.ToArray(),
                serverEnabled = true,
                serverInstanceTotalCount = totalInstanceCount,
                serverInstanceLocalCount = localInstanceCount,
                serverInstanceLocalStart = localInstanceStart
            };
        }

        private static HostConfigRule MakeRulePortfolioValuerServer(string buildCfg)
        {
            var assemblyPaths = new List<string>
                {
                    $@"..\..\..\..\..\Silverlight\Highlander.PortfolioValuer\Highlander.PortfolioValuer.Services\bin\{buildCfg}"
                };
            return new HostConfigRule
                {
                buildConfig = buildCfg,
                serverApplName = "PortfolioValuerServer",
                serverImplType = "Highlander.PortfolioValuer.Services.PortfolioValuerServer",
                serverAssmName = "Highlander.PortfolioValuer.Services",
                serverAssmPath = assemblyPaths.ToArray(),
                serverEnabled = true
            };
        }

        private static void AddRuleSetUnitTest(string buildCfg, List<HostConfigRule> rules)
        {
            string envName = EnvHelper.EnvName(EnvId.Utt_UnitTest);
            // default host instance
            rules.Add(new HostConfigRule
                {
                buildConfig = buildCfg,
                hostEnvName = envName,
                Priority = 1,
                serverApplName = "DataExportServer",
                serverImplType = "Highlander.Server.DataExporter.DataExportServer",
                serverAssmName = "Highlander.Server.DataExporter",
                serverAssmPath = new[] {$@"..\..\..\..\..\Services\TradeExport\TradeExporter\bin\{buildCfg}"},
                serverEnabled = true
            });
            // start a 2-node trade reval grid
            // - host instance A
            rules.Add(new HostConfigRule
                {
                buildConfig = buildCfg,
                hostEnvName = envName,
                hostInstance = "A",
                Priority = 1,
                serverApplName = "TradeValuationServer",
                serverImplType = "Highlander.Server.TradeRevaluer.TradeValuationServer",
                serverAssmName = "Highlander.Server.TradeRevaluer",
                serverAssmPath = new[] {$@"..\..\..\..\..\Services\TradeRevaluer\bin\{buildCfg}"},
                serverEnabled = true,
                serverInstanceTotalCount = 5,
                serverInstanceLocalCount = 1,
                serverInstanceLocalStart = 0
            });
            // - host instance B
            rules.Add(new HostConfigRule
                {
                buildConfig = buildCfg,
                hostEnvName = envName,
                hostInstance = "B",
                Priority = 1,
                serverApplName = "TradeValuationServer",
                serverImplType = "Highlander.Server.TradeRevaluer.TradeValuationServer",
                serverAssmName = "Highlander.Server.TradeRevaluer",
                serverAssmPath = new[] {$@"..\..\..\..\..\Services\TradeRevaluer\bin\{buildCfg}"},
                serverEnabled = true,
                serverInstanceTotalCount = 5,
                serverInstanceLocalCount = 4,
                serverInstanceLocalStart = 1
            });
        }

        private static HostConfigRule SetRuleEnv(EnvId env, string hostComputer, string hostInstance, string buildCfg, bool enabled, HostConfigRule rule)
        {
            rule.hostEnvName = EnvHelper.EnvName(env);
            rule.hostComputer = hostComputer;
            rule.hostInstance = hostInstance;
            rule.buildConfig = buildCfg;
            rule.Priority = 1;
            var assemblyPaths = new List<string>(rule.serverAssmPath)
                {
                    $@"C:\_highlander\Builds\{EnvHelper.EnvName(env)}\Current\{buildCfg}"
                };
            rule.serverAssmPath = assemblyPaths.ToArray();
            rule.serverEnabled = enabled;
            return rule;
        }

        private static void AddRuleSet(EnvId env, string hostComputer, string buildConfig, List<HostConfigRule> rules)
        {
            // note: The market data server (MDS) must run in the default host instance (null) as only this instance
            //       has the necessary configuration, Bloomberg meta data files, and references to Bloomberg assemblies.
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "MarketData", "MarketDataServer")));

            // the following services have no known configuration constraints - run them in the default host instance
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "FileImporter", "FileImportServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "TradeImporter", "TradeImportServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "Alerting", "AlertServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRulePortfolioValuerServer(buildConfig)));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "DataExporter", "DataExportServer")));

            // note: Services that use the Highlander analytics engine must run in separate processes (different host instances)
            //       because the engine contains a static singleton runtime client which cannot easily be shared. These services are:
            //       - CurveGenServer
            //       - StressGenServer
            //       - TradeValuationServer
            //       - PortfolioValuationServer
            rules.Add(SetRuleEnv(env, hostComputer, "A", buildConfig, true, MakeRuleGenericServer(buildConfig, "CurveGenerator", "CurveGenServer")));
            rules.Add(SetRuleEnv(env, hostComputer, "B", buildConfig, true, MakeRuleGenericServer(buildConfig, "StressGenerator", "StressGenServer")));
            rules.Add(SetRuleEnv(env, hostComputer, "C", buildConfig, true, MakeRuleGenericServer(buildConfig, "TradeRevaluer", "TradeValuationServer")));
            rules.Add(SetRuleEnv(env, hostComputer, "D", buildConfig, false, MakeRuleGenericServer(buildConfig, "TradeRevaluer", "PortfolioValuationServer")));
        }

        private static void AddRuleSetSydwCndb12S(string buildConfig, List<HostConfigRule> rules)
        {
            const EnvId env = EnvId.Dev_Development;
            const string hostComputer = "sydwcndb12s";
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "FileImporter", "FileImportServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "TradeImporter", "TradeImportServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "TradeRevaluer", "TradeValuationServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "TradeRevaluer", "PortfolioValuationServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "DataExporter", "DataExportServer")));
        }

        private static void AddRuleSetSydw7Jxjz1S(string buildConfig, List<HostConfigRule> rules)
        {
            const EnvId env = EnvId.Dev_Development;
            const string hostComputer = "sydw7jxjz1s";
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, false, MakeRuleGenericServer(buildConfig, "DataExporter", "DataExportServer")));
            rules.Add(SetRuleEnv(env, hostComputer, null, buildConfig, true, MakeRuleGenericServer(buildConfig, "TradeRevaluer", "PortfolioValuationServer")));
        }

        public static void Load(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            //logger.LogDebug("Deleting service host rules...");
            //targetClient.Proxy.DeleteObjects<HostConfigRule>(Expr.ALL);
            logger.LogDebug("Loading service host rules...");
            // generate rules
            var rules = new List<HostConfigRule>();
            AddRuleSet(EnvId.Dev_Development, "sydwadqds01", "Release", rules);
            AddRuleSet(EnvId.Sit_SystemTest, "sydwadqds01", "Release", rules);
            AddRuleSet(EnvId.Stg_StagingLive, "sydwadbrl01", "Release", rules);
            //AddRuleSet(EnvId.PRD_Production, "unknown", "Release", rules);
            // unit test rules
            AddRuleSetUnitTest("Debug", rules);
            AddRuleSetUnitTest("Release", rules);
            // specific developer rules
            AddRuleSetSydwCndb12S("Debug", rules);
            AddRuleSetSydwCndb12S("Release", rules);
            AddRuleSetSydw7Jxjz1S("Debug", rules);
            AddRuleSetSydw7Jxjz1S("Release", rules);
            // save rules
            foreach (HostConfigRule rule in rules)
            {
                rule.NameSpace = nameSpace;
                TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(rule, lifetime);
            }
            logger.LogDebug("Loaded service host rules.");
        }
    }
}
