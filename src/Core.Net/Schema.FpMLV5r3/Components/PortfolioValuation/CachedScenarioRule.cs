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

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Workflow.PortfolioValuation.V5r3
{
    public class CachedScenarioRule : IComparable<CachedScenarioRule>
    {
        public readonly string ScenarioId;
        public readonly string RuleId;
        public readonly int Priority;
        public readonly IExpression FilterExpr;
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
            return rule.Priority - Priority; // descending (highest to lowest)
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
            if (curveProperties == null)
                return null;
            return (from rule in scenarioRules
                    where rule.ScenarioId.Equals(scenarioName, StringComparison.OrdinalIgnoreCase) && Expr.CastTo(rule.FilterExpr.Evaluate(curveProperties), false)
                    select rule.StressId).FirstOrDefault();
        }
    }

}
