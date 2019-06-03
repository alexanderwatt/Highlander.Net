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
using System.Collections.Generic;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;

#endregion

namespace Metadata.Common
{
    // rule interface
    /// <summary>
    /// 
    /// </summary>
    public interface IRuleObject : ICoreObject, IComparable<IRuleObject>
    {
        string EnvName {get;}
        string HostName { get; }
        string Instance { get; }
        string UserName { get; }
        int RulePriority { get; }
        bool RuleDisabled { get; }
        string NameSpace { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class RuleHelper
    {
        // constants
        public const string HostEnvName = "HostEnvName";
        public const string HostComputer = "HostComputer";
        public const string HostUserName = "HostUserName";
        public const string HostInstance = "HostInstance";
        public const string HostNameSpace = "HostNameSpace";

        /// <summary>
        /// Makes a rule filter.
        /// </summary>
        /// <param name="envAbbr">The (optional) name of the application processing the rules.</param>
        /// <param name="hostName">The (optional) name of the computer running the rules processor.</param>
        /// <param name="userName">The (optional) name of the user account hosting the rules processor.</param>
        /// <param name="instance">The (optional) instance name for the rule processor.</param>
        /// <param name="nameSpace">The (optional) instance namespace for the rule processor.</param>
        /// <returns></returns>
        public static IExpression MakeRuleFilter(
            string envAbbr,     // optional environment short name
            string hostName,    // optional name of computer
            string instance,    // optional instance name
            string userName,
            string nameSpace)    // optional name of the user
        {
            var clauses = new List<IExpression>();
            if (envAbbr != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(HostEnvName), Expr.IsEQU(HostEnvName, envAbbr)));
            if (hostName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(HostComputer), Expr.IsEQU(HostComputer, hostName)));
            clauses.Add(instance != null ? Expr.IsEQU(HostInstance, instance) : Expr.IsNull(HostInstance));
            if (userName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(HostUserName), Expr.IsEQU(HostUserName, userName)));
            if (nameSpace != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(HostNameSpace), Expr.IsEQU(HostNameSpace, nameSpace)));
            return Expr.BoolAND(clauses.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static NamedValueSet MakeRuleProps(IRuleObject rule)
        {
            var result = new NamedValueSet();
            result.Set(HostEnvName, rule.EnvName);
            result.Set(HostComputer, rule.HostName);
            result.Set(HostUserName, rule.UserName);
            result.Set(HostInstance, rule.Instance);
            result.Set(HostNameSpace, rule.NameSpace);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static string FormatRuleScope(IRuleObject rule)
        {
            return
                $"{rule.EnvName ?? "(all-envs)"}.{rule.HostName ?? "(all-host)"}.{rule.Instance ?? "(all-inst)"}.{rule.UserName ?? "(all-user)"}.{rule.NameSpace ?? "(all-name)"}";
        }
    }
}
