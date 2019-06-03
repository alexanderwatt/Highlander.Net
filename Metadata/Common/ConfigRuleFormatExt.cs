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
    public partial class ConfigRule : IRuleObject
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

        protected virtual NamedValueSet OnGetAppProperties() { return null; }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{OnGetPrivateKey()}";

        /// <summary>
        /// 
        /// </summary>
        public string NetworkKey => String.Format(NameSpace + ".Configuration.{0}.{1}", GetType().Name, PrivateKey);

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

    /// <summary>
    /// 
    /// </summary>
    public partial class HostConfigRule
    {
        /// <summary>
        /// 
        /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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
    /// <summary>
    /// 
    /// </summary>
    public partial class HostConfigResult : IRuleObject
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
                result.Set("ApplName", serverApplNameField);
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
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{serverApplNameField}.{serverInstanceField}";

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
