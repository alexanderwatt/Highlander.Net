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

using System;
using System.Linq;
using System.Collections.Generic;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

namespace Orion.CurveEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class CachedScenarioRule : IComparable<CachedScenarioRule>
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string ScenarioId;
        /// <summary>
        /// 
        /// </summary>
        public readonly string RuleId;
        /// <summary>
        /// 
        /// </summary>
        public readonly int Priority;
        /// <summary>
        /// 
        /// </summary>
        public readonly IExpression FilterExpr;
        /// <summary>
        /// 
        /// </summary>
        public readonly string StressId;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenarioId"></param>
        /// <param name="ruleId"></param>
        /// <param name="priority"></param>
        /// <param name="filterExpr"></param>
        /// <param name="stressId"></param>
        public CachedScenarioRule(string scenarioId, string ruleId, int priority, IExpression filterExpr, string stressId)
        {
            ScenarioId = scenarioId;
            RuleId = ruleId;
            Priority = priority;
            FilterExpr = filterExpr;
            StressId = stressId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public int CompareTo(CachedScenarioRule rule)
        {
            return (rule.Priority - Priority); // descending (highest to lowest)
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenarioRules"></param>
        /// <param name="scenarioName"></param>
        /// <param name="curveProperties"></param>
        /// <returns></returns>
        public static string RunScenarioRules(
            IEnumerable<CachedScenarioRule> scenarioRules,
            string scenarioName,
            NamedValueSet curveProperties)
        {
            if (scenarioName == null)
                return null;
            return (from rule in scenarioRules
                    where (rule.ScenarioId.Equals(scenarioName, StringComparison.OrdinalIgnoreCase)) && Expr.CastTo(rule.FilterExpr.Evaluate(curveProperties), false)
                    select rule.StressId).FirstOrDefault();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CachedSummary
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string UniqueId;
        /// <summary>
        /// 
        /// </summary>
        public readonly NamedValueSet Properties;
        /// <summary>
        /// 
        /// </summary>
        public readonly ValuationReport Summary;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="properties"></param>
        /// <param name="summary"></param>
        public CachedSummary(string uniqueId, NamedValueSet properties, ValuationReport summary)
        {
            UniqueId = uniqueId;
            Properties = properties;
            Summary = summary;
        }
    }
}
