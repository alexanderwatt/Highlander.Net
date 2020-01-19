using System;
using System.Collections.Generic;
using System.Threading;
using Core.Alert.Server;
using Core.Common;
using Orion.MDAS.Server;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Servers;
using Orion.Util.Threading;
using Orion.V5r3.Configuration;
using Server.CurveGenerator;
using Server.StressGenerator;
using Server.TradeRevaluer;

namespace Orion.Server.ServiceHost
{
    internal class ServerFarmNode
    {
        public readonly string Key;
        public HostConfigRule Rule { get; set; }
        public ILogger Logger;
        public IBasicServer Server;
        public ServerFarmNode(string key) { Key = key; }
    }

    public class ServiceHost : ServerBase2
    {
        private readonly ILogger _logger;
        private readonly GuardedDictionary<string, ServerFarmNode> _serverFarmDict = new GuardedDictionary<string, ServerFarmNode>();
        private CoreCache _hostConfigRuleSubs;
        private int _ruleUpdatesQueued;

        public ServiceHost(ILogger logger)
        {
            _logger = new FilterLogger(logger, "ServiceHost: ");
        }

        protected override void OnFirstCallback()
        {
            _hostConfigRuleSubs = Client.CreateCache(
                delegate(CacheChangeData update)
                {
                    Interlocked.Increment(ref _ruleUpdatesQueued);
                    _MainThreadQueue.Dispatch<object>(null, OnRuleUpdate);
                }, null);
            _hostConfigRuleSubs.SubscribeWait<HostConfigRule>(RuleHelper.MakeRuleFilter(
                    EnvHelper.EnvName(Client.ClientInfo.ConfigEnv),
                    Client.ClientInfo.HostName,
                    ServerInstance,
                    Client.ClientInfo.UserName));

            Interlocked.Increment(ref _ruleUpdatesQueued);
            _MainThreadQueue.Dispatch<object>(null, OnRuleUpdate);
        }

        protected override void OnServerStopping()
        {
            // cleanup
            DisposeHelper.SafeDispose(ref _hostConfigRuleSubs);
            // stop all services
            _logger.LogInfo("Stopping all servers...");
            foreach (ServerFarmNode node in _serverFarmDict.GetValues())
            {
                try
                {
                    if (node.Server != null)
                    {
                        _logger.LogInfo("Server: '{0}' stopping...", node.Key);
                        node.Server.Stop();
                        DisposeHelper.SafeDispose(ref node.Server);
                        DisposeHelper.SafeDispose(ref node.Logger);
                        _logger.LogInfo("Server: '{0}' stopped.", node.Key);
                    }
                    // publish result
                    PublishHostConfigResult(node, null);
                }
                catch (Exception e)
                {
                    _logger.Log(e);
                    // publish result
                    PublishHostConfigResult(node, "Exception: " + e);
                }
            }
            _logger.LogInfo("All servers stopped.");
        }

