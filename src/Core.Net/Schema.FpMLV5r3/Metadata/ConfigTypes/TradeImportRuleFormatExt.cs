using System;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    public partial class TradeImportRule : IRuleObject
    {
        public string EnvName { get { return hostEnvNameField; } }
        public string HostName { get { return hostComputerField; } }
        public string Instance { get { return hostInstanceField; } }
        public string UserName { get { return hostUserNameField; } }
        public int RulePriority { get { return priorityField; } }
        public bool RuleDisabled { get { return disabledField; } }
        public string NameSpace { get; set; }
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
                // other props
                return result;
            }
        }
        public string NetworkKey { get { return String.Format(NameSpace + ".Configuration.{0}.{1}", this.GetType().Name, PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), RuleName); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

}
