using System;
using Metadata.Common;
using Orion.Util.Expressions;

namespace Orion.Workflow.CurveGeneration
{
    public class CachedStressRule : IComparable<CachedStressRule>
    {
        public readonly string StressId;
        public readonly string RuleId;
        public readonly bool Disabled;
        public readonly int Priority;
        public readonly IExpression FilterExpr;
        public readonly IExpression UpdateExpr;
        public CachedStressRule(string stressId, string ruleId, bool disabled, int priority, IExpression filterExpr, IExpression updateExpr)
        {
            StressId = stressId;
            RuleId = ruleId;
            Disabled = disabled;
            Priority = priority;
            FilterExpr = filterExpr;
            UpdateExpr = updateExpr;
        }
        public CachedStressRule(StressRule rule)
        {
            StressId = rule.StressId;
            RuleId = rule.RuleId;
            Disabled = rule.Disabled;
            Priority = rule.Priority;
            if (rule.FilterExpr != null)
                FilterExpr = Expr.Create(rule.FilterExpr);
            if (rule.UpdateExpr != null)
                UpdateExpr = Expr.Create(rule.UpdateExpr);
        }
        public int CompareTo(CachedStressRule rule)
        {
            return (rule.Priority - Priority); // descending (highest to lowest)
        }
    }
}
