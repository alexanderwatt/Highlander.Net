/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using Metadata.Common;
using Orion.Util.Expressions;
using Orion.V5r3.Configuration;

#endregion

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stressId"></param>
        /// <param name="ruleId"></param>
        /// <param name="disabled"></param>
        /// <param name="priority"></param>
        /// <param name="filterExpr"></param>
        /// <param name="updateExpr"></param>
        public CachedStressRule(string stressId, string ruleId, bool disabled, int priority, IExpression filterExpr, IExpression updateExpr)
        {
            StressId = stressId;
            RuleId = ruleId;
            Disabled = disabled;
            Priority = priority;
            FilterExpr = filterExpr;
            UpdateExpr = updateExpr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public int CompareTo(CachedStressRule rule)
        {
            return (rule.Priority - Priority); // descending (highest to lowest)
        }
    }
}
