/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using Core.Common;
using Metadata.Common;
using Orion.Util.Logging;

namespace FpML.V5r10.ConfigData
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

        public static void Load(ILogger logger, ICoreCache targetClient, string nameSpace)
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
                rule.NameSpace = nameSpace;
                TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(rule, lifetime);
            }

            logger.LogDebug("Loaded {0} trade import rules.", rules.Count);
        }
    }
}
