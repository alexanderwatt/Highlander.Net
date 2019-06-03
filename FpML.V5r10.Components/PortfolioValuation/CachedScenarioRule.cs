using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;

namespace Orion.PortfolioValuation
{
    public class CachedScenarioRule : IComparable<CachedScenarioRule>
    {
        public readonly string ScenarioId;
        public readonly string RuleId;
        public readonly int Priority;
        public readonly IExpression FilterExpr;
        public readonly string StressId;
        public CachedScenarioRule(string scenarioId, string ruleId, int priority, IExpression filterExpr, string stressId)
        {
            ScenarioId = scenarioId;
            RuleId = ruleId;
            Priority = priority;
            FilterExpr = filterExpr;
            StressId = stressId;
        }
        public int CompareTo(CachedScenarioRule rule)
        {
            return rule.Priority - Priority; // descending (highest to lowest)
        }

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
                    where (rule.ScenarioId.Equals(scenarioName, StringComparison.OrdinalIgnoreCase)) && Expr.CastTo(rule.FilterExpr.Evaluate(curveProperties), false)
                    select rule.StressId).FirstOrDefault();
        }
    }

}
