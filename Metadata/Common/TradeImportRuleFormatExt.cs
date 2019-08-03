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
using Orion.Util.NamedValues;

#endregion

namespace Metadata.Common
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TradeImportRule : IRuleObject
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

        /// <summary>
        /// 
        /// </summary>
        public NamedValueSet AppProperties
        {
            get
            {
                NamedValueSet result = RuleHelper.MakeRuleProps(this);
                // other props
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
        public string PrivateKey => $"{RuleHelper.FormatRuleScope(this)}.{RuleName}";

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
