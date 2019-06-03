using System;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    public partial class ConfigRule : IRuleObject
    {
        public string EnvName => hostEnvNameField;
        public string HostName => hostComputerField;
        public string Instance => hostInstanceField;
        public string UserName => hostUserNameField;
        public int RulePriority => priorityField;
        public bool RuleDisabled => disabledField;
        public string NameSpace { get; set; }

        public int CompareTo(IRuleObject other)
        {
            // descending (highest to lowest)
            return (other.RulePriority - RulePriority);
        }
        protected virtual NamedValueSet OnGetAppProperties() { return null; }
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Add(OnGetAppProperties());
                return result;
            }
        }
        protected virtual string OnGetPrivateKey() { throw new NotImplementedException(); }
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{OnGetPrivateKey()}";
        public string NetworkKey => String.Format(NameSpace + ".Configuration.{0}.{1}", GetType().Name, PrivateKey);
        public bool IsTransient => false;
        public TimeSpan Lifetime => TimeSpan.MaxValue;
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
        public string BuildConfig => buildConfigField;

        protected override NamedValueSet OnGetAppProperties()
        {
            return new NamedValueSet(new NamedValue("ApplName", serverApplNameField));
        }
        protected override string OnGetPrivateKey()
        {
            return $"{buildConfigField ?? "(all-cfgs)"}.{serverApplNameField}";
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
        public string EnvName => hostEnvNameField;
        public string HostName => hostComputerField;
        public string Instance => hostInstanceField;
        public string UserName => hostUserNameField;
        public int RulePriority => 0;
        public bool RuleDisabled => false;
        public string NameSpace { get; set; }
        public int CompareTo(IRuleObject other)
        {
            // descending (highest to lowest)
            return other.RulePriority - RulePriority;
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
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{serverApplNameField}.{serverInstanceField}";
        public bool IsTransient => false;
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }

}
