using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Util.NamedValues;

namespace Orion.Configuration
{
    public partial class ConfigRule : IRuleObject
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
        protected virtual NamedValueSet OnGetAppProperties() { return null; }
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Add(this.OnGetAppProperties());
                return result;
            }
        }
        protected virtual string OnGetPrivateKey() { throw new NotImplementedException(); }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), this.OnGetPrivateKey()); } }
        public string NetworkKey { get { return String.Format("Orion.Configuration.{0}.{1}", this.GetType().Name, this.PrivateKey); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

    public partial class BridgeConfigRule
    {
        protected override NamedValueSet OnGetAppProperties()
        {
            return new NamedValueSet(new NamedValue("RuleName", ruleNameField));
        }
        protected override string OnGetPrivateKey()
        {
            return ruleNameField;
        }
    }

    public partial class HostConfigRule
    {
        public string BuildConfig { get { return buildConfigField; } }
        protected override NamedValueSet OnGetAppProperties()
        {
            return new NamedValueSet(new NamedValue("ApplName", serverApplNameField));
        }
        protected override string OnGetPrivateKey()
        {
            return String.Format("{0}.{1}", buildConfigField ?? "(all-cfgs)", serverApplNameField);
        }
    }

    public partial class FileImportRule
    {
        protected override NamedValueSet OnGetAppProperties()
        {
            return new NamedValueSet(new NamedValue(RuleConst.RuleName, ruleNameField));
        }
        protected override string OnGetPrivateKey()
        {
            return ruleNameField;
        }
    }

    // rule results

    public partial class HostConfigResult : IRuleObject
    {
        public string EnvName { get { return hostEnvNameField; } }
        public string HostName { get { return hostComputerField; } }
        public string Instance { get { return hostInstanceField; } }
        public string UserName { get { return hostUserNameField; } }
        public int RulePriority { get { return 0; } }
        public bool RuleDisabled { get { return false; } }
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
                result.Set("ApplName", serverApplNameField);
                return result;
            }
        }
        public string NetworkKey { get { return String.Format("Orion.Status.{0}.{1}", this.GetType().Name, PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}.{2}", RuleHelper.FormatRuleScope(this), serverApplNameField, serverInstanceField); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

}
