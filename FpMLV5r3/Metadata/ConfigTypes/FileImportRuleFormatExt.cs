using System;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    public enum RuleStatusEnum
    {
        Undefined,
        Disabled,
        Inactive,
        NotReady,
        Completed,
        Failed
    }

    public static class RuleConst
    {
        //public const string HostName = "HostName";
        //public const string Instance = "Instance";
        public const string RuleName = "RuleName";
        public const string FileName = "FileName";
        public const string RuleStatus = "RuleStatus";
        public const string ImportDelay = "ImportDelay";
        public const string LastImportResult = "LastImportResult";
        public const string LastImportException = "LastImportException";
        public const string LastImportDateTime = "LastImportDateTime";
        public const string EffectiveDateTime = "EffectiveDateTime";
        public const string LastCheckFailed = "LastCheckFailed";
        public const string LastCheckException = "LastCheckException";
        public const string LastCheckDateTime = "LastCheckDateTime";
        public const string FileContentGwml65 = "GWML_6_5";
        public const string FileContentGenericXML = "XML";
        public const string FileContentGenericText = "Text";
    }

    public partial class ImportRuleResult : IRuleObject
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
            return (other.RulePriority - RulePriority);
        }
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Set(RuleConst.RuleName, ruleNameField);
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{ruleNameField}";
        public bool IsTransient => false;
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }

    public partial class ImportFileResult : IRuleObject
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
                result.Set(RuleConst.RuleName, ruleNameField);
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{fileNameField.Replace('.', '_')}";
        public bool IsTransient => false;
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }

    public partial class ProcessFileResult : IRuleObject
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
            return (other.RulePriority - RulePriority);
        }
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, processResultField);
                return result;
            }
        }
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{fileNameField.Replace('.', '_')}";
        public bool IsTransient => false;
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }
}
