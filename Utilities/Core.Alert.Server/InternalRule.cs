/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

#endregion

namespace Core.Alert.Server
{
    public class InternalRule
    {
        // from AlertRule
        public string RuleName { get; }
        public bool Disabled { get; }
        public TimeSpan MonitorPeriod { get; }
        public TimeSpan PublishPeriod { get; }
        public string DataTypeName { get; }
        public IExpression DataSubsExpr { get; }
        public ItemKind DataItemKind { get; }
        public IExpression Constraint { get; }
        public IExpression Condition { get; }
        public NamedValueSet Properties { get; }
        public string SignalFormat { get; }

        // calculated
        public ISubscription DataSubs { get; set; }
        public ICoreItem PreviousReceivedItem { get; set; }
        public ICoreItem CurrentReceivedItem { get; set; }
        public AlertStatus LastStatus { get; set; }
        public InternalSignal LastSignal { get; set; }
        public DateTimeOffset LastMonitored { get; set; }
        public DateTimeOffset LastPublished { get; set; }

        public string UniqueKey => RuleName.Trim().ToLower();

        public InternalRule() { }
        public InternalRule(AlertRule rule)
        {
            RuleName = rule.RuleUniqueId;
            Disabled = rule.Disabled;
            DataTypeName = rule.DataTypeName;
            DataSubsExpr = rule.DataSubsExpr != null ? Expr.Create(rule.DataSubsExpr) : Expr.Const(false);
            DataItemKind = EnumHelper.Parse(rule.DataItemKind, true, ItemKind.Undefined);
            Condition = rule.Condition != null ? Expr.Create(rule.Condition) : Expr.Const(false);
            Constraint = rule.Constraint != null ? Expr.Create(rule.Constraint) : Expr.Const(true);
            Properties = rule.AlertProperties != null ? new NamedValueSet(rule.AlertProperties) : new NamedValueSet();
            MonitorPeriod = rule.MonitorPeriod != null ? TimeSpan.Parse(rule.MonitorPeriod) : TimeSpan.FromMinutes(1);
            PublishPeriod = rule.PublishPeriod != null ? TimeSpan.Parse(rule.PublishPeriod) : TimeSpan.FromMinutes(5);
            SignalFormat = rule.SignalFormat;
        }
    }
}
