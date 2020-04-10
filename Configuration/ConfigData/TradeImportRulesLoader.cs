using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Util.Logging;

namespace Orion.Configuration
{
    public static class TradeImportRuleLoader
    {
        private static TradeImportRule MakeTradeImportRule(
            string ruleName, bool disabled,
            string includeCounterpartyIds)
        {
            return new TradeImportRule
                       {
                hostEnvName = null,
                hostComputer = null,
                hostInstance = null,
                hostUserName = null,
                RuleName = ruleName,
                Disabled = disabled,
                IncludeCounterpartyIds = includeCounterpartyIds
            };
        }

        public static void Load(ILogger logger, ICoreCache targetClient)
        {
            logger.LogDebug("Loading trade import rules...");
            // generate import rules
            var rules = new List<TradeImportRule>
                            {
                                MakeTradeImportRule(
                                    "TradeImporter",
                                    false, // rule enabled
                                    "13142,14859,126636,1637" // Barclays, Woolworths, Fix bill line, nab swaps
                                    )
                            };
            // build rules

            // save Import rules
            foreach (TradeImportRule rule in rules)
            {
                logger.LogDebug("  Loading {0} ...", rule.RuleName);
                TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(rule, lifetime);
            }

            logger.LogDebug("Loaded {0} trade import rules.", rules.Count);
        }
    }
}
