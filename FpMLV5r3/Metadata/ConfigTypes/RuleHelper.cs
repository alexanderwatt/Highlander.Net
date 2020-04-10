using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    // rule interface
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

        public static string FormatRuleScope(IRuleObject rule) => String.Format("{0}.{1}.{2}.{3}.{4}",
                rule.EnvName ?? "(all-envs)",
                rule.HostName ?? "(all-host)",
                rule.Instance ?? "(all-inst)",
                rule.UserName ?? "(all-user)",
                rule.NameSpace ?? "(all-name)");
    }
}
