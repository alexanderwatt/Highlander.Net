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
using Orion.Util.NamedValues;

#endregion

namespace Metadata.Common
{
    public enum AlertStatus
    {
        Undefined,
        Disabled,
        Inactive,
        AllClear,
        Alerted
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class AlertRule : IRuleObject
    {
        public string EnvName => hostEnvNameField;

        /// <summary>
        /// 
        /// </summary>
        public string HostName => hostComputerField;

        /// <summary>
        /// 
        /// </summary>
        public string Instance => hostInstanceField;

        /// <summary>
        /// 
        /// </summary>
        public string UserName => hostUserNameField;

        /// <summary>
        /// 
        /// </summary>
        public int RulePriority => priorityField;

        /// <summary>
        /// 
        /// </summary>
        public bool RuleDisabled => disabledField;

        /// <summary>
        /// 
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IRuleObject other)
        {
            // descending (highest to lowest)
            return other.RulePriority - RulePriority;
        }

        /// <summary>
        /// 
        /// </summary>
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Set(RuleConst.RuleName, ruleNameField);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string NetworkKey => String.Format(NameSpace + ".Configuration.{0}.{1}", GetType().Name, PrivateKey);

        /// <summary>
        /// 
        /// </summary>
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{ruleNameField}";

        /// <summary>
        /// 
        /// </summary>
        public bool IsTransient => false;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class AlertSignal : IComparable<AlertSignal>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public int CompareTo(AlertSignal signal)
        {
            // ascending RuleName
            int result = string.Compare(RuleName, signal.RuleName, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            // ascending AlertServer
            result = String.Compare(AlertServer, signal.AlertServer, StringComparison.OrdinalIgnoreCase); // A -> Z
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ItemName =>
            $"Orion.Status.AlertSignal.{(ruleNameField ?? "(unknown)").Replace('.', '_')}.{(alertServerField ?? "(unknown)").Replace('.', '_')}";

        /// <summary>
        /// 
        /// </summary>
        public NamedValueSet ItemProps
        {
            get
            {
                var result = new NamedValueSet();
                result.Set("RuleName", ruleNameField ?? "(unknown)");
                result.Set("AlertServer", alertServerField ?? "(unknown)");
                result.Set("Status", statusField ?? "Undefined");
                return result;
            }
        }
    }
}