        private void PublishHostConfigResult(ServerFarmNode node, string comment)
        {
            var result = new HostConfigResult
                {
                    hostEnvName = EnvHelper.EnvName(Client.ClientInfo.ConfigEnv),
                    hostComputer = Client.ClientInfo.HostName,
                    hostUserName = Client.ClientInfo.UserName
                };
            // create config Result
            if (node != null)
            {
                result.serverApplName = node.Key;
                if (node.Rule != null)
                {
                    result.serverImplType = node.Rule.serverImplType;
                }
            }
            result.serverEnabled = false;
            result.serverComment = "Stopped";
            if (node != null && node.Server != null)
            {
                result.serverEnabled = true;
                result.serverComment = "Starting";
                if (node.Server.HasStarted)
                    result.serverComment = "Running";
                if (node.Server.HasStopped)
                    result.serverComment = "Stopped";
            }
            if (comment != null)
                result.serverComment = comment;
            // publish Result
            Client.SaveObject<HostConfigResult>(result, true, TimeSpan.FromDays(30));
        }
        private void OnRuleUpdate(object notUsed)
        {
            try
            {
                // process rules and start/stop servers as required
                int updatesQueued = Interlocked.Decrement(ref _ruleUpdatesQueued);
                if (updatesQueued > 0)
                    return;

                // subscription is ready and callback flood has stopped
                List<ICoreItem> ruleItems = _hostConfigRuleSubs.Items;
                _logger.LogDebug("Processing {0} host configuration rules", ruleItems.Count);
                // delete old rules
                foreach (ServerFarmNode node in _serverFarmDict.GetValues())
                    node.Rule = null;
                // process rules by priority
                foreach (ICoreItem item in ruleItems)
                {
                    try
                    {
                        if (item.Expires >= DateTimeOffset.Now)
                        {
                            var newRule = (HostConfigRule)item.Data;
                            // ensure server node exists
                            ServerFarmNode node = _serverFarmDict.GetOrSet(
                                newRule.serverApplName, () => new ServerFarmNode(newRule.serverApplName));
                            int oldPriority = Int32.MinValue;
                            HostConfigRule oldRule = node.Rule;
                            if (oldRule != null)
                                oldPriority = oldRule.Priority;
                            if ((newRule.Priority >= oldPriority) && (!newRule.Disabled))
                            {
                                node.Rule = newRule;
                            }
                        }
                    }
                    catch (Exception excp)
                    {
                        _logger.Log(excp);
                    }
                }

                // now start/stop servers to match rules
                foreach (ServerFarmNode node in _serverFarmDict.GetValues())
                {
                    try
                    {
                        if (node.Server == null)
                        {
                            if (node.Rule != null)
                            {
                                // create and start?
                                if (node.Rule.serverEnabled)
                                {
                                    // server rule enabled but server does not exist
                                    _logger.LogInfo("Server: '{0}' starting...", node.Key);
                                    _logger.LogDebug("    Rule ItemName  : {0}", node.Rule.PrivateKey);
                                    _logger.LogDebug("    Rule Priority  : {0}", node.Rule.Priority);
                                    _logger.LogDebug("    Server ApplName: {0}", node.Rule.serverApplName);
                                    _logger.LogDebug("    Server ImplType: {0}", node.Rule.serverImplType);
                                    _logger.LogDebug("    Server Enabled?: {0}", node.Rule.serverEnabled);
                                    // todo - load dynamically
                                    string serverName = node.Rule.serverImplType;
                                    ILogger logger = new FileLogger(@"C:\_qrsc\ServiceLogs\" + serverName + ".{dddd}.log");
                                    // alert monitor
                                    if (serverName == typeof(AlertServer).Name)
                                    {
                                        node.Server = new AlertServer(logger, EnvId.Undefined);
                                    }
                                    //// file importer
                                    //else if (serverName == typeof(FileImportServer).Name)
                                    //{
                                    //    node.Server = new FileImportServer(logger, EnvId.Undefined);
                                    //}
                                    // market data server
                                    else if (serverName == typeof(MarketDataServer).Name)
                                    {
                                        node.Server = new MarketDataServer(logger, EnvId.Undefined);
                                    }
                                    //// trade importer
                                    //else if (serverName == typeof(TradeImportServer).Name)
                                    //{
                                    //    node.Server = new TradeImportServer(logger, EnvId.Undefined);
                                    //}
                                    //// curve importer
                                    //else if (serverName == typeof(CurveImportServer).Name)
                                    //{
                                    //    node.Server = new CurveImportServer(logger, EnvId.Undefined);
                                    //}
                                    // base curve generator - todo - cant be added until ServerStore is deprecated from HL Engine
                                    else if (serverName == typeof(CurveGenServer).Name)
                                    {
                                        node.Server = new CurveGenServer(logger, this.Client);
                                    }
                                    // stressed curve generator
                                    else if (serverName == typeof(StressGenServer).Name)
                                    {
                                        node.Server = new StressGenServer(logger, this.Client);
                                    }
                                    // trade valuation server
                                    else if (serverName == typeof(TradeValuationServer).Name)
                                    {
                                        node.Server = new TradeValuationServer(logger, this.Client);
                                    }
                                    // valuation aggregator
                                    //else if (serverName == typeof(ValueAggServer).Name)
                                    //{
                                    //    node.Server = new ValueAggServer(logger, EnvId.Undefined);
                                    //}
                                    else
                                    {
                                        DisposeHelper.SafeDispose(ref logger);
                                        throw new ApplicationException(
                                            "Unknown serverImplType: '" + serverName + "'");
                                    }
                                    node.Logger = logger;
                                    node.Server.Start();
                                    _logger.LogInfo("Server: '{0}' started.", node.Key);
                                }
                            }
                        }
                        else
                        {
                            // still running
                            // stop and destroy?
                            if ((node.Rule == null) || (!node.Rule.serverEnabled))
                            {
                                if (node.Rule != null)
                                {
                                    _logger.LogDebug("    Rule ItemName  : {0}", node.Rule.PrivateKey);
                                    _logger.LogDebug("    Rule Priority  : {0}", node.Rule.Priority);
                                    _logger.LogDebug("    Server ApplName: {0}", node.Rule.serverApplName);
                                    _logger.LogDebug("    Server Enabled?: {0}", node.Rule.serverEnabled);
                                }
                                else
                                {
                                    _logger.LogDebug("    No rules found for this server!");
                                }
                                // server exists but rule is not enabled
                                _logger.LogInfo("Server: '{0}' stopping...", node.Key);
                                node.Server.Stop();
                                DisposeHelper.SafeDispose(ref node.Server);
                                DisposeHelper.SafeDispose(ref node.Logger);
                                _logger.LogInfo("Server: '{0}' stopped.", node.Key);
                            }
                        }
                        // publish result
                        PublishHostConfigResult(node, null);
                    }
                    catch (Exception e)
                    {
                        _logger.Log(e);
                        // publish result
                        PublishHostConfigResult(node, "Exception: " + e);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
        }
    }
}
