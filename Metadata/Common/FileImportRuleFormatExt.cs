/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Metadata.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum RuleStatusEnum
    {
        Undefined,
        Disabled,
        Inactive,
        NotReady,
        Completed,
        Failed
    }

    /// <summary>
    /// 
    /// </summary>
    public static class RuleConst
    {
        public const string HostName = "HostName";
        public const string Instance = "Instance";
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

    /// <summary>
    /// 
    /// </summary>
    public partial class ImportRuleResult : IRuleObject
    {
        /// <summary>
        /// 
        /// </summary>
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
        public int RulePriority => 0;

        /// <summary>
        /// 
        /// </summary>
        public bool RuleDisabled => false;

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
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);

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
    public partial class ImportFileResult : IRuleObject
    {
        /// <summary>
        /// 
        /// </summary>
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
        public int RulePriority => 0;

        /// <summary>
        /// 
        /// </summary>
        public bool RuleDisabled => false;

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
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, importResultField);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);

        /// <summary>
        /// 
        /// </summary>
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{fileNameField.Replace('.', '_')}";

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
    public partial class ProcessFileResult : IRuleObject
    {
        /// <summary>
        /// 
        /// </summary>
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
        public int RulePriority => 0;

        /// <summary>
        /// 
        /// </summary>
        public bool RuleDisabled => false;

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
                result.Set(RuleConst.FileName, fileNameField);
                result.Set(RuleConst.RuleStatus, processResultField);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string NetworkKey => String.Format(NameSpace + ".Status.{0}.{1}", GetType().Name, PrivateKey);

        /// <summary>
        /// 
        /// </summary>
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{fileNameField.Replace('.', '_')}";

        /// <summary>
        /// 
        /// </summary>
        public bool IsTransient => false;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Lifetime => TimeSpan.MaxValue;
    }
}
