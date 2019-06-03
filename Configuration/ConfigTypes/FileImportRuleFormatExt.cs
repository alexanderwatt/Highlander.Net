using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Util.NamedValues;

namespace Orion.Configuration
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
        public const string FileContent_GWML_6_5 = "GWML_6_5";
        public const string FileContent_GenericXML = "XML";
        public const string FileContent_GenericText = "Text";
    }

    public partial class ImportRuleResult : IRuleObject
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
                result.Set(RuleConst.RuleName, ruleNameField);
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }
        public string NetworkKey { get { return String.Format("Orion.Status.{0}.{1}", this.GetType().Name, this.PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), ruleNameField); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

    public partial class ImportFileResult : IRuleObject
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
                result.Set(RuleConst.RuleName, ruleNameField);
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }
        public string NetworkKey { get { return String.Format("Orion.Status.{0}.{1}", this.GetType().Name, PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), fileNameField.Replace('.', '_')); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }

    public partial class ProcessFileResult : IRuleObject
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
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, processResultField);
                return result;
            }
        }
        public string NetworkKey { get { return String.Format("Orion.Status.{0}.{1}", this.GetType().Name, this.PrivateKey); } }
        public string PrivateKey { get { return String.Format("{0}.{1}", RuleHelper.FormatRuleScope(this), fileNameField.Replace('.', '_')); } }
        public bool IsTransient { get { return false; } }
        public TimeSpan Lifetime { get { return TimeSpan.MaxValue; } }
    }
}
