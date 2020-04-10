using System;
using System.Collections.Generic;
using System.Text;
using Orion.Util.NamedValues;

namespace Orion.Configuration
{
    public enum AlertStatus
    {
        Undefined,
        Disabled,
        Inactive,
        AllClear,
        Alerted
    }

    public partial class AlertRule : IRuleObject
    {
        public string EnvName { get { return hostEnvNameField; } }
        public string HostName { get { return hostComputerField; } }
        public string Instance { get { return hostInstanceField; } }
        public string UserName { get { return hostUserNameField; } }
        public int RulePriority { get { return priorityField; } }
        public bool RuleDisabled { get { return disabledField; } }
        public int CompareTo(IRuleObject other)
        {
            // descending (highest to lowest)
            return (other.RulePriority - this.RulePriority);
        }
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Set(RuleConst.RuleName, ruleNameField);
                return result;
            }
        }
        public string NetworkKey { get { return String.Format("Orion.Configuration.{0}.{1}", this.GetType().Name, PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), ruleNameField); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

    public partial class AlertSignal : IComparable<AlertSignal>
    {
        public int CompareTo(AlertSignal signal)
        {
            int result;
            // ascending RuleName
            result = String.Compare(this.RuleName, signal.RuleName, true); // A -> Z
            if (result != 0)
                return result;

            // ascending AlertServer
            result = String.Compare(this.AlertServer, signal.AlertServer, true); // A -> Z
            return result;
        }
        public string ItemName
        {
            get { return String.Format("Orion.Status.AlertSignal.{0}.{1}", 
                (ruleNameField ?? "(unknown)").Replace('.', '_'), (alertServerField ?? "(unknown)").Replace('.','_')); }
        }
        public NamedValueSet ItemProps
        {
            get
            {
                NamedValueSet result = new NamedValueSet();
                result.Set("RuleName", (ruleNameField ?? "(unknown)"));
                result.Set("AlertServer", (alertServerField ?? "(unknown)"));
                result.Set("Status", (statusField ?? "Undefined"));
                return result;
            }
        }
    }
}
